using UnityEngine;

namespace Dinghies
{   /// <summary>
    /// Controls the tiller behaviour
    /// </summary>
    public class TillerRudder : GoPointerButton
    {   
        private Rudder rudder;

        private HingeJoint hingeJoint;

        private AudioSource audio;

        public bool locked;
        private bool held;

        public float input;
        private float lastInput;
        private float rotationAngleLimit;
        private float volumeMult = 0.05f;
        private float mult = 0.025f;        //makes the rudder more or less responsive

        private void Awake()
        {
            rudder = transform.parent.GetComponent<Rudder>();
            hingeJoint = rudder.GetComponent<HingeJoint>();
            input = 0f;
            lastInput = 0f;
            rotationAngleLimit = hingeJoint.limits.max;
            audio = GetComponentInChildren<AudioSource>();
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

            if (locked)
            {
                Unlock();
            }
        }
        public override void OnUnactivate(GoPointer activatingPointer)
        {
            if (Settings.steeringWithMouse && activatingPointer.type == GoPointer.PointerType.crosshairMouse)
            {
                MouseLook.ToggleMouseLook(newState: true);
            }
        }
        private void ToggleLock()
        {
            if (!locked)
            {
                Lock();
            }
            else
            {
                Unlock();
            }
        }
        private void Lock()
        {
            if ((bool)stickyClickedBy)
            {
                UnStickyClick();
            }

            locked = true;
            Juicebox.juice.PlaySoundAt("lock unlock", base.transform.position, 0f, 0.66f, 0.88f);
        }
        private void Unlock()
        {
            locked = false;
            Juicebox.juice.PlaySoundAt("lock unlock", base.transform.position, 0f, 0.66f, 1f);
        }
        public override void ExtraLateUpdate()
        {   // controls the tiller and the rudder connected to it

            if ((bool)stickyClickedBy || isClicked)
            {   //when it's clicked we control it with the A and D keys
                if (stickyClickedBy.AltButtonDown())
                {
                    ToggleLock();
                }
                if (!locked)
                {
                    if (!held)
                    {   // increases 5 times damper and spring values so the tiller is more stable
                        held = true;
                        ChangeDamper(held);
                    }
                    int invert = DinghiesMain.invertedTillerConfig.Value ? -1 : 1;
                    input += stickyClickedBy.movement.GetKeyboardDelta().x * mult * invert;
                    if (stickyClickedBy.movement.GetKeyboardDelta().y != 0)
                    {   //this should detect pressing forward or backward buttons
                        input = 0;
                    }
                }
                ApplyRotationLimit();
                RotateRudder();
            }
            else if (locked)
            {   //keep applying the rotation if it's locked. Also the input does not get set to 0 if it's locked
                ApplyRotationLimit();
                RotateRudder();
            }
            else
            {   //not clicked and not locked, bring the spring values to vanilla ones
                if (held)
                {
                    held = false;
                    ChangeDamper(held);
                }
                //set the input value from the rudder angle so that it does not bounce when clicked
                input = rudder.currentAngle;
            }
            if ((bool)audio)
            {   //play creaking sound when moving rudder
                float num = Mathf.Abs(input - lastInput) / Time.deltaTime;
                audio.volume = Mathf.Lerp(audio.volume, num * volumeMult, Time.deltaTime * 3f);
            }
            lastInput = input;
        }
        private void RotateRudder()
        {   //old, taken from GPButtonSteeringWheel
            float num = input / rotationAngleLimit;
            JointSpring spring = hingeJoint.spring;
            spring.targetPosition = hingeJoint.limits.max * num;
            hingeJoint.spring = spring;
        }
        private void ApplyRotationLimit()
        {   //limits the input value betwee the min and max rotation limit
            if (input > rotationAngleLimit)
            {
                input = rotationAngleLimit;
            }

            if (input < 0f - rotationAngleLimit)
            {
                input = 0f - rotationAngleLimit;
            }
        }
        private void RotateRudderAlt()
        {   //alternative way to operate the rudder
            //this seemed better, but in hindsight the og one might work better. Will release it like this for now.
            //rudder.currentAngle = input; //DEBUG: It worked with this, add it back if it does not work anymore!
            JointSpring spring = hingeJoint.spring;
            spring.targetPosition = input;
            hingeJoint.spring = spring;
        }
        private void ChangeDamper(bool held)
        {
            if (held)
            {
                JointSpring spring = hingeJoint.spring;
                spring.spring = 250f;
                spring.damper = 50f;
                hingeJoint.spring = spring;
            }
            else
            {
                JointSpring spring = hingeJoint.spring;
                spring.spring = 50f;
                spring.damper = 10;
                hingeJoint.spring = spring;
            }
        }
    }
}
