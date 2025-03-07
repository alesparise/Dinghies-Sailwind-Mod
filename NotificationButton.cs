using UnityEngine;

namespace Dinghies
{
    /// <summary>
    /// Controls the button for the notification message
    /// </summary>
    public class NotificationButton : GoPointerButton
    {   
        private enum ButtonType
        { 
            github,
            ok
        }

        private ButtonType type;

        private GameObject window;

        private const string url = "https://github.com/alesparise/Dinghies-Sailwind-Mod/releases/latest";
        public void Awake()
        {
            window = transform.parent.gameObject;
            if (name == "okButton")
            {
                type = ButtonType.ok;
            }
            else if (name == "githubButton")
            {
                type = ButtonType.github;
            }
        }
        public override void OnActivate()
        {   //button is clicked
            if (type == ButtonType.github)
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
