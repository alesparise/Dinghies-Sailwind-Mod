using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Dinghies
{   /// <summary>
    /// Manages the notification system
    /// </summary>
    public class NotificationManager : MonoBehaviour
    {
        private const string url = "https://raw.githubusercontent.com/alesparise/Dinghies-Sailwind-Mod/refs/heads/main/notification.json";

        private TextMesh header;
        private TextMesh message;

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
                MessageNote note = JsonUtility.FromJson<MessageNote>(jsonResponse);
                if (note.messageVersion != DinghiesMain.lastNoteVer.Value)
                {   //if the messageVersion is different, then show the notification and save the new messageVersion in the config file
                    header.text = note.header;
                    message.text = note.message;
                    message.characterSize = note.charSize;
                    DinghiesMain.lastNoteVer.Value = note.messageVersion;
                }
                else
                {   //if the message is the same, hide the notification
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
            public float charSize;
            public string messageVersion;
        }
    }
}
