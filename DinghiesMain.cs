using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;
//poorly written by pr0skynesis (discord username)

namespace Dinghies
{   /// <summary>
    /// Patch notes:    (v1.0.3)
    /// • Added SaveCleaner option;                                                             OK
    /// • Added some missing sound effects (rudder creaking, impact sounds, bow splash);        OK
    /// • Made cutter slightly more prone to sinking when heeling;                              OK
    /// • Slightly lowered speed;                                                               OK
    /// • Rudder: made the rudder more firm when held by the player;                            OK
    /// • Rudder: made the rudder slightly less powerful;                                       OK
    /// • Rudder: press forward or backward to center rudder;                                   OK
    /// • Added oars and rowing system;                                                         OK
    /// • Added eyes as a boat option;                                                          OK
    /// 
    /// BACKEND:
    /// • dynamic boat index manager
    /// • save cleaner option
    /// • automatically updates the legacy saves to the new system;
    /// • solved incompatibility with Le Requin;                                                OK
    /// • removed bundle manifest file as it caused confusion;
    /// </summary>
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    //[BepInDependency("Romance.LeRequin", BepInDependency.DependencyFlags.SoftDependency)]   //this bypasses the issue with the index but its not a fix!
    public class DinghiesMain : BaseUnityPlugin
    {
        // Necessary plugin info
        public const string pluginGuid = "pr0skynesis.dinghies";
        public const string pluginName = "Dinghies";
        public const string pluginVersion = "1.0.3";    //HAD TO INCREASE VERSION BECAUSE OF COURSE I MESSED UP THUNDERSTORE...

        //config file info
        public static ConfigEntry<bool> nothingConfig;
        public static ConfigEntry<bool> invertedTillerConfig;
        public static ConfigEntry<KeyCode> leftFConfig;
        public static ConfigEntry<KeyCode> rightFConfig;
        public static ConfigEntry<KeyCode> leftBConfig;
        public static ConfigEntry<KeyCode> rightBConfig;
        public static ConfigEntry<bool> saveCleanerConfig;

