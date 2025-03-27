using DinghiesScripts;
using UnityEngine;

namespace Dinghies
{
    public class Tarp : GoPointerButton
    {
        private GameObject stowedBoat;
        private ConfigurableJoint[] joints;

        public void Awake()
        {
            stowedBoat = transform.parent.gameObject;
            joints = transform.parent.GetComponents<ConfigurableJoint>();
        }
        public override void OnActivate()
        {   //when clicked, if no hooks are connected, and hands are empty, we launch
            if (joints[0].connectedBody != null || joints[1].connectedBody != null || isClickedBy?.pointer?.GetHeldItem() != null) return;
            else Launch();
        }
        private void Launch() => Davits.list.Find(d => d.stowedBoat == stowedBoat).Launch();
    }
}
