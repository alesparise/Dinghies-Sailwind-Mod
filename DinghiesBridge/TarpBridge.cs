using UnityEngine;
using Dinghies;

namespace DinghiesBridge
{
    public class TarpBridge : MonoBehaviour
    {
        public void Awake()
        {
            gameObject.AddComponent<Tarp>();
            Destroy(this);
        }
    }
}
