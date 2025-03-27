using UnityEngine;
using Dinghies;

namespace DinghiesBridge
{
    public class NotificationButtonBridge : MonoBehaviour
    {
        [Tooltip("Set to 0 for link button, set to 1 for ok button")]
        public int typeIndex;
        public string url;

        public void Awake()
        {
            gameObject.AddComponent<NotificationButton>().Init(typeIndex, url);
        }
    }
}
