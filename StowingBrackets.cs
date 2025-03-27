using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Dinghies
{   ///<summary>
    ///Controls the metal bracket used to connect the hooks with the stowed boat model
    ///ISSUE:
    ///1) The bracket outline stays on after bein clicked.
    /// </summary>
    public class StowingBrackets : GoPointerButton
    {
        private const string key = DinghiesMain.shortName + ".stowingBrackets";

        private bool connected;
        private bool initialized;

        private static int toLoad;

        private ConfigurableJoint joint;

        private Hook hook;

        private static List<BracketSaver> bracketSavers;

        private BracketSaver saver;

        public void Init(ConfigurableJoint j)
        {
            joint = j;
            if (bracketSavers == null) bracketSavers = new List<BracketSaver>();
            saver = new BracketSaver();
        }
        public void FixedUpdate()
        {
            if (!initialized)
            {
                LoadBracket();
                initialized = true;
            }
            if (pointedAtBy?.GetHeldItem() == null)
            {
                ForceUnlook();
            }
        }
        public override void OnActivate()
        {
            Hook h = isClickedBy?.pointer?.GetHeldItem()?.GetComponent<Hook>();
            if (h != null)
            {
                ConnectHook(h);
            }
            else if (connected)
            {   
                connected = false;
                DisconnectHook();
            }
            ForceUnlook();
        }
        private void ConnectHook(Hook h)
        {
            hook = h;
            joint.connectedBody = hook.rigidbody;
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;
            hook.bracket = this;
            hook.OnDrop();
            hook.held?.DropItem();
            
            connected = true;

            if (toLoad == 0)
            {
                SaveBracket();
            }
            else
            {
                toLoad--;
            }
        }
        public void DisconnectHook()
        {
            joint.xMotion = ConfigurableJointMotion.Free;
            joint.yMotion = ConfigurableJointMotion.Free;
            joint.zMotion = ConfigurableJointMotion.Free;
            joint.connectedBody = null;
            hook.bracket = null;
            hook = null;
            connected = false;

            UnsaveBracket();
        }
        private void LoadBracket()
        {
            if (GameState.modData.ContainsKey(key))
            {
                List<BracketSaver> list = BracketSaver.Unserialize(GameState.modData[key]);
                if (toLoad == 0)
                {
                    toLoad = list.Count;
                }
                foreach (BracketSaver bs in list)
                {
                    if (bs.bracket == name)
                    {
                        hook = GameObject.Find(bs.hook).GetComponent<Hook>();
                        ConnectHook(hook);
                        break;
                    }
                }
            }
        }
        private void SaveBracket()
        {
            saver.bracket = name;
            saver.hook = hook.name;
            if (!bracketSavers.Contains(saver)) bracketSavers.Add(saver);

            GameState.modData[key] = BracketSaver.Serialize(bracketSavers);
        }
        private void UnsaveBracket()
        {
            if (bracketSavers.Contains(saver)) bracketSavers.Remove(saver);

            if (bracketSavers.Count == 0) GameState.modData[key] = "";
            else GameState.modData[key] = BracketSaver.Serialize(bracketSavers);
        }
        internal class BracketSaver
        {
            public string bracket;
            public string hook;

            public static string Serialize(List<BracketSaver> bracketSavers)
            {
                StringBuilder sb = new StringBuilder();
                foreach (BracketSaver bs in bracketSavers)
                {
                    sb.Append(bs.bracket);   //davits_brig_0
                    sb.Append(':');         //davits_brig_0:
                    sb.Append(bs.hook);     //davits_brig_0:DNG Cutter (Clone)
                    sb.Append(';');         //davits_brig_0:DNG Cutter (Clone);
                }

                return sb.ToString();
            }
            public static List<BracketSaver> Unserialize(string s)
            {
                List<BracketSaver> bracketSavers = new List<BracketSaver>();
                string[] pairs = s.Split(';');
                foreach (string pair in pairs)
                {
                    if (pair == "") continue;

                    string[] split = pair.Split(':');
                    BracketSaver bs = new BracketSaver
                    {
                        bracket = split[0],
                        hook = split[1]
                    };
                    bracketSavers.Add(bs);
                }

                return bracketSavers;
            }
        }
    }
}
