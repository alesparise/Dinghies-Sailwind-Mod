using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using ShipyardExpansion;
using System;
using System.Collections.Generic;
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
        public static GameObject notificationUI;
        public static GameObject stowedCutter;
        public static GameObject brigAssets;

        public static GameObject[] letters = new GameObject[26];
        

        // PATCHES
        [HarmonyPrefix]
        public static void StartPatch(FloatingOriginManager __instance)
        {   //this patches the FloatingOriginManager Start() method and does all the setup part
            SetupThings();

            IndexManager.AssignAvailableIndex(cutter);

            Transform shiftingWorld = __instance.transform;
            SetMooring(shiftingWorld);

            GameObject cutterInstance = Object.Instantiate(cutter, shiftingWorld);

            //SET UP WALK COL
            cutterEmbark = cutterInstance.transform.Find("WALK cutter").gameObject;
            cutterEmbark.transform.parent = GameObject.Find("walk cols").transform;

            //Set up stowing
            GameObject stowedCutterInstance = Object.Instantiate(stowedCutter, shiftingWorld);
            GameObject brig = GameObject.Find("BOAT medi medium (50)");
            AddBrigOptions(brig, brigAssets);

            //SET INITIAL POSITION
            Vector3 startPos = new Vector3(5691.12f, 0.3087376f, 38987.02f);
            SetRotationAndPosition(cutterInstance, -89.8f, startPos);
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

            //load stowing assets
            string stowedCutterPath = "Assets/Dinghies/stowedCutter.prefab";
            stowedCutter = bundle.LoadAsset<GameObject>(stowedCutterPath);
            string brigPath = "Assets/Dinghies/brig_stowing.prefab";
            brigAssets = bundle.LoadAsset<GameObject>(brigPath);

            //ADD COMPONENTS
            Transform cutterT = cutter.transform;
            Transform cutterModel = cutter.transform.Find("cutterModel");

            //Set the region
            cutter.GetComponent<PurchasableBoat>().region = GameObject.Find("Region Medi").GetComponent<Region>();

            //Add tiller component
            cutterModel.Find("rudder").Find("rudder_tiller_cutter").gameObject.AddComponent<TillerRudder>();

            //Add oar components
            Transform oarLocks = cutterModel.Find("oars_locks");
            oarLocks.GetChild(0).GetChild(0).gameObject.AddComponent<Oar>();
            oarLocks.GetChild(1).GetChild(0).gameObject.AddComponent<Oar>();
            oarLocks.gameObject.AddComponent<OarLocks>();

            //add stowing brackets component
            Transform stowedCutterModel = stowedCutter.transform;
            stowedCutterModel.Find("stowing_att_0").gameObject.AddComponent<StowingBrackets>();
            stowedCutterModel.Find("stowing_att_1").gameObject.AddComponent<StowingBrackets>();

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

        public static void AddBrigOptions(GameObject boat, GameObject prefab)
        {   //adds the parts to the brig
            //get brig references
            BoatCustomParts parts = boat.GetComponent<BoatCustomParts>();
            Rigidbody rigidbody = boat.GetComponent<Rigidbody>();
            BoatRefs refs = boat.GetComponent<BoatRefs>();
            Transform walkCol = refs.walkCol;
            Transform model = refs.boatModel;

            //instantiate the prefab and then set up the assets
            GameObject partInst = Object.Instantiate(prefab);
            Transform root = partInst.transform;
            Transform davitsTransform = root.Find("davits");
            Transform noDavits = root.Find("no_davits");
            Transform davitsWalk = root.Find("davitsWalk");
            Transform noDavitsWalk = root.Find("no_davits_walk");

            //Add the Davits component
            davitsTransform.gameObject.AddComponent<Davits>();

            //move and set parent as necessary
            davitsTransform.SetParent(model, false);
            noDavits.SetParent(model, false);
            davitsWalk.SetParent(walkCol, false);
            noDavitsWalk.SetParent(walkCol, false);

            //fix winch not finding the boat by enabling the winch now, fix the connected rigidbody for the hooks
            Transform block0 = davitsTransform.Find("block0");
            Transform block1 = davitsTransform.Find("block1");
            davitsTransform.Find("winch0").gameObject.SetActive(true);
            davitsTransform.Find("winch1").gameObject.SetActive(true);
            block0.Find("hook").GetComponent<ConfigurableJoint>().connectedBody = rigidbody;
            block1.Find("hook").GetComponent<ConfigurableJoint>().connectedBody = rigidbody;

            //add hooks components
            block0.Find("hook").gameObject.AddComponent<Hook>();
            block1.Find("hook").gameObject.AddComponent<Hook>();


            //create and add the boatPart
            BoatPart part = new BoatPart
            {
                category = 1,
                partOptions = new List<BoatPartOption>
                {
                    davitsTransform.GetComponent<BoatPartOption>(),
                    noDavits.GetComponent<BoatPartOption>()
                },
                activeOption = 0
            };
            
            parts.availableParts.Add(part);

            Debug.LogWarning("Dinghies: added davits to the brig");
        }
    }
}
