using UnityEngine;
using System.Runtime.InteropServices;
using Spaces.React.Runtime;

namespace Spaces.React.Runtime
{
    public class MediaScreenClick : MonoBehaviour
    {
        [DllImport("__Internal")]
        private static extern void JsSendMediaScreenClick(string jsonData);

        public static void SendMediaScreenClick(MediaScreenClickData data)
        {
            string jsonData = JsonUtility.ToJson(data);
            Debug.Log($"Sending MediaScreen click with ID: {data.mediaScreenId}");

#if UNITY_WEBGL && !UNITY_EDITOR
            JsSendMediaScreenClick(jsonData);
#else
            Debug.Log($"[MediaScreenClick] Would send to React: {jsonData}");
#endif
        }
    }
} 