using UnityEngine;
using Dinghies;

namespace DinghiesBridge
{
    public class TillerBridge : MonoBehaviour
    {
        public Rudder rudder;
        public HingeJoint hingeJoint;
        public AudioSource audio;

        public void Awake()
        {   //adds the actual component and self destroys
            gameObject.AddComponent<TillerRudder>().Init(rudder, hingeJoint, audio);
            Destroy(this);
        }
    }
}
