using System.Collections;
using UnityEngine;

namespace Spaces.React.Runtime
{
    public class HLSStreamReceiver : MonoBehaviour
    {
        // Static identifier that React can use to reference this receiver - no GameObject names needed
        public const string LIVE_PROJECTOR_ID = "LiveProjector";

        [Tooltip("Reference to the HLSStreamDirectModifier component")]
        public HLSStreamDirectModifier streamModifier;
        
        [Tooltip("How often to check player status and report it back to React (in seconds)")]
        public float statusUpdateInterval = 1.0f;

        [Tooltip("Whether to automatically find the HLSStreamDirectModifier in the scene")]
        public bool autoFindModifier = true;
        
        [Tooltip("Scale of the video quad or plane, default is (1,1,1)")]
        public Vector3 videoScale = new Vector3(1, 1, 1);

        private bool hasLoggedError = false;

        private void Awake()
        {
            // Try to find the modifier if not assigned
            if (streamModifier == null && autoFindModifier)
            {
                // First try to get it from the same GameObject
                streamModifier = GetComponent<HLSStreamDirectModifier>();
                
                // If not found, try to find it in the scene
                if (streamModifier == null)
                {
                    streamModifier = FindObjectOfType<HLSStreamDirectModifier>();
                }
                
                // If still not found, try to find it by name
                if (streamModifier == null)
                {
                    var modifierObject = GameObject.Find("HLSStreamDirectModifier");
                    if (modifierObject != null)
                    {
                        streamModifier = modifierObject.GetComponent<HLSStreamDirectModifier>();
                    }
                }
            }

            if (streamModifier == null && !hasLoggedError)
            {
                Debug.LogError($"HLSStreamReceiver ({LIVE_PROJECTOR_ID}): No HLSStreamDirectModifier found! Please ensure you have a GameObject with the HLSStreamDirectModifier component in your scene.");
                hasLoggedError = true;
            }
            else if (streamModifier != null)
            {
                Debug.Log($"HLSStreamReceiver ({LIVE_PROJECTOR_ID}): Successfully connected to HLSStreamDirectModifier");
            }
            
            // Apply scale to the game object
            if (transform.localScale != videoScale)
            {
                transform.localScale = videoScale;
                Debug.Log($"HLSStreamReceiver: Applied scale {videoScale}");
            }
        }

        private void OnEnable()
        {
            // Subscribe to the OnReactSetHLSStream event
            ReactIncomingEvent.OnReactSetHLSStream += HandleSetHLSStream;
            
            // Start the status monitoring coroutine
            StartCoroutine(MonitorPlayerStatus());
            
            Debug.Log($"HLSStreamReceiver enabled with identifier: {LIVE_PROJECTOR_ID}");
        }

        private void OnDisable()
        {
            // Unsubscribe from the OnReactSetHLSStream event
            ReactIncomingEvent.OnReactSetHLSStream -= HandleSetHLSStream;
            
            // Stop all coroutines
            StopAllCoroutines();
        }

        private void HandleSetHLSStream(HLSStreamData data)
        {
            // We now only use the fixed identifier "LiveProjector" - ignore GameObject names completely
            // Check if this event is meant for this receiver using our fixed identifier
            if (!string.IsNullOrEmpty(data.identifier) && data.identifier != LIVE_PROJECTOR_ID)
            {
                // This event is for a different receiver, ignore it
                return;
            }
            
            Debug.Log($"HLSStreamReceiver ({LIVE_PROJECTOR_ID}): Handling stream URL: {data.streamUrl}");
            
            // If the URL is empty or null, don't process it
            if (string.IsNullOrEmpty(data.streamUrl))
            {
                Debug.LogWarning($"HLSStreamReceiver ({LIVE_PROJECTOR_ID}): Received empty stream URL, ignoring");
                return;
            }
            
            // Update the stream URL
            if (streamModifier != null)
            {
                streamModifier.UpdateURL(data.streamUrl);
            }
            else if (!hasLoggedError)
            {
                Debug.LogError($"HLSStreamReceiver ({LIVE_PROJECTOR_ID}): No HLSStreamDirectModifier available! Please ensure you have a GameObject with the HLSStreamDirectModifier component in your scene.");
                hasLoggedError = true;
            }
        }
        
        // Coroutine to monitor player status and report it back to React
        private IEnumerator MonitorPlayerStatus()
        {
            while (true)
            {
                // Only monitor if we have a valid reference
                if (streamModifier != null && streamModifier.playerController != null)
                {
                    #if HISPLAYER_ENABLE
                    // We only have one player (index 0)
                    SendStatusUpdate(0);
                    #endif
                }
                
                // Wait for the specified interval
                yield return new WaitForSeconds(statusUpdateInterval);
            }
        }
        
        // Helper method to send player status to React
        private void SendStatusUpdate(int playerIndex)
        {
            #if HISPLAYER_ENABLE
            if (streamModifier != null && streamModifier.playerController != null)
            {
                // We can't directly check if the player is ready or playing, 
                // so we'll just report that it's ready and playing if we have a player controller
                bool isReady = true;
                bool isPlaying = true;
                
                // Send the status to React using ReactRaiseEvent, using our fixed identifier
                ReactRaiseEvent.SendHLSStreamStatus(LIVE_PROJECTOR_ID, playerIndex.ToString(), isReady, isPlaying);
            }
            #endif
        }
    }
} 