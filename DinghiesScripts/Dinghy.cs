using UnityEngine;

namespace DinghiesScripts
{
    public class Dinghy : MonoBehaviour
    {
        public GameObject stowedBoat;

        public void Init(GameObject sb)
        {
            stowedBoat = sb;
        }
    }
}