        public void Awake()
        {
            //Create config file in BepInEx\config\
            nothingConfig = Config.Bind("A) General Settings", "nothing", true, "This setting does nothing. Default is true, set to false to disable.");
            invertedTillerConfig = Config.Bind("B) Rudder Settings", "Inverted Tiller", false, "Inverts the tiller control, e.g. press left to move right. Default is false, set to true to enable.");
            leftFConfig = Config.Bind("C) Rowing Settings", "Forward Left Button", KeyCode.A, "Controls the rowing forward of the left oar. Default is A. Use the Bepniex Configuration Manager for this!;");
            rightFConfig = Config.Bind("C) Rowing Settings", "Forward Right Button", KeyCode.D, "Controls the rowing forward of the right oar. Default is D. Use the Bepniex Configuration Manager for this!;");
            leftBConfig = Config.Bind("C) Rowing Settings", "Bakcward Left Button", KeyCode.Q, "Controls the rowing backward of the left oar. Default is Q. Use the Bepniex Configuration Manager for this!;");
            rightBConfig = Config.Bind("C) Rowing Settings", "Backward Right Button", KeyCode.E, "Controls the rowing backward of the right oar. Default is E. Use the Bepniex Configuration Manager for this!;");
            saveCleanerConfig = Config.Bind("D) Other Settings", "Save Cleaner", false, "Removes the saves dependency on this mod. Only use if you want to remove the mod from an ongoing save! Change to true (with the game closed), open the game → load the save → save → close the game → remove the mod → done. A save backup is recommended.");

            //PATCHING
            Harmony harmony = new Harmony(pluginGuid);
            //patch to manage indexes
            MethodInfo original4 = AccessTools.Method(typeof(SaveLoadManager), "LoadGame");
            MethodInfo patch4 = AccessTools.Method(typeof(IndexManager), "Manager");
            harmony.Patch(original4, new HarmonyMethod(patch4));

            //save modded indexes
            MethodInfo original5 = AccessTools.Method(typeof(SaveLoadManager), "LoadModData");
            MethodInfo patch5 = AccessTools.Method(typeof(IndexManager), "SaveIndex");
            harmony.Patch(original5, new HarmonyMethod(patch5));

            //Save mod data on new game
            MethodInfo original6 = AccessTools.Method(typeof(StartMenu), "StartNewGame");
            MethodInfo patch6 = AccessTools.Method(typeof(IndexManager), "StartNewGamePatch");
            harmony.Patch(original6, new HarmonyMethod(patch6));

            //CONDITIONAL PATCHES
            if (!saveCleanerConfig.Value)
            {
                //load the boat and spawn it
                MethodInfo original = AccessTools.Method(typeof(FloatingOriginManager), "Start");
                MethodInfo patch = AccessTools.Method(typeof(DinghiesPatches), "StartPatch");
                harmony.Patch(original, new HarmonyMethod(patch));

                //patch to attach initial sails
                MethodInfo original2 = AccessTools.Method(typeof(Mast), "Start");
                MethodInfo patch2 = AccessTools.Method(typeof(DinghiesPatches), "SailSetupPatch");
                harmony.Patch(original2, new HarmonyMethod(patch2));
            }
            else
            {   //clean the save
                MethodInfo original3 = AccessTools.Method(typeof(SaveLoadManager), "LoadGame");
                MethodInfo patch3 = AccessTools.Method(typeof(SaveCleaner), "CleanSave");
                harmony.Patch(original3, new HarmonyMethod(patch3));
            }
        }
    }
    public class DinghiesPatches
    {
        // variables 
        public static AssetBundle bundle;
        public static string bundlePath;
        public static GameObject cutter;
        public static GameObject cutterEmbark;
        // PATCHES
        [HarmonyPrefix]
        public static void StartPatch(FloatingOriginManager __instance)
        {
            SetupThings();

            IndexManager.AssignAvailableIndex(cutter);

            Transform shiftingWorld = __instance.transform;
            SetMooring(cutter, shiftingWorld);
            cutter = Object.Instantiate(cutter, shiftingWorld);

            //SET UP WALK COL
            cutterEmbark = cutter.transform.Find("WALK cutter").gameObject;
            cutterEmbark.transform.parent = GameObject.Find("walk cols").transform;
            //RUDDER THINGS
            //cutter.transform.Find("cutterModel").Find("rudder").Find("rudder_tiller_cutter").gameObject.AddComponent<TillerRudder>();
            //SET INITIAL POSITION
            SetRotation(cutter, -89.8f);
        }
        [HarmonyPrefix]
        public static void SailSetupPatch(Mast __instance)
        {   // set the inital sail to attach them
            if (__instance.transform.parent.parent.name == "cutterModel")
            {   //if this is a mast on the cutter
                GameObject[] sails = PrefabsDirectory.instance.sails;
                GameObject mainSail = sails[45];
                GameObject mizzenSail = sails[3];
                if (__instance.name == "mizzen_mast")
                {
                    __instance.startSailPrefab = mizzenSail;
                }
                if (__instance.name == "main_mast")
                {
                    __instance.startSailPrefab = mainSail;
                }
            }
        }
        // HELPER METHODS
        public static void SetupThings()
        {   // loads the boat prefab
            bundlePath = Paths.PluginPath + "\\Dinghies\\dinghies";
            bundle = AssetBundle.LoadFromFile(bundlePath);
            if (bundle == null)
            {   // Maybe the user downloaded from thunderstore...
                bundlePath = Paths.PluginPath + $"\\Pr0SkyNesis-Dinghies-{DinghiesMain.pluginVersion}" + "\\dinghies";
                bundle = AssetBundle.LoadFromFile(bundlePath);
                if (bundle == null)
                {
                    bundlePath = Paths.PluginPath + $"\\Pr0SkyNesis-Dinghies" + "\\dinghies";
                    bundle = AssetBundle.LoadFromFile(bundlePath);
                    if (bundle == null)
                    {
                        Debug.LogError("Dinghies: Bundle not loaded! Did you place it in the correct folder?");
                    }
                }
            }
            string cutterPath = "Assets/Dinghies/BOAT Cutter (130).prefab";
            cutter = bundle.LoadAsset<GameObject>(cutterPath);

            Transform cutterT = cutter.transform;

            //Set the region
            cutter.GetComponent<PurchasableBoat>().region = GameObject.Find("Region Medi").GetComponent<Region>();

            //Add oar components
            cutterT.Find("cutterModel").Find("rudder").Find("rudder_tiller_cutter").gameObject.AddComponent<TillerRudder>();
            
            Transform oarLocks = cutterT.Find("cutterModel").Find("oars_locks");
            oarLocks.GetChild(0).GetChild(0).gameObject.AddComponent<Oar>();
            oarLocks.GetChild(1).GetChild(0).gameObject.AddComponent<Oar>();
            oarLocks.gameObject.AddComponent<OarLocks>();

            //Fix materials
            MatLib.RegisterMaterials();
            cutterT.Find("WaterFoam").GetComponent<MeshRenderer>().material = MatLib.foam;
            cutterT.Find("WaterObjectInteractionSphereBack").GetComponent<MeshRenderer>().material = MatLib.objectInteraction;
            cutterT.Find("WaterObjectInteractionSphereFront").GetComponent<MeshRenderer>().material = MatLib.objectInteraction;
            Transform cutterModel = cutter.transform.Find("cutterModel");
            cutterModel.Find("mask").GetComponent<MeshRenderer>().material = MatLib.convexHull;
            cutterModel.Find("damage_water").GetComponent<MeshRenderer>().material = MatLib.water4;
            cutterModel.Find("mask_splash").GetComponent<MeshRenderer>().material = MatLib.mask;
            cutterT.Find("overflow particles").GetComponent<Renderer>().material = MatLib.overflow;
            cutterT.Find("overflow particles (1)").GetComponent<Renderer>().material = MatLib.overflow;

            //Easter egg
            cutterModel.Find("easter_egg").gameObject.SetActive(false);
            if (DinghiesMain.nothingConfig.Value)
            {
                EasterEgg(cutter.transform.Find("cutterModel").Find("easter_egg").gameObject);
            }
        }
        public static void SetRotation(GameObject boat, float yRot)
        {   //set the initial rotation of the boat
            boat.transform.eulerAngles = new Vector3 (0f, yRot, 0f);
        }
        public static void SetMooring(GameObject boat, Transform shiftingWorld)
        {   //attach the initial mooring lines to the correct cleats
            Transform fort = shiftingWorld.Find("island 15 M (Fort)");
            cutter.GetComponent<BoatMooringRopes>().mooringFront = fort.Find("dock_mooring M").transform;
            cutter.GetComponent<BoatMooringRopes>().mooringBack = fort.Find("dock_mooring M (8)").transform;
        }
        public static void EasterEgg(GameObject easterEgg)
        {   //does absolutely nothing on a specific or date range

            DateTime today = DateTime.Now;
            int month = today.Month;
            int day = today.Day;
            if ((month == 12 && day >= 8) || (month == 1 && day <= 6))
            {
                easterEgg.SetActive(true);
            }
        }
    }
    public class MatLib
    {   // manages a material library to easily access and assign materials that are not correctly decompiled (e.g Crest materials)
        public static Material convexHull;
        public static Material foam;
        public static Material objectInteraction;
        public static Material water4;
        public static Material mask;
        public static Material overflow;

        public static void RegisterMaterials()
        {   //save the materials from the cog to the variables so that I can then easily assign them to the dinghy's gameobjects
            GameObject cog = GameObject.Find("BOAT medi small (40)");
            Transform mediSmall = cog.transform.Find("medi small");
            convexHull = mediSmall.Find("mask").GetComponent<MeshRenderer>().sharedMaterial;
            foam = cog.transform.Find("WaterFoam").GetComponent<MeshRenderer>().sharedMaterial;
            objectInteraction = cog.transform.Find("WaterObjectInteractionSphereBack").GetComponent<MeshRenderer>().sharedMaterial;
            water4 = mediSmall.Find("damage_water").GetComponent<MeshRenderer>().sharedMaterial;
            mask = mediSmall.Find("mask_splash").GetComponent<MeshRenderer>().sharedMaterial;
            overflow = cog.transform.Find("overflow particles").GetComponent<Renderer>().sharedMaterial;
        }
    }
}
