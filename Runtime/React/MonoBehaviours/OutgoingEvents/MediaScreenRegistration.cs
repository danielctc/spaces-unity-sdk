using UnityEngine;
using System.Runtime.InteropServices;
using Spaces.React.Runtime;

namespace Spaces.React.Runtime
{
    public class MediaScreenRegistration : MonoBehaviour
    {
        [DllImport("__Internal")]
        private static extern void JsRegisterMediaScreen(string jsonData);

        public static void RegisterMediaScreen(MediaScreenRegistrationData data)
        {
            string jsonData = JsonUtility.ToJson(data);
            Debug.Log($"Registering MediaScreen with ID: {data.mediaScreenId}");

#if UNITY_WEBGL && !UNITY_EDITOR
            JsRegisterMediaScreen(jsonData);
#else
            Debug.Log($"[MediaScreenRegistration] Would send to React: {jsonData}");
#endif
        }
    }
} 