using UnityEngine;
using System.Runtime.InteropServices;

namespace Spaces.React.Runtime
{
    [System.Serializable]
    public class PortalClickData
    {
        public string portalId;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
    }

    public class PortalClick
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void JsPortalClicked(string portalClickDataJson);
#endif

        public static void SendPortalClick(PortalClickData data)
        {
            string jsonData = JsonUtility.ToJson(data);
            Debug.Log($"[PortalClick] Sending Portal Clicked event: {jsonData}");

#if UNITY_WEBGL && !UNITY_EDITOR
            JsPortalClicked(jsonData);
#else
            Debug.Log("[PortalClick] Running in editor, skipping JS call.");
#endif
        }
    }
} 