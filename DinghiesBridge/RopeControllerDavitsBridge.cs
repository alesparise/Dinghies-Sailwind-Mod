using UnityEngine;
using Dinghies;

namespace DinghiesBridge
{
    public class RopeControllerDavitsBridge : MonoBehaviour
    {
        public RopeEffect hookRope;

        public void Awake()
        {
            gameObject.AddComponent<RopeControllerDavits>().Init(hookRope);
        }
    }
}
