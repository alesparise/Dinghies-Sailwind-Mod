using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DinghiesScripts
{
    /// <summary>
    /// Controls the stowing and launching
    /// </summary>
    public class Davits : MonoBehaviour
    {
        private const string key = "pr0.dinghies.stowing";

        public GameObject tarpPackage;
        public GameObject winch0;
        public GameObject winch1;

        private GameObject boat;
        public GameObject stowedBoat;

        public ConfigurableJoint hook0;
        public ConfigurableJoint hook1;
        
        private Rigidbody boatAnchor;

        private bool initialized = false;

        private static int toLoad;

        private List<Renderer> toHide;

        public static List<Davits> list;
        
        private static List<StowingSaver> savers;

        private StowingSaver saver;

        public void Awake()
        {
            if (list == null) list = new List<Davits>();
            list.Add(this);

            if (savers == null) savers = new List<StowingSaver>();

            saver = new StowingSaver();
        }
        public void FixedUpdate()
        {
            if (!initialized && GameState.playing)
            {   //initialise the plate

                winch0.SetActive(true);
                winch1.SetActive(true);

                if (winch0.transform.localScale.x > 0.8f)
                {
                    winch0.transform.localScale = new Vector3(0.7f, 0.7f, 0.6f);
                    winch1.transform.localScale = new Vector3(0.7f, 0.7f, 0.6f);
                }

                LoadStowed();
                initialized = true;
            }
        }
        public void Stow(GameObject boat, bool loading)
        {   //replaces the boat with the stowed version
            RegisterBoat(boat);

            Transform boatTransform = boat.transform;
            Transform stowedTransform = stowedBoat.transform;
            if(!loading)
            {
                stowedTransform.position = boatTransform.position;
                stowedTransform.rotation = Quaternion.Euler(boatTransform.eulerAngles.x, boatTransform.eulerAngles.y - 90f, boatTransform.eulerAngles.z);
            }
            else
            {
                stowedTransform.position = transform.position + transform.forward * 4f;
                stowedTransform.rotation = Quaternion.identity;
            }

            boatAnchor.isKinematic = true;
            boatAnchor.transform.parent = boatTransform;

            //boat.SetActive(false);
            boatTransform.position = new Vector3(boatTransform.position.x, boatTransform.position.y + 400f, boatTransform.position.z);
            boat.GetComponent<Rigidbody>().isKinematic = true;

            HideRenderers(boatTransform);

            stowedBoat.SetActive(true);
            tarpPackage.SetActive(false);

            SaveStowed();

            Debug.LogWarning("Davits: stowed");
        }
        public void Launch()
        {   //replaces the stowed version with the boat

            Transform boatTransform = boat.transform;
            Transform stowedTransform = stowedBoat.transform;
            boatTransform.position = stowedTransform.position;
            boatTransform.rotation = Quaternion.Euler(stowedTransform.eulerAngles.x, stowedTransform.eulerAngles.y + 90f, stowedTransform.eulerAngles.z);
            
            stowedBoat.SetActive(false);
            //boat.SetActive(true);
            boat.GetComponent<Rigidbody>().isKinematic = false;
            
            ShowRenderers(boatTransform);

            boatAnchor.isKinematic = false;
            boatAnchor.transform.parent = null;
            
            tarpPackage.SetActive(true);
            
            UnregisterBoat();
            UnsaveStowed();

            Debug.LogWarning("Davits: launched");
        }
        public void RegisterBoat(GameObject b)
        {
            boat = b;
            stowedBoat = boat.GetComponent<Dinghy>().stowedBoat;
            boatAnchor = boat.GetComponent<BoatMooringRopes>().anchor.gameObject.GetComponent<Rigidbody>();
        }
        private void UnregisterBoat()
        {
            boat = null;
            stowedBoat = null;
            boatAnchor = null;
        }
        private void LoadStowed()
        {
            if (GameState.modData.ContainsKey(key))
            {
                List<StowingSaver> list = StowingSaver.Unserialize(GameState.modData[key]);
                if (toLoad == 0)
                {
                    toLoad = list.Count;
                }
                foreach (StowingSaver ss in list)
                {
                    if (ss.davits == name)
                    {
                        Stow(GameObject.Find(ss.boat), true);
                        break;
                    }
                }
            }
        }
        private void SaveStowed()
        {
            saver.boat = boat.name;
            saver.davits = name;
            
            if (!savers.Contains(saver)) savers.Add(saver);
            
            GameState.modData[key] = StowingSaver.Serialize(savers);

            Debug.LogWarning("Saved stowed boat from " + name);
        }
        private void UnsaveStowed()
        {
            if (savers.Contains(saver)) savers.Remove(saver);

            if (savers.Count == 0) GameState.modData[key] = "";
            else GameState.modData[key] = StowingSaver.Serialize(savers);

            Debug.LogWarning("Unsaved stowed boat from " + name);
        }
        private void HideRenderers(Transform obj)
        {
            toHide = new List<Renderer>();
            foreach (Renderer renderer in obj.GetComponentsInChildren<Renderer>())
            {
                if (renderer.enabled == true)
                {
                    toHide.Add(renderer);
                    renderer.enabled = false;
                }
            }
        }
        private void ShowRenderers(Transform obj)
        {
            foreach (Renderer renderer in toHide)
            {
                renderer.enabled = true;
            }
        }

        internal class StowingSaver
        {
            public string davits;
            public string boat;

            public static string Serialize(List<StowingSaver> stowingSavers)
            {
                StringBuilder sb = new StringBuilder();
                foreach (StowingSaver ss in stowingSavers)
                {
                    sb.Append(ss.davits);   //davits_brig_0
                    sb.Append(':');         //davits_brig_0:
                    sb.Append(ss.boat);     //davits_brig_0:DNG Cutter (Clone)
                    sb.Append(';');         //davits_brig_0:DNG Cutter (Clone);
                }

                return sb.ToString();
            }
            public static List<StowingSaver> Unserialize(string s)
            {
                List<StowingSaver> stowingSavers = new List<StowingSaver>();
                string[] pairs = s.Split(';');
                foreach (string pair in pairs)
                {
                    if (pair == "") continue;

                    string[] split = pair.Split(':');
                    StowingSaver ss = new StowingSaver
                    {
                        davits = split[0],
                        boat = split[1]
                    };
                    stowingSavers.Add(ss);
                }

                return stowingSavers;
            }
        }
    }
}
