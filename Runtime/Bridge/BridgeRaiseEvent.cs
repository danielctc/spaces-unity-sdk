using System.Runtime.InteropServices;
using UnityEngine;

namespace Spaces.React.Runtime.Bridge
{
    /// <summary>
    /// BridgeRaiseEvent - Sends events from Unity to React
    ///
    /// In the thin client architecture, Unity sends:
    /// - Local player transform updates
    /// - Input events (clicks, keypresses)
    /// - Spawn/despawn requests
    /// - Custom events
    ///
    /// React handles all logic and sends back authoritative state.
    /// </summary>
    public static class BridgeRaiseEvent
    {
        #region JavaScript Interop

        #if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void JsBridgeLocalTransform(string jsonData);

        [DllImport("__Internal")]
        private static extern void JsBridgeLocalAnimation(string jsonData);

        [DllImport("__Internal")]
        private static extern void JsBridgeSpawnRequest(string jsonData);

        [DllImport("__Internal")]
        private static extern void JsBridgeDespawnRequest(string jsonData);

        [DllImport("__Internal")]
        private static extern void JsBridgeOwnershipRequest(string jsonData);

        [DllImport("__Internal")]
        private static extern void JsBridgeCustomEvent(string jsonData);

        [DllImport("__Internal")]
        private static extern void JsBridgeInputEvent(string jsonData);
        #endif

        #endregion

        #region Public API

        /// <summary>
        /// Send local actor transform to React
        /// </summary>
        public static void SendLocalTransform(LocalTransformData data)
        {
            string json = JsonUtility.ToJson(data);

            #if UNITY_WEBGL && !UNITY_EDITOR
            JsBridgeLocalTransform(json);
            #else
            Debug.Log($"[BridgeRaiseEvent] SendLocalTransform: {json}");
            #endif
        }

        /// <summary>
        /// Send local actor animation state to React
        /// </summary>
        public static void SendLocalAnimation(LocalAnimationData data)
        {
            string json = JsonUtility.ToJson(data);

            #if UNITY_WEBGL && !UNITY_EDITOR
            JsBridgeLocalAnimation(json);
            #else
            Debug.Log($"[BridgeRaiseEvent] SendLocalAnimation: {json}");
            #endif
        }

        /// <summary>
        /// Request to spawn an object (React will handle and confirm)
        /// </summary>
        public static void RequestSpawn(SpawnRequestData data)
        {
            string json = JsonUtility.ToJson(data);

            #if UNITY_WEBGL && !UNITY_EDITOR
            JsBridgeSpawnRequest(json);
            #else
            Debug.Log($"[BridgeRaiseEvent] RequestSpawn: {json}");
            #endif
        }

        /// <summary>
        /// Request to despawn an object
        /// </summary>
        public static void RequestDespawn(DespawnRequestData data)
        {
            string json = JsonUtility.ToJson(data);

            #if UNITY_WEBGL && !UNITY_EDITOR
            JsBridgeDespawnRequest(json);
            #else
            Debug.Log($"[BridgeRaiseEvent] RequestDespawn: {json}");
            #endif
        }

        /// <summary>
        /// Request ownership of an object
        /// </summary>
        public static void RequestOwnership(OwnershipRequestData data)
        {
            string json = JsonUtility.ToJson(data);

            #if UNITY_WEBGL && !UNITY_EDITOR
            JsBridgeOwnershipRequest(json);
            #else
            Debug.Log($"[BridgeRaiseEvent] RequestOwnership: {json}");
            #endif
        }

        /// <summary>
        /// Send a custom event to React
        /// </summary>
        public static void SendCustomEvent(CustomEventData data)
        {
            string json = JsonUtility.ToJson(data);

            #if UNITY_WEBGL && !UNITY_EDITOR
            JsBridgeCustomEvent(json);
            #else
            Debug.Log($"[BridgeRaiseEvent] SendCustomEvent: {json}");
            #endif
        }

        /// <summary>
        /// Send an input event to React
        /// </summary>
        public static void SendInputEvent(InputEventData data)
        {
            string json = JsonUtility.ToJson(data);

            #if UNITY_WEBGL && !UNITY_EDITOR
            JsBridgeInputEvent(json);
            #else
            Debug.Log($"[BridgeRaiseEvent] SendInputEvent: {json}");
            #endif
        }

        #endregion
    }
}
