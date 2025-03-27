using UnityEngine;
using Dinghies;

namespace DinghiesBridge
{
    public class HookBridge : MonoBehaviour
    {
        public RopeControllerAnchor rope;

        public Transform block;

        public void Awake()
        {
            gameObject.AddComponent<Hook>().Init(rope, block);
            Destroy(this);
        }
    }
}
