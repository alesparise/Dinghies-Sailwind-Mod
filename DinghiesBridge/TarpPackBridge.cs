using UnityEngine;
using Dinghies;

namespace DinghiesBridge
{
    public class TarpPackBridge : MonoBehaviour
    {
        public Transform davits;    //to store the parent of the tarp package

        public void Awake()
        {   //adds the actual component and self destroys
            gameObject.AddComponent<TarpPack>().Init(davits);
            Destroy(this);
        }
    }
}