using UnityEngine;
using System.Runtime.InteropServices;

namespace Spaces.React.Runtime
{
    public class ForceUpdateMediaScreen : MonoBehaviour
    {
        [DllImport("__Internal")]
        private static extern void JsForceUpdateMediaScreen(string data);

        public static void SendForceUpdateMediaScreen(ForceUpdateMediaScreenData data)
        {
            string jsonData = JsonUtility.ToJson(data);
            Debug.Log($"Sending ForceUpdateMediaScreen event: {jsonData}");

#if UNITY_WEBGL && !UNITY_EDITOR
            JsForceUpdateMediaScreen(jsonData);
#else
            Debug.Log("ForceUpdateMediaScreen is only available in WebGL builds.");
#endif
        }
    }
} 