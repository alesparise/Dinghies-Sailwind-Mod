using BepInEx.Bootstrap;
using ShipyardExpansion;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

using Object = UnityEngine.Object;
using System.Linq;
using HarmonyLib;
using DinghiesScripts;

namespace Dinghies
{
    public class DinghiesPatches
    {
        // variables 
        public static string modFolder;
        public const string bridge = "DinghiesBridge.dll";      //the name of the .dll file containing the bridge components
        public const string scripts = "DinghiesScripts.dll";    //the name of the .dll file containing the scripts components
        
        public static AssetBundle bundle;

        public static GameObject cutter;
        public static GameObject cutterEmbark;
        public static GameObject notificationUI;
        public static GameObject stowedCutter;
        public static GameObject brigAssets;
        public static GameObject sanbuqAssets;
        public static GameObject junkAssets;
        public static GameObject jongAssets;

        public static GameObject[] letters = new GameObject[26];

        // PATCHES
        public static void StartPatch(FloatingOriginManager __instance)
        {   //this patches the FloatingOriginManager Start() method and does all the setup part

            //Stopwatch sw = new Stopwatch();
            //sw.Start();

            SetupThings();

            IndexManager.AssignAvailableIndex(cutter);

            Transform shiftingWorld = __instance.transform;
            SetMooring(shiftingWorld);

            GameObject cutterInstance = Object.Instantiate(cutter, shiftingWorld);

            //SET UP WALK COL
            cutterEmbark = cutterInstance.transform.Find("WALK cutter").gameObject;
            cutterEmbark.transform.parent = GameObject.Find("walk cols").transform;

            //Set up stowing
            //GameObject stowedCutterInstance = Object.Instantiate(stowedCutter, shiftingWorld);
            GameObject stowedCutterInstance = cutterInstance.transform.Find("stowedCutter").gameObject;
            stowedCutterInstance.transform.parent = shiftingWorld;
            stowedCutterInstance.SetActive(false);
            GameObject brig = GameObject.Find("BOAT medi medium (50)");
            GameObject sanbuq = GameObject.Find("BOAT dhow medium (20)");
            GameObject junk = GameObject.Find("BOAT junk medium (80)");
            GameObject jong = GameObject.Find("BOAT junk large (70)");

            AddDavitsOptions(brig, brigAssets, "brig");
            AddDavitsOptions(sanbuq, sanbuqAssets, "sanbuq");
            AddDavitsOptions(junk, junkAssets, "junk");
            AddDavitsOptions(jong, jongAssets, "jong");

            //SET INITIAL POSITION
            Vector3 startPos = new Vector3(5691.12f, 0.3087376f, 38987.02f);
            SetRotationAndPosition(cutterInstance, -89.8f, startPos);

            //sw.Stop();
            //Debug.LogWarning("Dinghies: SetUp took: " + sw.ElapsedMilliseconds + " ms");
            //LogStartupTime(sw.ElapsedMilliseconds);
        }
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
        public static bool ImpactPatch(Collision collision)
        {   //prevents waking up the player when the stowed dinghy collides with the boat
            if (collision?.collider?.name == "stowedCutter" || collision?.collider?.transform.parent?.name == "stowedCutter")
            {
                return false;
            }
            return true;
        }
        // HELPER METHODS
        public static void SetupThings()
        {   // loads all the mods stuff (assemblies and assets)

            //Get the folder of the mod's assembly
            modFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            //Load the scripts assembly where the bridge components are stored
            string bridgePath = Path.Combine(modFolder, bridge);
            string scriptsPath = Path.Combine(modFolder, scripts);
            if (File.Exists(bridgePath) && File.Exists(scriptsPath))
            {
                Assembly.LoadFrom(bridgePath);
                Assembly.LoadFrom(scriptsPath);
            }
            else
            {
                Debug.LogError("Dinghies: Couldn't load " + bridge + " and/or " + scripts + "!");
            }

            //Load bundle
            string bundlePath = Path.Combine(modFolder, "dinghies");
            bundle = AssetBundle.LoadFromFile(bundlePath);
            if (bundle == null)
            {
                Debug.LogError("Dinghies: Could not load the bundle!");
                return;
            }
            string cutterPath = "Assets/Dinghies/DNG Cutter.prefab";
            cutter = bundle.LoadAsset<GameObject>(cutterPath);

            //load letters meshes
            string lettersPath = "Assets/Dinghies/nameplate/letter";
            for (int i = 0; i < 26; i++)
            {   //load the letters
                letters[i] = bundle.LoadAsset<GameObject>(lettersPath + i + ".prefab");
            }

            //load notification UI
            if (DinghiesMain.notificationsConfig.Value)
            {
                string notificationPath = "Assets/Dinghies/notificationWindow.prefab";
                notificationUI = bundle.LoadAsset<GameObject>(notificationPath);
                GameObject window = Object.Instantiate(notificationUI);
            }

            //load stowing assets
            string brigPath = "Assets/Dinghies/brig_stowing.prefab";
            brigAssets = bundle.LoadAsset<GameObject>(brigPath);
            string sanbuqPath = "Assets/Dinghies/sanbuq_stowing.prefab";
            sanbuqAssets = bundle.LoadAsset<GameObject>(sanbuqPath);
            string junkPath = "Assets/Dinghies/junk_stowing.prefab";
            junkAssets = bundle.LoadAsset<GameObject>(junkPath);
            string jongPath = "Assets/Dinghies/jong_stowing.prefab";
            jongAssets = bundle.LoadAsset<GameObject>(jongPath);

            //ADD COMPONENTS
            Transform cutterT = cutter.transform;
            Transform cutterModel = cutter.transform.Find("cutterModel");

            //Set the region
            cutter.GetComponent<PurchasableBoat>().region = GameObject.Find("Region Medi").GetComponent<Region>();

            //Add oar components
            Transform oarLocks = cutterModel.Find("oars_locks");
            oarLocks.GetChild(0).GetChild(0).gameObject.AddComponent<Oar>();
            oarLocks.GetChild(1).GetChild(0).gameObject.AddComponent<Oar>();
            oarLocks.gameObject.AddComponent<OarLocks>();

            //Get the stowedCutter model
            stowedCutter = cutterT.Find("stowedCutter").gameObject;
            Transform stowedCutterModel = stowedCutter.transform;

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

            //DEBUG:
            //ShowWalkCols(); //this shows the WalkCols layer
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
        }
        public static void AddDavitsOptions(GameObject boat, GameObject prefab, string boatName)
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
            Transform davitsTransform = root.Find("davits_0_" + boatName);
            Transform noDavits = root.Find("no_davits");
            Transform davitsWalk = root.Find("davits_0_walk");
            Transform noDavitsWalk = root.Find("no_davits_walk");

