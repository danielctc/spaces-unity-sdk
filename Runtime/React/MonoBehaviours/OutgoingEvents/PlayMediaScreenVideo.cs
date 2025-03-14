using UnityEngine;
using System.Runtime.InteropServices;
using Spaces.React.Runtime;

namespace Spaces.React.Runtime
{
    public class PlayMediaScreenVideo : MonoBehaviour
    {
        [DllImport("__Internal")]
        private static extern void JsPlayMediaScreenVideo(string jsonData);

        public static void SendPlayMediaScreenVideo(PlayMediaScreenVideoData data)
        {
            string jsonData = JsonUtility.ToJson(data);
            Debug.Log($"Sending PlayMediaScreenVideo with ID: {data.mediaScreenId}");

#if UNITY_WEBGL && !UNITY_EDITOR
            JsPlayMediaScreenVideo(jsonData);
#else
            Debug.Log($"[PlayMediaScreenVideo] Would send to React: {jsonData}");
#endif
        }
    }
} 