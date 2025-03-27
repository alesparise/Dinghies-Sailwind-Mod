using System.Security.Policy;
using UnityEngine;

namespace Dinghies
{
    /// <summary>
    /// Controls the button for the notification message
    /// </summary>
    public class NotificationButton : GoPointerButton
    {   
        public enum ButtonType
        { 
            link,   //index 0
            ok      //index 1
        }

        private ButtonType type;

        private GameObject window;

        private string url = "https://github.com/alesparise/Dinghies-Sailwind-Mod/releases/latest";

        public void Init(int t, string u)
        {
            window = transform.parent.gameObject;
            type = (ButtonType)t;
            url = u;
        }
        
        public override void OnActivate()
        {   //button is clicked

            if (type == ButtonType.link)
            {
                Application.OpenURL(url);
            }
            if (type == ButtonType.ok)
            {
                window.SetActive(false);
            }
        }
    }
}
