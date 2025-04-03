using UnityEngine;

namespace Dinghies
{

    [RequireComponent(typeof(RopeEffect))]
    public class RopeControllerDavits : RopeController
    {
        private RopeEffect hookRope;

        private RopeEffect rope;

        public ConfigurableJoint joint;

        public float maxLength = 20f;

        private void Start()
        {
            rope = GetComponent<RopeEffect>();

            ConfigurableJoint[] joints = transform.parent.GetComponentsInChildren<ConfigurableJoint>();
            if (name.Contains("0")) joint = joints[0];
            else joint = joints[1];
        }
        public void Init(RopeEffect hr)
        {
            hookRope = hr;
        }

        private void Update()
        {
            SoftJointLimit linearLimit = joint.linearLimit;
            linearLimit.limit = Mathf.Lerp(0f, maxLength, currentLength);
            joint.linearLimit = linearLimit;
            float num = Vector3.Distance(base.transform.position, rope.attachment.position);
            if ((bool)hookRope)
            {
                num = Vector3.Distance(hookRope.transform.position, joint.transform.position);
            }

            float value = joint.linearLimit.limit - num;
            rope.currentRopeLength = Mathf.InverseLerp(0f, maxLength, value);
            if ((bool)hookRope)
            {
                hookRope.currentRopeLength = rope.currentRopeLength;
            }
        }
    }
}
