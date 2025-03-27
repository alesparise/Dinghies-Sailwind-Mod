using UnityEngine;

namespace Dinghies
{
    public class Hook : PickupableItem
    {
        private RopeControllerAnchor rope;

        private Transform block;

        public Rigidbody rigidbody;

        public StowingBrackets bracket;

        private Quaternion initialRot;

        private Vector3 initialPos;

        public override void Start()
        {   //initialises the rigigidbody and sets its center of mass. Seems like it can only be done in Start()
            base.Start();
            rigidbody = GetComponent<Rigidbody>();
            rigidbody.centerOfMass = new Vector3(0f, 0f, -1f);
        }
        public void Init(RopeControllerAnchor r, Transform b)
        {   //initialises the Hook component, called in the bridge awake. Kinda like an Awake()
            block = b;
            rope = r;
            initialRot = transform.localRotation;
            initialPos = transform.localPosition;
        }
        public override void OnPickup()
        {   //if the hook is connected, we disconnect it. When picked up the length is set to max
            bracket?.DisconnectHook();
            rope.currentLength = rope.maxLength;
        }
        public override void OnDrop()
        {   //adjust the length based on distance from the block, resets rotation
            rope.currentLength = GetDistance() / rope.maxLength;
            transform.localRotation = initialRot;
        }
        public void OnDisable()
        {   //reset the hook to it's initial position before disabling
            ResetHook();
        }
        public override void ExtraLateUpdate()
        {   //shows a red highlight if you are going too far and drops the hook if you go further still

            if (!(bool)held)
            {
                enableRedOutline = false;
                return;
            }

            float dist = GetDistance();
            if (dist > rope.maxLength * 0.8f)
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
        {   //calculates the distance between the hook and the block on the davit
            return Vector3.Distance(transform.position, block.position);
        }
        private void ResetHook()
        {   //resets the Hook position and rope controller length
            transform.localPosition = initialPos;
            rope.currentLength = 0f;
        }
    }
}