            //move and set parent as necessary
            davitsTransform.SetParent(model, false);
            noDavits.SetParent(model, false);
            davitsWalk.SetParent(walkCol, false);
            noDavitsWalk.SetParent(walkCol, false);

            //fix the connected rigidbody for the hooks
            Transform hook0 = davitsTransform.Find("block0/davits_0_hook_0_" + boatName);
            Transform hook1 = davitsTransform.Find("block1/davits_0_hook_1_" + boatName);
            hook0.GetComponent<ConfigurableJoint>().connectedBody = rigidbody;
            hook1.GetComponent<ConfigurableJoint>().connectedBody = rigidbody;

            //add ropecontroller to ropes
            RopeControllerDavits rope0 = davitsTransform.Find("controller0").gameObject.GetComponent<RopeControllerDavits>();
            RopeControllerDavits rope1 = davitsTransform.Find("controller1").gameObject.GetComponent<RopeControllerDavits>();
            hook0.GetComponent<Hook>().rope = rope0;
            hook1.GetComponent<Hook>().rope = rope1;

            davitsTransform.Find("winch0").GetComponent<GPButtonRopeWinch>().rope = rope0;
            davitsTransform.Find("winch1").GetComponent<GPButtonRopeWinch>().rope = rope1;

            //create and add the boatPart
            BoatPart part = new BoatPart
            {
                category = 1,
                partOptions = new List<BoatPartOption>
                {
                    davitsTransform.GetComponent<BoatPartOption>(),
                    noDavits.GetComponent<BoatPartOption>()
                },
                activeOption = 1    //davits disabled by default
            };

            parts.availableParts.Add(part);
        }

        //DEBUG METHODS (This are not used in the released versions)
        public static void LogStartupTime(float timeInMs)
        {
            string path = Path.Combine(modFolder, "StartupTimes.txt");
            try
            {
                List<float> startupTimes = new List<float>();

                // Read existing times if the file exists
                if (File.Exists(path))
                {
                    string[] lines = File.ReadAllLines(path);

                    // Skip the first line (it contains the average)
                    foreach (string line in lines.Skip(1))
                    {
                        if (line.Contains("Startup #"))
                        {
                            string[] parts = line.Split(new[] { ":" }, StringSplitOptions.None);
                            if (parts.Length > 1 && float.TryParse(parts[1].Replace("ms", "").Trim(), out float time))
                            {
                                startupTimes.Add(time);
                            }
                        }
                    }
                }

                // Add the new startup time
                startupTimes.Add(timeInMs);

                // Calculate the new average
                float averageTime = startupTimes.Average();

                // Prepare new log contents
                List<string> logContents = new List<string>
            {
                $"Average Startup Time: {averageTime:F2} ms" // First line with the average
            };

                // Add all previous log entries + new one
                int i = 0;
                foreach (float time in startupTimes)
                {
                    logContents.Add($"Startup #{i}: {time} ms");
                    i++;
                }

                // Write back to the file
                File.WriteAllLines(path, logContents);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to write log: {e.Message}");
            }
        }
        public static void ShowWalkCols()
        {   //debug method to show the walk cols
            
            Camera.main.cullingMask |= 1 << LayerMask.NameToLayer("WalkCols");
        }
    }
}
