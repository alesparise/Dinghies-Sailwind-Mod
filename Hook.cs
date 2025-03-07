using System;
using UnityEngine;

namespace Dinghies
{
    public class Hook : PickupableItem
    {
        private RopeControllerAnchor rope;

        private Transform parent;

        public Rigidbody boatBody;
        public Rigidbody rb;

        public StowingBrackets bracket;

        public void Awake()
        {
            rb = GetComponent<Rigidbody>();
            parent = transform.parent;
            if (parent.name == "block0")
            {
                rope = parent.parent.Find("controller0").GetComponent<RopeControllerAnchor>();
            }
            else
            {
                rope = parent.parent.Find("controller1").GetComponent<RopeControllerAnchor>();
            }
            RegisterBoat();
        }
        public override void OnPickup()
        {
            if (bracket != null)
            {
                bracket.DisconnectHook();
            }
            rope.currentLength = rope.maxLength;
            Debug.LogWarning("Hook: picked up");
        }
        public override void OnDrop()
        {
            rope.currentLength = GetDistance() / rope.maxLength;

            Debug.LogWarning("Hook: dropped");
        }
        public override void ExtraLateUpdate()
        {   //shows a red highlight if you are going too far
            float dist = GetDistance();
            if ((bool)held && dist > rope.maxLength * 0.8f)
            {
                enableRedOutline = true;
                if (dist >= rope.maxLength)
                {
                    OnDrop();
                    held.DropItem();
                }
            }
            else
            {
                enableRedOutline = false;
            }
        }

        private float GetDistance()
        {
            return Vector3.Distance(transform.position, parent.position);
        }
        private void RegisterBoat()
        {
            Transform tf = parent;
            while (tf != null)
            {
                Rigidbody rb = tf.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    boatBody = rb;
                    return;
                }
                tf = tf.parent;
            }
        }
    }
}
