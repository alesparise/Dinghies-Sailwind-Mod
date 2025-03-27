using HarmonyLib;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Dinghies
{   /// <summary>
    /// Cleans the save so you can remove the mod safely
    /// </summary>
    internal class SaveCleaner
    {
        [HarmonyPriority(300)]
        [HarmonyPrefix]
        private static void CleanSave(int backupIndex)
        {   //removes all references to the modded boat before the game loads
            Debug.LogWarning("Dinghies: CleanSave running");
            Debug.LogWarning("Dinghies: cleaning save " + SaveSlots.currentSlot);
            string path = ((backupIndex != 0) ? SaveSlots.GetBackupPath(SaveSlots.currentSlot, backupIndex) : SaveSlots.GetCurrentSavePath());
            SaveContainer saveContainer;
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (FileStream fileStream = File.Open(path, FileMode.Open))
            {   // Deserialize the save container from the file
                saveContainer = (SaveContainer)binaryFormatter.Deserialize(fileStream);
            }
            if (IndexManager.loadedIndexMap.Count == 0)
            {   //running the cleaner on a non-updated legacy save
                saveContainer.savedPrefabs.RemoveAll(x => x.itemParentObject == 130 || x.itemParentObject == 131 || x.itemParentObject == 132);
                saveContainer.savedObjects.RemoveAll(x => x.sceneIndex == 130 || x.sceneIndex == 131 || x.sceneIndex == 132);
            }
            else
            {   //running the cleaner in a 1.0.3+ save
                foreach (string boat in IndexManager.loadedIndexMap.Keys)
                {
                    if (boat == "DNG Cutter")
                    {   //in the case of the cutter we need to clean two extra spots for the ropes
                        saveContainer.savedPrefabs.RemoveAll(x => x.itemParentObject == IndexManager.loadedIndexMap[boat] || x.itemParentObject == IndexManager.loadedIndexMap[boat] + 1 || x.itemParentObject == IndexManager.loadedIndexMap[boat] + 2);
                        saveContainer.savedObjects.RemoveAll(x => x.sceneIndex == IndexManager.loadedIndexMap[boat] || x.sceneIndex == IndexManager.loadedIndexMap[boat] + 1 || x.sceneIndex == IndexManager.loadedIndexMap[boat] + 2);
                    }
                }
                foreach (SaveObjectData data in saveContainer.savedObjects)
                {
                    if (data.customization != null)
                    {
                        if (data.sceneIndex != 20 && data.sceneIndex != 50 && data.sceneIndex != 70 && data.sceneIndex != 80)
                        data.customization = CleanDavits(data.customization, data.sceneIndex);
                    }
                }
            }
            using (FileStream fileStream = File.Open(path, FileMode.Create))
            {
                binaryFormatter.Serialize(fileStream, saveContainer);
            }
            Debug.LogWarning("Dinghies: save cleaned...");
        }
        private static SaveBoatCustomizationData CleanDavits(SaveBoatCustomizationData custom, int index)
        {
            BoatCustomParts parts = SaveLoadManager.instance.GetCurrentObjects()[index].GetComponent<BoatCustomParts>();

            if (custom.partActiveOptions.Count > parts.availableParts.Count)
            {
                custom.partActiveOptions.RemoveRange(parts.availableParts.Count, custom.partActiveOptions.Count - parts.availableParts.Count);
            }
            for (int i = 0; i < custom.partActiveOptions.Count; i++)
            {
                if (custom.partActiveOptions[i] >= parts.availableParts[i].partOptions.Count)
                {
                    custom.partActiveOptions[i] = parts.availableParts[i].activeOption;
                }
            }

            return custom;
        }
    }
}
