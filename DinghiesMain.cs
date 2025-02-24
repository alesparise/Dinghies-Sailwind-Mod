using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
//poorly written by pr0skynesis (discord username)

namespace Dinghies
{   /// <summary>
    /// Patchnotes (v.1.0.6)
    /// • Changed default rig when Shipyard Expansion is installed
    /// • Slightly increased rudder power
    /// 
    /// TODO:   (v1.0.6)
    /// • Refactor DinghiesMain.cs, separate patch classes and MatLib, check code for general improvements?
    /// • Stowing / Launching;
    /// • Add check for the oars so that they cannot be used when the boat is not purchased
    /// • Add in-game manual explaining custom features → moved to AESPA (Assets, Equipment, Shipyard and Portolan Add-on)
    /// • Add a message system that opens a window on game launch to make important communications
    /// • Add sleeping bag (a rollable bed) → moved to AESPA (Assets, Equipment, Shipyard and Portolan Add-on)
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
        public const string pluginVersion = "1.0.5";    //WIP version is 1.0.6
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
                //Patch to scale sails if Shipyard Expansion is installed
                MethodInfo original2b = AccessTools.Method(typeof(Mast), "AttachInitialSail");
                MethodInfo patch2b = AccessTools.Method(typeof(DinghiesPatches), "SailSetupPatch2");
                harmony.Patch(original2b, null, new HarmonyMethod(patch2b));

            }
            else
            {   //clean the save
                MethodInfo original3 = AccessTools.Method(typeof(SaveLoadManager), "LoadGame");
                MethodInfo patch3 = AccessTools.Method(typeof(SaveCleaner), "CleanSave");
                harmony.Patch(original3, new HarmonyMethod(patch3));
            }
        }
    }
}
