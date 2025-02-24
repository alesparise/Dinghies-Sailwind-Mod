using AmplifyOcclusion;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Dinghies
{   /// <summary>
    /// Manages the notification system
    /// </summary>
    public class NotificationManager : MonoBehaviour
    {
        private string url = "https://example.com/mod_version.json"; // your URL to the version file

        //UNITY SPECIAL METHODS
        public void Awake()
        {
            Transform player = GameObject.FindGameObjectWithTag("Player").transform;
            transform.parent = player;
            transform.localPosition = new Vector3(0, 0.75f, 0.5f);
            transform.localRotation = Quaternion.identity;
            transform.Find("button").gameObject.AddComponent<NotificationButton>();

            StartCoroutine(CheckForUpdate());
        }

        //MAIN METHODS
        private IEnumerator CheckForUpdate()
        {
            UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogError("Error fetching version info: " + www.error);
            }
            else
            {
                string jsonResponse = www.downloadHandler.text;
                VersionInfo versionInfo = JsonUtility.FromJson<VersionInfo>(jsonResponse);
                if (versionInfo != null)
                {
                    Debug.LogWarning("NM: success...");
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
            public string hash;
        }

        //HELPER METHODS
        public static bool CheckForNotifications()
        {
            //cool code that checks if a notification is available and if it hasn't been shown yet.
            return true;
        }
    }
}
