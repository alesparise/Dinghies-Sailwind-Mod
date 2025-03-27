using System.Collections;
using UnityEngine;
using DinghiesScripts;
using System.Text;

namespace Dinghies
{
    /// <summary>
    /// Controls the packed tarp item
    /// </summary>
    public class TarpPack : PickupableItem
    {
        private Vector3 storedPosition; // = new Vector3 (0.532f, 1.909f, -0.442f);
        private Quaternion storedRotation; // = Quaternion.Euler(96.95898f, -90f, -90f);

        private Transform holder;
        private Transform trans;

        private Davits davits;

        public float maxDist = 50f;
        
        public void Init(Transform h)
        {   //initialises the TarpPack component, called in the bridge awake. Kinda like an Awake()
            holder = h;
            davits = holder.GetComponent<Davits>();
            trans = transform;
            
            description = "tarp cover";

            storedPosition = trans.localPosition;
            storedRotation = trans.localRotation;
        }
        public override void OnDrop()
        {
            StartCoroutine(ReturnToPosition());
        }
        public void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.GetComponent<Dinghy>())
            {   //check if the trigger is a dinghy
                if (GameState.currentBoat?.parent != other.transform)
                {   //check if the player is on the boat
                    davits.Stow(other.gameObject, false);
                    trans.localPosition = storedPosition;
                    trans.localRotation = storedRotation;
                    OnDrop();
                    held.DropItem();
                    gameObject.SetActive(false);
                }
            }
        }
        public override void ExtraLateUpdate()
        {   //Adds the red outline if you go too far and drops the package if you go further
            if (!(bool)held) 
            {
                enableRedOutline = false;
                return;
            }

            float dist = GetDistance();
            if (dist > maxDist * 0.8f)
            {
                enableRedOutline = true;
                if (dist >= maxDist)
                {
                    OnDrop();
                    held.DropItem();
                }
            }
            else
            {
                enableRedOutline = false;
            }
        }
        private IEnumerator ReturnToPosition()
        {
            Vector3 startPos = trans.localPosition;
            Quaternion startRot = trans.localRotation;

            Vector3 targetPos = holder.TransformPoint(storedPosition);
            Quaternion targetRot = holder.rotation * storedRotation;

            for (float t = 0f; t < 1f; t += Time.deltaTime)
            {
                trans.localPosition = Vector3.Lerp(startPos, storedPosition, t);
                trans.localRotation = Quaternion.Lerp(startRot, storedRotation, t);
                yield return new WaitForEndOfFrame();
            }
            trans.localPosition = storedPosition;
            trans.localRotation = storedRotation;
        }
        private float GetDistance()
        {
            return Vector3.Distance(storedPosition, trans.localPosition);
        }
    }
}
