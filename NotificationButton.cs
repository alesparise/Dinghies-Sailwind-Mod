using UnityEngine;

namespace Dinghies
{
    /// <summary>
    /// Controls the button for the notification message
    /// </summary>
    public class NotificationButton : GoPointerButton
    {   
        public override void OnActivate()
        {   //button is clicked

            //closes the UI and writes somewhere that the message was seen
            Debug.LogWarning("NM: Button activated...");
        }
    }
}
