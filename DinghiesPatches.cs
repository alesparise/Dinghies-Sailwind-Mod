using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using ShipyardExpansion;
using System;
using UnityEngine;

using Object = UnityEngine.Object;

namespace Dinghies
{
    public class DinghiesPatches
    {
        // variables 
        public static string bundlePath;
        public static AssetBundle bundle;
        public static Material harlequinMat;
        public static GameObject cutter;
        public static GameObject cutterEmbark;
        public static GameObject[] letters = new GameObject[26];
        public static GameObject notificationUI;

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
                GameObject mainSail = new GameObject();
                GameObject mizzenSail = new GameObject();
                if (Chainloader.PluginInfos.ContainsKey("com.nandbrew.shipyardexpansion"))
                {   //use lug sails from Shipyard Expansion if it's installed

                    mainSail = sails[158];    //scale: 0.7447
                    mizzenSail = sails[156];  //scale: 0.5250

                    if (__instance.name == "mizzen_mast")
                    {
                        __instance.startSailPrefab = mizzenSail;
                        //__instance.startSailPrefab.GetComponent<SailScaler>().SetScaleRel(0.5250f);
                    }
                    if (__instance.name == "main_mast")
                    {
                        __instance.startSailPrefab = mainSail;
                        //__instance.startSailPrefab.GetComponent<SailScaler>().SetScaleRel(0.7447f);
                    }
                }
                else
                {   //vanilla initial sails
                    mainSail = sails[45];
                    mizzenSail = sails[3];
                    if (__instance.name == "mizzen_mast") __instance.startSailPrefab = mizzenSail;
                    if (__instance.name == "main_mast") __instance.startSailPrefab = mainSail;
                }
            }
        }
        public static void SailSetupPatch2(Mast __instance)
        {   //fixes the default rig's scale and install height
            if (__instance.transform.parent.parent.name == "cutterModel" && Chainloader.PluginInfos.ContainsKey("com.nandbrew.shipyardexpansion"))
            {
                if (__instance.name == "mizzen_mast")
                {
                    __instance.GetComponentInChildren<SailScaler>().SetScaleRel(0.5250f);
                    Sail sail = __instance.GetComponentInChildren<Sail>();
                    sail.ChangeInstallHeight(-1f);    //starts at 3
                    sail.UpdateInstallPosition();
                    
                }
                if (__instance.name == "main_mast")
                {
                    __instance.GetComponentInChildren<SailScaler>().SetScaleRel(0.7447f);
                    Sail sail = __instance.GetComponentInChildren<Sail>();
                    sail.ChangeInstallHeight(0.3f);    //starts at 4.2
                    sail.UpdateInstallPosition();
                    
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

            //load notification UI
            if (DinghiesMain.notificationsConfig.Value)
            {
                string notificationPath = "Assets/Dinghies/notificationWindow.prefab";
                notificationUI = bundle.LoadAsset<GameObject>(notificationPath);
                notificationUI.AddComponent<NotificationManager>();
                GameObject window = Object.Instantiate(notificationUI);
            }

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
            t.eulerAngles = new Vector3(0f, yRot, 0f);
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
}
