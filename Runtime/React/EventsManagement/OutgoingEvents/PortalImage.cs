using UnityEngine;
using System.Runtime.InteropServices;

namespace Spaces.React.Runtime
{
    [System.Serializable]
    public class PortalImageData
    {
        public string portalId;
        public string imageUrl;
    }

    public class PortalImage
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void JsSetPortalImage(string portalImageDataJson);
#endif

        public static void SendPortalImage(PortalImageData data)
        {
            string jsonData = JsonUtility.ToJson(data);
            Debug.Log($"[PortalImage] Sending Portal Image event: {jsonData}");

#if UNITY_WEBGL && !UNITY_EDITOR
            JsSetPortalImage(jsonData);
#else
            Debug.Log("[PortalImage] Running in editor, skipping JS call.");
#endif
        }
    }
} 