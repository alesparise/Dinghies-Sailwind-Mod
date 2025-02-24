using System.Security.Cryptography;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

namespace Dinghies
{   /// <summary>
    /// Manages the notification system
    /// </summary>
    public class NotificationManager : MonoBehaviour
    {
        private const string url = "https://raw.githubusercontent.com/alesparise/Dinghies-Sailwind-Mod/refs/heads/main/notification.json";

        private TextMesh header;
        private TextMesh message;

        //UNITY SPECIAL METHODS
        public void Awake()
        {
            Transform player = GameObject.FindGameObjectWithTag("Player").transform;
            transform.parent = player;
            transform.localPosition = new Vector3(0, 0.75f, 0.5f);
            transform.localRotation = Quaternion.identity;
            transform.Find("okButton").gameObject.AddComponent<NotificationButton>();
            transform.Find("githubButton").gameObject.AddComponent<NotificationButton>();

            header = transform.Find("header").GetComponent<TextMesh>();
            message = transform.Find("message").GetComponent<TextMesh>();

            StartCoroutine(CheckForUpdate());
        }

        //MAIN METHODS
        private IEnumerator CheckForUpdate()
        {   //checks the url for a notification message
            UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {   //no connection or wrong url, I think
                gameObject.SetActive(false);
                Debug.LogError("Dinghies: cannot fetch notifications from GitHub: " + www.error);
            }
            else
            {   
                string jsonResponse = www.downloadHandler.text;
                string hash = CalculateSHA256(jsonResponse);
                if (hash != DinghiesMain.lastNoteHash.Value)
                {   //if the hash is different, then show the notification and save the new hash as last hash
                    MessageNote note = JsonUtility.FromJson<MessageNote>(jsonResponse);
                    header.text = note.header;
                    message.text = note.message;
                    DinghiesMain.lastNoteHash.Value = hash;
                    Debug.LogWarning("NM: notification shown");
                }
                else
                {   //if the message is the same, hide the notification
                    Debug.LogWarning("Notification has same hash!");
                    gameObject.SetActive(false);
                }
            }
        }
        [System.Serializable]
        internal class MessageNote
        {   //This class is used to store the notification data
            public string mod;
            public string latestVersion;
            public string header;
            public string message;
        }

        //HELPER METHODS
        public static bool CheckForNotifications()
        {
            //cool code that checks if a notification is available and if it hasn't been shown yet.
            return true;
        }
        public string CalculateSHA256(string input)
        {   //calculate hash of the json message
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder hashString = new StringBuilder();
                foreach (byte b in bytes)
                {
                    hashString.Append(b.ToString("x2"));
                }
                return hashString.ToString();
            }
        }
    }
}
