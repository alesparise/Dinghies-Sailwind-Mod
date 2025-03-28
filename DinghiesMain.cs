using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
//poorly written by pr0skynesis (discord username)

namespace Dinghies
{   /// <summary>
    /// Patchnotes (v.1.0.6)
    /// • Added stowing mechanic:
    ///     - Davits are available in the shipyard for the Sanbuq, Junk, Brig and Jong;
    ///     - Davits come with a tarp cover package. Grab it and clip it with the dinghy to cover it;
    ///     - Grab one of the hooks from the davits and click on the metal bits sticking out from the covered dinghy;
    ///     - When both hooks are connected, raise the dinghy with the winches that come with the davits;
    /// • Changed default rig when Shipyard Expansion is installed
    /// • Slightly increased rudder power
    /// • Added notification system:
    ///     - Opening the game will show a notification message if there is a new one;
    ///     - This can be disabled in the config file;
    ///     - Messages will let you know about new updates or bugfixes;
    /// • Made asset loader find assets relative to the .dll;
    /// • Added bridge and scripts assemblies;
    /// • Added a default name to the nameplate so that it's easier to find;
    /// • Changed namplate saving and loading to allow multiple boats having names (future proofing, might erase your old name)
    /// • Added check for the boat being bought before allowing the oars to be used
    /// • Added a pillow option that acts like a bed, requires the long seat to be installed
    /// • Added a centered tiller option, requires the mizzen mast to not be installed
    /// • Merged the two flags option into one and added check for the correct shrouds to the flag options
    /// 
    /// TODO: (later)
    /// • Experiment with automatic updates?
    /// • Integrated storage undre bow cover
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
        public const string pluginVersion = "1.0.6";    //1.0.6 was the stowing update
        public const string shortName = "pr0.dinghies";
        
        //config file info
        public static ConfigEntry<bool> nothingConfig;
        public static ConfigEntry<bool> invertedTillerConfig;
        public static ConfigEntry<KeyCode> leftFConfig;
        public static ConfigEntry<KeyCode> rightFConfig;
        public static ConfigEntry<KeyCode> leftBConfig;
        public static ConfigEntry<KeyCode> rightBConfig;
        public static ConfigEntry<bool> saveCleanerConfig;
        public static ConfigEntry<bool> notificationsConfig;
        public static ConfigEntry<string> lastNoteVer;

        public void Awake()
        {
            //Create config file in BepInEx\config\
            //A) General Settings
            nothingConfig = Config.Bind("A) General Settings", "nothing", true, "This setting does nothing. Default is true, set to false to disable.");
            //B) Rudder Settings
            invertedTillerConfig = Config.Bind("B) Rudder Settings", "Inverted Tiller", false, "Inverts the tiller control, e.g. press left to move right. Default is false, set to true to enable.");
            //C) Rowing Settings
            leftFConfig = Config.Bind("C) Rowing Settings", "Forward Left Button", KeyCode.A, "Controls the rowing forward of the left oar. Default is A. Use the Bepniex Configuration Manager for this!;");
            rightFConfig = Config.Bind("C) Rowing Settings", "Forward Right Button", KeyCode.D, "Controls the rowing forward of the right oar. Default is D. Use the Bepniex Configuration Manager for this!;");
            leftBConfig = Config.Bind("C) Rowing Settings", "Bakcward Left Button", KeyCode.Q, "Controls the rowing backward of the left oar. Default is Q. Use the Bepniex Configuration Manager for this!;");
            rightBConfig = Config.Bind("C) Rowing Settings", "Backward Right Button", KeyCode.E, "Controls the rowing backward of the right oar. Default is E. Use the Bepniex Configuration Manager for this!;");
            //D) Other Settings
            saveCleanerConfig = Config.Bind("D) Other Settings", "Save Cleaner", false, "Removes the saves dependency on this mod. Only use if you want to remove the mod from an ongoing save! Change to true (with the game closed), open the game → load the save → save → close the game → remove the mod → done. A save backup is recommended.");
            notificationsConfig = Config.Bind("D) Other Settings", "Notifications", true, "Enable this mod notifications on startup. Set to false to disable.");
            lastNoteVer = Config.Bind("D) Other Settings", "Last Note Version", "", "Saves the hash of the last notification. Only change this if you want to see the last notification again.");
            
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
                //load the dinghies and all assets
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
