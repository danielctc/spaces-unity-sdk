using UnityEngine;
using System.Runtime.InteropServices;

namespace Spaces.React.Runtime
{
    [System.Serializable]
    public class PortalRegistrationData
    {
        public string portalId;
        public string position; // JSON string
        public string rotation; // JSON string
        public string scale;    // JSON string
        public string currentImageUrl;
        public bool hasImage;
    }

    public class PortalRegistration
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void JsRegisterPortal(string portalDataJson);
#endif

        public static void RegisterPortal(PortalRegistrationData data)
        {
            string jsonData = JsonUtility.ToJson(data);
            Debug.Log($"[PortalRegistration] Registering Portal: {jsonData}");

#if UNITY_WEBGL && !UNITY_EDITOR
            JsRegisterPortal(jsonData);
#else
            Debug.Log("[PortalRegistration] Running in editor, skipping JS call.");
#endif
        }
    }
} 