﻿using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using cakeslice;
using HarmonyLib;
using OVR;
using UnityEngine;
using Object = UnityEngine.Object;
//poorly written by pr0skynesis (discord username)

namespace Dinghies
{   /// <summary>
    /// Patchnotes (v.1.0.5)
    /// • Fixed the crash;
    /// • Fixed anchor physics;  
    /// • Added textures
    /// 
    /// TODO:   (v1.0.6)
    /// • Refactor DinghiesMain.cs, separate patch classes and MatLib, check code for general improvements?
    /// • Stowing / Launching;
    /// • Add check for the oars so that they cannot be used when the boat is not purchased
    /// • Add in-game manual explaining custom features
    /// • Add a message system that opens a window on game launch to make important communications
    /// • Add sleeping bag (a rollable bed)
    /// • Texture
    /// 
    /// TODO: (later)
    /// • Experiment with automatic updates?
    /// • Mast unstepping, steppin?
    /// • Other dinghies
    /// 
    /// </summary>
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class DinghiesMain : BaseUnityPlugin
    {   
        // Necessary plugin info
        public const string pluginGuid = "pr0skynesis.dinghies";
        public const string pluginName = "Dinghies";
        public const string pluginVersion = "1.0.5";    //1.0.5 is the version where I fixed the crash and made the texture
        public const string shortName = "pr0.dinghies";
        
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

            //FindObjectsOfType crash prevention
            /*MethodInfo outlineOG1 = AccessTools.Method(typeof(Outline), "Awake");
            MethodInfo outlineP1 = AccessTools.Method(typeof(TypeManager), "AddOutline");
            harmony.Patch(outlineOG1, new HarmonyMethod(outlineP1));
            MethodInfo outlineOG2 = AccessTools.Method(typeof(Outline), "OnDisable");
            MethodInfo outlineP2 = AccessTools.Method(typeof(TypeManager), "RemoveOutline");
            harmony.Patch(outlineOG2, new HarmonyMethod(outlineP2));
            MethodInfo outlineOG3 = AccessTools.Method(typeof(OutlineEffect), "OnEnable");
            MethodInfo outlineP3 = AccessTools.Method(typeof(TypeManager), "OnEnablePatch");
            harmony.Patch(outlineOG3, new HarmonyMethod(outlineP3));
            */

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
        public static string bundlePath;
        public static AssetBundle bundle;
        public static Material harlequinMat;
        public static GameObject cutter;
        public static GameObject cutterEmbark;
        public static GameObject[] letters = new GameObject[26];

        // PATCHES
        [HarmonyPrefix]
        public static void StartPatch(FloatingOriginManager __instance)
        {   //this patches the FloatingOriginManager Start() method and does all the setup part
            SetupThings();

            IndexManager.AssignAvailableIndex(cutter);

            Transform shiftingWorld = __instance.transform;
            SetMooring(shiftingWorld);

            GameObject instantiatedCutter = Object.Instantiate(cutter, shiftingWorld);
            
            //SET UP WALK COL
            cutterEmbark = instantiatedCutter.transform.Find("WALK cutter").gameObject;
            cutterEmbark.transform.parent = GameObject.Find("walk cols").transform;

            //SET INITIAL POSITION
            Vector3 startPos = new Vector3(5691.12f, 0.3087376f, 38987.02f);
            SetRotationAndPosition(instantiatedCutter, -89.8f, startPos);
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

            //load letters meshes
            string lettersPath = "Assets/Dinghies/nameplate/letter";
            for (int i = 0; i < 26; i++)
            {   //load the letters
                letters[i] = bundle.LoadAsset<GameObject>(lettersPath + i + ".prefab");
            }

            //load harlequin texture (I'll delete this eventually)
            string harlequinPath = "Assets/Dinghies/Materials/cutterHarlequin.mat";
            harlequinMat = bundle.LoadAsset<Material>(harlequinPath);

            //ADD COMPONENTS
            Transform cutterT = cutter.transform;
            Transform cutterModel = cutter.transform.Find("cutterModel");

            //Set the region
            cutter.GetComponent<PurchasableBoat>().region = GameObject.Find("Region Medi").GetComponent<Region>();

            //Add tiller component
            cutterT.Find("cutterModel").Find("rudder").Find("rudder_tiller_cutter").gameObject.AddComponent<TillerRudder>();
            
            //Add oar components
            Transform oarLocks = cutterT.Find("cutterModel").Find("oars_locks");
            oarLocks.GetChild(0).GetChild(0).gameObject.AddComponent<Oar>();
            oarLocks.GetChild(1).GetChild(0).gameObject.AddComponent<Oar>();
            oarLocks.gameObject.AddComponent<OarLocks>();

            //add nameplate component
            Transform nameplateParent = cutterModel.Find("nameplates");
            nameplateParent.Find("nameplate_left").gameObject.AddComponent<Nameplate>();
            nameplateParent.Find("nameplate_right").gameObject.AddComponent<Nameplate>();

            //Fix materials
            MatLib.RegisterMaterials();
            cutterT.Find("WaterFoam").GetComponent<MeshRenderer>().sharedMaterial = MatLib.foam;
            cutterT.Find("WaterObjectInteractionSphereBack").GetComponent<MeshRenderer>().sharedMaterial = MatLib.objectInteraction;
            cutterT.Find("WaterObjectInteractionSphereFront").GetComponent<MeshRenderer>().sharedMaterial = MatLib.objectInteraction;
            cutterT.Find("overflow particles").GetComponent<Renderer>().sharedMaterial = MatLib.overflow;
            cutterT.Find("overflow particles (1)").GetComponent<Renderer>().sharedMaterial = MatLib.overflow;
            cutterModel.Find("mask").GetComponent<MeshRenderer>().sharedMaterial = MatLib.convexHull;
            cutterModel.Find("damage_water").GetComponent<MeshRenderer>().sharedMaterial = MatLib.water4;
            cutterModel.Find("mask_splash").GetComponent<MeshRenderer>().sharedMaterial = MatLib.mask;
            //MatLib.FixFlagsMaterials(cutterModel.Find("structure"));


            //Easter egg
            cutterModel.Find("easter_egg").gameObject.SetActive(false);
            if (DinghiesMain.nothingConfig.Value)
            {
                EasterEgg(cutterModel);
            }
        }
        public static void SetRotationAndPosition(GameObject boat, float yRot, Vector3 position)
        {   //set the initial rotation of the boat
            Transform t = boat.transform;
            t.eulerAngles = new Vector3 (0f, yRot, 0f);
            t.position = position;
        }
        public static void SetMooring(Transform shiftingWorld)
        {   //attach the initial mooring lines to the correct cleats in Fort Aestrin (for now)
            Transform fort = shiftingWorld.Find("island 15 M (Fort)");
            BoatMooringRopes mr = cutter.GetComponent<BoatMooringRopes>();
            mr.mooringFront = fort.Find("dock_mooring M").transform;
            mr.mooringBack = fort.Find("dock_mooring M (8)").transform;
        }
        public static void EasterEgg(Transform cutterModel)
        {   //does absolutely nothing on a specific or date range

            DateTime today = DateTime.Now;
            int month = today.Month;
            int day = today.Day;
            if ((month == 12 && day >= 8) || (month == 1 && day <= 6))
            {   //reindeer season
                GameObject easterEgg = cutterModel.Find("easter_egg").gameObject;
                easterEgg.SetActive(true);
            }
            if ((month == 2 && day >= 27) || (month == 3 && day <= 4))
                {   //from 27 of february to 4th of march it's harlequin time
                Debug.LogWarning("Dinghies: harlequin time!");
                GameObject hull = cutterModel.Find("hull").gameObject;
                Material[] mats = hull.GetComponent<MeshRenderer>().sharedMaterials;
                mats[1] = harlequinMat;
                hull.GetComponent<MeshRenderer>().sharedMaterials = mats;
                Debug.LogWarning("Dinghies: harlequin time enabled");
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
        //public static Material flags;

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
            //flags = new Material(Shader.Find("Ciconia Studio/Double Sided/Standard/Diffuse Bump"));
            //Color col = new Color32(0x93, 0x00, 0x00, 0xFF);
            //flags.color = col;
        }
        /*public static void FixFlagsMaterials(Transform structure)
        {
            structure.Find("main_mast").Find("main_mast_flag").GetComponent<Renderer>().sharedMaterial = flags;
            structure.Find("mizzen_mast").Find("mizzen_mast_flag").GetComponent<Renderer>().sharedMaterial = flags;
            structure.Find("mid_mast").Find("mid_mast_flag").GetComponent<Renderer>().sharedMaterial = flags;
            structure.Find("mid_mast").Find("shrouds_1").Find("shrouds_1_flag").GetComponent<Renderer>().sharedMaterial = flags;
            structure.Find("mid_mast").Find("shrouds_2").Find("shrouds_2_flag").GetComponent<Renderer>().sharedMaterial = flags;
        }*/
    }
}
