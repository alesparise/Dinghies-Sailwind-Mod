using System.Collections;
using UnityEngine;

namespace Dinghies
{   /// <summary>
    /// Controls the Oars and rowing mechanics
    /// </summary>
    public class Oar : GoPointerButton
    {
        private Oar leftOar;
        private Oar rightOar;

        public HingeJoint joint;
        public HingeJoint parentJoint;

        public static bool grabbed;
        public static bool used;
        public static bool firstTime;
        public bool rowing;
        private int isLeft;
        

        private Rigidbody rb;

        private Transform forcePoint;

        public Rudder rudder;

        public TillerRudder tiller;

        public OarLocks locks;

        private void Awake()
        {
            joint = GetComponent<HingeJoint>();
            parentJoint = transform.parent.GetComponent<HingeJoint>();
            if (name == "oar_left")
            {   //get this as leftOar
                leftOar = this;
                isLeft = 1;
                rightOar = transform.parent.parent.GetChild(1).GetComponentInChildren<Oar>();
                forcePoint = transform.parent.parent.GetChild(2);
            }
            else
            {   //get this as rightOar
                rightOar = this;
                isLeft = -1;
                leftOar = transform.parent.parent.GetChild(0).GetComponentInChildren<Oar>();
                forcePoint = transform.parent.parent.GetChild(3);
            }
            rb = transform.parent.parent.parent.parent.GetComponent<Rigidbody>();
            rudder = rb.transform.Find("cutterModel").Find("rudder").GetComponent<Rudder>();
            //tiller = rudder.transform.GetChild(0).GetComponent<TillerRudder>();
        }
        public override void OnActivate(GoPointer activatingPointer)
        {
            if (Settings.steeringWithMouse && activatingPointer.type == GoPointer.PointerType.crosshairMouse)
            {
                MouseLook.ToggleMouseLook(newState: false);
            }

            if (!Settings.steeringWithMouse && activatingPointer.type == GoPointer.PointerType.crosshairMouse)
            {
                StickyClick(activatingPointer);
            }
        }
        public override void OnUnactivate(GoPointer activatingPointer)
        {
            if (Settings.steeringWithMouse && activatingPointer.type == GoPointer.PointerType.crosshairMouse)
            {
                MouseLook.ToggleMouseLook(newState: true);
            }
        }
        public override void ExtraLateUpdate()
        {
            if (!firstTime)
            {
                locks.oarUp = true;
                locks.SetPosition(locks.oarUp);
                firstTime = true;
                return;
            }
            if (locks.oarUp)
            {   //if the oars are up we cannot move them
                return;
            }
            if ((bool)stickyClickedBy || isClicked)
            {
                leftOar.joint.useLimits = false;
                leftOar.joint.useSpring = true;
                rightOar.joint.useLimits = false;
                rightOar.joint.useSpring = true;
                used = true;
                if (tiller == null)
                {
                    tiller = rudder.transform.GetChild(0).GetComponent<TillerRudder>();
                }
                if (!tiller.locked) 
                {
                    rudder.rudderPower = 0f;
                }
                if (Input.GetKey(DinghiesMain.leftFConfig.Value))   //A
                {   //left forward

                    if (!leftOar.rowing)
                    {
                        StartCoroutine(leftOar.Row(1f));
                    }
                }
                if (Input.GetKey(DinghiesMain.rightFConfig.Value))  //D
                {   //right forward
                    if (!rightOar.rowing)
                    {
                        StartCoroutine(rightOar.Row(1f));
                    }
                }
                if (Input.GetKey(DinghiesMain.leftBConfig.Value))   //Q
                {   //left backward
                    if (!leftOar.rowing)
                    {
                        StartCoroutine(leftOar.Row(-1f));
                    }
                }
                if (Input.GetKey(DinghiesMain.rightBConfig.Value))  //E
                {   //right backward
                    if (!rightOar.rowing)
                    {
                        StartCoroutine(rightOar.Row(-1f));
                    }
                }
            }
            else if (used && !(bool)leftOar.stickyClickedBy && !(bool)rightOar.stickyClickedBy && !leftOar.isClicked && !rightOar.isClicked)
            {
                rudder.rudderPower = 10f;
                leftOar.ResetPos();
                rightOar.ResetPos();
            }
        }
        public IEnumerator Row(float dir)
        {
            rowing = true;
            JointSpring spring1 = joint.spring;
            JointSpring spring2 = parentJoint.spring;

            for (float t = 0f; t <= 1f; t += Time.deltaTime * 0.5f)
            {
                //Animate oar
                float heel = GetBoatHeeling() * isLeft;
                float angle = t * 2f * Mathf.PI;
           
                spring1.targetPosition = Mathf.Lerp(-10f + heel, 25f + heel, 0.5f + 0.5f * Mathf.Sin(angle));   //CONTROLS UP-DOWN
                joint.spring = spring1;

                spring2.targetPosition = Mathf.Lerp(35f * dir, -35f * dir, 0.5f + 0.5f * Mathf.Cos(angle));     //CONTROLS BACK-FORTH
                parentJoint.spring = spring2;

                //adding a check here for the value of t could be used to get perfectly timed strokes and give a speed bonus
                
                yield return new WaitForEndOfFrame();
            }

            AddOarForce(dir);
            rowing = false;
        }
        public void ResetPos()
        {   //reset the oar to a state where the oar is seemingly unpowered (wobbling and all that)
            JointSpring spring = joint.spring;
            JointSpring parentSpring = parentJoint.spring;
            JointLimits limits = joint.limits;

            spring.targetPosition = 0f;
            parentSpring.targetPosition = 0f;
            limits.max = 45f;

            rightOar.joint.spring = spring;
            leftOar.joint.spring = spring;
            rightOar.parentJoint.spring = parentSpring;
            leftOar.parentJoint.spring = parentSpring;
            rightOar.joint.limits = limits;
            leftOar.joint.limits = limits;

            joint.useLimits = true;
            joint.useSpring = false;

            used = false;
        }
        private void AddOarForce(float dir)
        {   //Apply force
            Juicebox.juice.PlaySoundAt("ug tank splash", transform.position, 2f, 1f);
            
            float lSpeed = rb.transform.InverseTransformDirection(rb.velocity).z;
            Vector3 forceDir = dir > 0f ? rb.transform.forward : -rb.transform.forward;
            float speed = rb.velocity.magnitude;
            float forceMag = Mathf.Lerp(250f, 0f, Mathf.Clamp(speed, 0f, 2f) / 2f);         //F = m * a => 500 * 0.5 = 250 (+100 for good measure);
            if (lSpeed * dir < 0f)
            {   //detects when rowing against the boat movement and applies a decent force to stop and turn.
                forceMag = 200f;
            }
            rb.AddForceAtPosition(forceDir * forceMag, forcePoint.position, ForceMode.Impulse);
        }
        private static float GetBoatHeeling()
        {   //returns boat heeling in degrees
            Transform currentShip = GameState.currentBoat;
            Vector3 boatUp = currentShip.transform.up;

            return Vector3.SignedAngle(boatUp, Vector3.up, -Vector3.forward);
        }
    }
}
