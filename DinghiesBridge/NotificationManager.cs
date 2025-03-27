using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Dinghies;

namespace DinghiesBridge
{   /// <summary>
    /// Manages the notification system
    /// </summary>
    public class NotificationManager : MonoBehaviour
    {
        private const string URL = "https://raw.githubusercontent.com/alesparise/Dinghies-Sailwind-Mod/refs/heads/main/notification.json";

        public TextMesh header;
        public TextMesh message;

        private bool debugMessage = false;  //this can be set to true to test messages without changing the message version
                                            //this way you don't push the message to everyone!!!
        public void Awake()
        {
            Transform player = GameObject.FindGameObjectWithTag("Player").transform;
            transform.parent = player;
            transform.localPosition = new Vector3(0, 0.75f, 0.5f);
            transform.localRotation = Quaternion.identity;

            StartCoroutine(CheckForUpdate());
        }

        private IEnumerator CheckForUpdate()
        {   //checks the url for a notification message
            using (UnityWebRequest www = UnityWebRequest.Get(URL))
            {
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
                    if (note.messageVersion != DinghiesMain.lastNoteVer.Value || debugMessage)
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
        }
        [System.Serializable]
        internal class MessageNote
        {   //This class is used to store the notification data
            #pragma warning disable CS0649
            public string mod;
            public string latestVersion;
            public string header;
            public string message;
            public float charSize;
            public string messageVersion;
            #pragma warning restore CS0649
        }
    }
}
