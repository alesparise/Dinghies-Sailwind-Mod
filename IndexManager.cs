using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Dinghies
{
    internal class IndexManager
    {
        private static bool updateSave;
        private static bool updateLegacySave;
        private static bool xebec;

        public static Dictionary<string, int> loadedIndexMap = new Dictionary<string, int>();
        public static Dictionary<string, int> indexMap = new Dictionary<string, int>();

        public static string loadedVersion;

        //PATCHES
        [HarmonyPriority(400)]  //this should make sure Manager() runs before SaveCleaner()
        [HarmonyPrefix]
        private static void Manager(int backupIndex)
        {   //runs the necessary methods in order when LoadGame() is called
            Debug.LogWarning("Dinghies: Manager running");
            string path = ((backupIndex != 0) ? SaveSlots.GetBackupPath(SaveSlots.currentSlot, backupIndex) : SaveSlots.GetCurrentSavePath());
            SaveContainer saveContainer;
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (FileStream fileStream = File.Open(path, FileMode.Open))
            {   //unpack the save to access the saveContainer
                saveContainer = (SaveContainer)binaryFormatter.Deserialize(fileStream);
            }
            LoadSavedIndexes(saveContainer);
            DetectXebec(saveContainer);
            ValidateIndexes();
            if (updateSave || updateLegacySave)
            {
                if (updateLegacySave)
                {
                    saveContainer = UpdateFromLegacy(saveContainer);
                }
                if (updateSave)
                {
                    saveContainer = UpdateSave(saveContainer);
                }
                //using (FileStream fileStream = File.Open(path, FileMode.Create))
                //{   //pack the savecontainer back into the save
                //    binaryFormatter.Serialize(fileStream, saveContainer);
                //}
            }
            using (FileStream fileStream = File.Open(path, FileMode.Create))
            {   //pack the savecontainer back into the save
                binaryFormatter.Serialize(fileStream, saveContainer);
            }
        }
        [HarmonyPostfix]
        public static void SaveIndex()
        {   //adds the boatName:sceneIndex data to the modData dictionary
            //this runs toward the end of the LoadGame() method (it patches LoadModData())

            if (!GameState.currentlyLoading)
            {
                return;
            }
            GameState.modData[DinghiesMain.pluginGuid] = "";
            SaveModData();
        }
        [HarmonyPostfix]
        public static void StartNewGamePatch()
        {   //we need to save the modData if this is a newgame!
            SaveModData();
        }

        // SAVE MANIPULATION
        public static void AssignAvailableIndex(GameObject boat)
        {   //assigns the first available index to the boat
            SaveableObject[] objs = SaveLoadManager.instance.GetCurrentObjects();
            int holeSize = GetHoleSize(boat);
            int index = FindHole(objs, holeSize);
            boat.GetComponent<SaveableObject>().sceneIndex = index;
            indexMap[boat.name] = index;
        }
        public static void LoadSavedIndexes(SaveContainer saveContainer)
        {   //Check if we have previously assigned indexes to modded items

            if (saveContainer.modData.ContainsKey(DinghiesMain.pluginGuid))
            {
                string data = saveContainer.modData[DinghiesMain.pluginGuid];
                string[] entries = data.Split(';');                         //entries is now an array like: ["boat:1","boat:2",...]
                for (int i = 0; i < entries.Length - 1; i++)
                {
                    string boatName = entries[i].Split(':')[0];             //itemName is a string like: "item1"
                    int sceneIndex = int.Parse(entries[i].Split(':')[1]);    //itemIndex is an int like: 1
                    loadedIndexMap[boatName] = sceneIndex;
                }
            }
            else
            {
                //Debug.LogWarning("Dinghies: BoatIndexManager: No mod data saved...");
            }
            if (saveContainer.modData.ContainsKey(DinghiesMain.shortName + ".version"))
            {
                loadedVersion = saveContainer.modData[DinghiesMain.shortName + ".version"];
                //check for version here if needed
            }
            else
            {   //no version saved means the save is a legacy version and that we need to update it
                updateLegacySave = true;
            }

        }
        private static void DetectXebec (SaveContainer saveContainer)
        {   //detects if the xebec mod was installed on this save
            if (saveContainer.savedObjects.Any(x => x.sceneIndex == 133) && updateLegacySave)
            {
                xebec = true;
            }
        }
        private static SaveContainer UpdateSave(SaveContainer saveContainer)
        {   //updates the indexes in the save if necessary

            Debug.LogWarning("Dinghies: updating save " + SaveSlots.currentSlot);
            foreach (string boat in loadedIndexMap.Keys)
            {   // change the parentIndex of saved items
                foreach (SavePrefabData prefab in saveContainer.savedPrefabs.Where(x => x != null && x.itemParentObject == loadedIndexMap[boat]))
                {
                    prefab.itemParentObject = indexMap[boat];
                }

                //change the boat indexes
                foreach (SaveObjectData savedBoat in saveContainer.savedObjects.Where(x => x != null && x.sceneIndex == loadedIndexMap[boat]))
                {
                    savedBoat.sceneIndex = indexMap[boat];
                }

                //updates the Shipyard Expansion saved data (important otherwise sail are loaded with their vanilla size!)
                if (saveContainer.modData.ContainsKey($"SEboatSails.{loadedIndexMap[boat]}"))
                {   
                    string seConfig = saveContainer.modData[$"SEboatSails.{loadedIndexMap[boat]}"];
                    saveContainer.modData.Remove($"SEboatSails.{loadedIndexMap[boat]}");
                    saveContainer.modData[$"SEboatSails.{indexMap[boat]}"] = seConfig;
                }
            }
            Debug.LogWarning("Dinghies: save updated...");
            return saveContainer;
        }
        public static SaveContainer UpdateFromLegacy(SaveContainer saveContainer)
        {   //updates froma a legacy save
            Debug.LogWarning("Dinghies: updating save " + SaveSlots.currentSlot + " from legacy version");
            foreach (string boat in indexMap.Keys)
            {   // change the parentIndex of saved items
                if (!xebec)
                {   // if the xebec was not installed, we update 131, 132, 133
                    foreach (SavePrefabData prefab in saveContainer.savedPrefabs.Where(x => x.itemParentObject == 130 || x.itemParentObject == 131 || x.itemParentObject == 132))
                    {
                        prefab.itemParentObject = indexMap[boat];
                    }
                }
                else
                {   // if the xebec was installed we update only 130
                    foreach (SavePrefabData prefab in saveContainer.savedPrefabs.Where(x => x.itemParentObject == 130))
                    {
                        prefab.itemParentObject = indexMap[boat];
                    }
                }
            }
            foreach (string boat in indexMap.Keys)
            {   //change the boat indexes
                if (!xebec)
                {   // if the xebec was not installed, we update 130, 131, 132
                    foreach (SaveObjectData savedBoat in saveContainer.savedObjects.Where(x => x.sceneIndex == 130 || x.sceneIndex == 131 || x.sceneIndex == 132))
                    {
                        if (savedBoat.sceneIndex == 130)
                        {   // cutter proper
                            savedBoat.sceneIndex = indexMap[boat];
                        }
                        else if (savedBoat.sceneIndex == 131)
                        {   // front mooring rope
                            savedBoat.sceneIndex = indexMap[boat] + 1;
                        }
                        else if (savedBoat.sceneIndex == 132)
                        {   // back mooring rope
                            savedBoat.sceneIndex = indexMap[boat] + 2;
                        }
                    }
                }
                else
                {   // if the xebec was installed we only update 130
                    foreach (SaveObjectData savedBoat in saveContainer.savedObjects.Where(x => x.sceneIndex == 130))
                    {
                        if (savedBoat.sceneIndex == 130)
                        {   // cutter proper
                            savedBoat.sceneIndex = indexMap[boat];
                        }
                    }
                }
            }
            if (saveContainer.modData.ContainsKey("SEboatSails.130"))
            {   //updates the Shipyard Expansion saved data (important otherwise sail are loaded with their vanilla size!)
                string seConfig = saveContainer.modData["SEboatSails.130"];
                saveContainer.modData.Remove("SEboatSails.130");
                saveContainer.modData[$"SEboatSails.{indexMap[indexMap.Keys.First()]}"] = seConfig;
            }
            Debug.LogWarning("Dinghies: save updated from legacy version...");
            return saveContainer;
        }
        
        //UTILITIES
        private static void SaveModData()
        {   //used by SaveIndex and StartNewGamePatch to save data to modData dictionary
            GameState.modData[DinghiesMain.pluginGuid] = "";
            foreach (string name in indexMap.Keys)
            {
                string entry = name.ToString() + ":" + indexMap[name].ToString() + ";"; //name:1;
                if (GameState.modData.ContainsKey(DinghiesMain.pluginGuid))
                {
                    GameState.modData[DinghiesMain.pluginGuid] += entry;
                }
                else
                {
                    GameState.modData[DinghiesMain.pluginGuid] = entry;
                }
            }
            //Add mod version informations
            GameState.modData[DinghiesMain.shortName + ".version"] = DinghiesMain.pluginVersion;
        }
        private static int GetHoleSize(GameObject boat)
        {   //calculates how many free spaces the mod needs in the SaveableObjects array
            return boat.GetComponent<BoatMooringRopes>().ropes.Length + 1;
        }
        public static int FindHole(SaveableObject[] array, int holeSize)
        {   //finds the first "hole" in the array of the correct size (for the boat and all ropes)
            for (int i = 1; i < array.Length - holeSize; i++)
            {   //iterate over the whole array
                if (array[i] == null)
                {   //found a null
                    for (int j = 0; j < holeSize; j++)
                    {   //iterate over the next holeSize things to see if they are all null
                        if (array[i + j] != null)
                        {   //if one isn't null, leave this loop
                            break;
                        }
                        else
                        {   //keep going
                            if (j == holeSize - 1)
                            {   //if this is the right size, return i
                                return i;
                            }
                        }
                    }
                }
            }
            return -1;
        }
        private static void ValidateIndexes()
        {   //goes through all the items in the indexMap and checks if the old indexes are still valid
            //if at least one index is no longer valid we set updateSave to true
            foreach (string boat in indexMap.Keys)
            {
                if (loadedIndexMap.ContainsKey(boat))
                {
                    if (loadedIndexMap[boat] == indexMap[boat])
                    {
                        //Debug.LogWarning($"IndexManager: {boat} index is still valid: {indexMap[boat]}");
                    }
                    else
                    {
                        //Debug.LogWarning($"IndexManager: {boat} index is no longer valid, update required!");
                        updateSave = true;
                    }
                }
                else
                {
                    //Debug.LogWarning($"IndexManager: {boat} was not on the list before...");
                }
            }
        }
    }
}
