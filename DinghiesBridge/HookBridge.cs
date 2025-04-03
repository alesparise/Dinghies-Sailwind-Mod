using UnityEngine;
using Dinghies;

namespace DinghiesBridge
{
    public class HookBridge : MonoBehaviour
    {
        public Transform block;

        public void Awake()
        {
            gameObject.AddComponent<Hook>().Init(block);
            Destroy(this);
        }
    }
}
