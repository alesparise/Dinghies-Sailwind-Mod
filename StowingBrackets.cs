using System.Globalization;
using UnityEngine;

namespace Dinghies
{
    public class StowingBrackets : GoPointerButton
    {
        bool connected;

        private Transform shiftingWorld;
        private Transform stowedBoat;

        public Rigidbody boatBody;

        private Hook hook;

        private HingeJoint joint;

        public void Awake()
        {
            stowedBoat = transform.parent;
            shiftingWorld = stowedBoat.parent;

            boatBody = stowedBoat.GetComponent<Rigidbody>();

            HingeJoint[] joints = stowedBoat.GetComponents<HingeJoint>();
            
            if (name.Contains("0"))
            {
                joint = joints[0];
            }
            else
            {
                joint = joints[1];
            }
        }

        public override void OnActivate()
        {
            Hook h = isClickedBy.pointer.GetHeldItem().GetComponent<Hook>();
            if (isClickedBy.pointer.GetHeldItem() != null && h != null)
            {
                ConnectHook(h);
                Debug.LogWarning("StowingBrackets: clicked with hook");
            }
            else if (connected)
            {   
                connected = false;
                DisconnectHook();
            }
        }
        private void ConnectHook(Hook h)
        {
            hook = h;
            joint.connectedBody = hook.rb;
            hook.bracket = this;
            hook.OnDrop();
            hook.held.DropItem();
            
            connected = true;

            Debug.LogWarning("StowingBracket: Hook connected");
        }
        public void DisconnectHook()
        {
            joint.connectedBody = null;
            hook.bracket = null;
            hook = null;
            connected = false;
            
            Debug.LogWarning("StowingBracket: Hook disconnected");
        }
    }
}
