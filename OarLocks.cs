using UnityEngine;

namespace Dinghies
{   /// <summary>
    /// Controls the OarLocks, which enable and disable the oars.
    /// </summary>
    public class OarLocks : GoPointerButton
    {
        public bool oarUp;

        private Oar leftOar;
        private Oar rightOar;

        private float min1 = -3f;
        private float max1 = -2.9f;
        private float min2 = 94.5f;
        private float max2 = 95f;

        private void Awake()
        { 
            leftOar = transform.GetChild(0).GetChild(0).GetComponent<Oar>();
            rightOar = transform.GetChild(1).GetChild(0).GetComponent<Oar>();
            leftOar.locks = this;
            rightOar.locks = this;
        }
        public override void OnActivate()
        {
            if (!oarUp)
            {
                oarUp = true;
                SetPosition(oarUp);
                Juicebox.juice.PlaySoundAt("lock unlock", transform.position, 0f, 0.66f, 0.88f);
            }
            else
            {
                oarUp = false;
                SetPosition(oarUp);
                Juicebox.juice.PlaySoundAt("lock unlock", transform.position, 0f, 0.66f, 0.88f);
            }
        }
        public void SetPosition(bool up)
        {   //fix the oars in their up position if up is true, else let them loose
            if (up)
            {
                JointLimits limits = leftOar.joint.limits;
                JointLimits parentLimits = leftOar.parentJoint.limits;

                limits.min = min1;
                limits.max = max1;
                parentLimits.min = min2;
                parentLimits.max = max2;

                leftOar.joint.limits = limits;
                rightOar.joint.limits = limits;
                leftOar.parentJoint.limits = parentLimits;
                rightOar.parentJoint.limits = parentLimits;

                leftOar.joint.useLimits = true;
                rightOar.joint.useLimits = true;
                leftOar.parentJoint.useLimits = true;
                rightOar.parentJoint.useLimits = true;

                leftOar.joint.useSpring = false;
                rightOar.joint.useSpring = false;
                leftOar.parentJoint.useSpring = false;
                rightOar.parentJoint.useSpring = false;
            }
            else
            {
                leftOar.joint.useLimits = false;
                rightOar.joint.useLimits = false;
                leftOar.parentJoint.useLimits = false;
                rightOar.parentJoint.useLimits = false;

                leftOar.joint.useSpring = true;
                rightOar.joint.useSpring = true;
                leftOar.parentJoint.useSpring = true;
                rightOar.parentJoint.useSpring = true;
            }
        }
    }
}
