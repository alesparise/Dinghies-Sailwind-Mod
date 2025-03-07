using UnityEngine;
using Crest;

namespace Dinghies
{
    /// <summary>
    /// Controls the dinghy specific mechanics like stowing and launching
    /// I think disabling the physics is not the way to go.
    /// Replacing the GameObject might be better.
    /// Actually, if I disable the BoatProbes and BoatAlignNormal components,
    /// only when the dinghy is attached to the parent boat then the rigidbody could still be useful.
    /// 
    /// Main issue rn is that we'd need to address the walk collider. 
    /// It might be better to disable the dinghy and replace it with a simplified object version.
    /// The main issue with this is the following: 
    ///     - The code below does seem to work fine as an object replacer, with some adjustments I don't think it would look too bad.
    /// STEPS:
    /// • Object replacer (boat ↔ stowedBoat)                   DONE (To be improved)
    /// • Add davits part                                       DONE
    /// • TODO How to connect the boat to the parent boat?
    ///     - metal hooks at the end of the davits ropes, pick up like mooring ropes and connect them to the boat
    /// • TODO Hoisting / Lowering mechanic
    /// • TODO Cover with tarp?             This one is easy
    ///     - when the boat is fully up it gets covered in tarp (low poly model)
    /// • Note: the SaveCleaner should be updated to remove the davits from the brig
    /// QUESTIONS:
    /// When should the object be replaced?
    ///     • As soon as it's connected to the parent boat
    ///         Pro: lowered risk of boat-boat collisions and related problems
    ///         Cons: might be hard to replace it when the player is still on board
    ///     • Once you start pulling on the winch
    ///         Pro: risk of boat-boat collisions should still be low
    ///              The player would not be on board when this happens, since they'd be pulling on the winch
    ///              on the parent boat.
    /// ISSUE: 
    /// The game breaks if you replace the boat as you are on it. You can sort of recover by replacing it again but it breaks the menu
    /// 
    /// NOTES: code needs to be refactored and cleaned FOR REAL!!!
    /// </summary>
    public class Davits : MonoBehaviour
    {
        private GameObject boat;
        private GameObject stowedBoat;

        private Rigidbody stowedBody;

        private Transform boatTrans;
        private Transform stowedBoatTrans;

        private bool stowed;
        public void Awake()
        {
            boat = GameObject.Find("BOAT Cutter (130)(Clone)");
            stowedBoat = GameObject.Find("stowedCutter(Clone)");
            boatTrans = boat.transform;
            stowedBoatTrans = stowedBoat.transform;
            stowedBody = stowedBoat.GetComponent<Rigidbody>();
            stowedBoat.SetActive(false);
            Debug.LogWarning("Davits: awakened...");
        }
        public void FixedUpdate()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                if (stowed)
                {
                    stowedBody.isKinematic = false;
                    stowed = false;
                    boatTrans.position = stowedBoatTrans.position;
                    boatTrans.rotation = Quaternion.Euler(stowedBoatTrans.eulerAngles.x, stowedBoatTrans.eulerAngles.y + 90f, stowedBoatTrans.eulerAngles.z);
                    stowedBoat.SetActive(false);
                    boat.SetActive(true);
                    Debug.LogWarning("Davits: launched");
                }
                else
                {
                    stowedBody.isKinematic = true;
                    stowed = true;
                    stowedBoatTrans.position = boatTrans.position;
                    stowedBoatTrans.rotation = Quaternion.Euler(boatTrans.eulerAngles.x, boatTrans.eulerAngles.y - 90f, boatTrans.eulerAngles.z);
                    boat.SetActive(false);
                    stowedBoat.SetActive(true);
                    Debug.LogWarning("Davits: stowed");
                }
                
            }
        }

        /*private void DisablePhysics()
        {
            boatProbes.enabled = false;
            boatAlignNormal.enabled = false;
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;
        }
        private void EnablePhysics()
        {
            boatProbes.enabled = true;
            boatAlignNormal.enabled = true;
            rigidbody.isKinematic = false;
            rigidbody.useGravity = true;
        }*/
    }
}
