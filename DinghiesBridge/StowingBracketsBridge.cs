using UnityEngine;
using Dinghies;
using System.Collections.Generic;
using System.Text;

namespace DinghiesBridge
{
    public class StowingBracketsBridge : MonoBehaviour
    {
        public Transform stowedBoat;

        private void Awake()
        {
            ConfigurableJoint[] joints = stowedBoat.GetComponents<ConfigurableJoint>();
            ConfigurableJoint joint = name.Contains("0") ? joints[0] : joints[1];
            gameObject.AddComponent<StowingBrackets>().Init(joint);
            Destroy(this);
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
                    StowingSaver ns = new StowingSaver
                    {
                        davits = split[0],
                        boat = split[1]
                    };
                    stowingSavers.Add(ns);
                }

                return stowingSavers;
            }
        }
    }
}
