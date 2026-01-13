using System;
using System.Collections.Generic;
using Spaces.React.Runtime.Bridge;
using UnityEngine;

namespace Spaces.React.Runtime
{
    /// <summary>
    /// ReactIncomingEvent - Handles events from React using a registry pattern.
    ///
    /// Instead of a massive switch statement, MonoBehaviours register their own handlers.
    /// This makes adding new events trivial - just call RegisterHandler() in your script.
    /// </summary>
    public class ReactIncomingEvent : MonoBehaviour
    {
        public static ReactIncomingEvent Instance { get; private set; }

        // Handler registry - maps event names to handler functions
        private static readonly Dictionary<string, Action<string>> _handlers = new Dictionary<string, Action<string>>();

        // Legacy events (kept for backward compatibility)
        #region Legacy Events

        public delegate void ReceivedFirebaseUserHandler(FirebaseUserData data);
        public static event ReceivedFirebaseUserHandler OnReceivedFirebaseUser;

        public delegate void HelloFromReactHandler(HelloFromReactData data);
        public static event HelloFromReactHandler OnReactHelloFromReact;

        public delegate void EmoteTestHandler(EmoteTestData data);
        public static event EmoteTestHandler OnReactEmoteTest;

        public delegate void AvatarUrlFromReactHandler(AvatarUrlData data);
        public static event AvatarUrlFromReactHandler OnReactAvatarUrlFromReact;

        public delegate void DisplayVimeoThumbnailHandler(VimeoThumbnailData data);
        public static event DisplayVimeoThumbnailHandler OnReactDisplayVimeoThumbnail;

        public delegate void SetThumbnailHandler(ThumbnailData data);
        public static event SetThumbnailHandler OnReactSetThumbnail;

        public delegate void MediaScreenHandler(MediaScreenData data);
        public static event MediaScreenHandler OnReactMediaScreen;

        public delegate void SetMediaScreenImageHandler(MediaScreenImageData data);
        public static event SetMediaScreenImageHandler OnReactSetMediaScreenImage;

        public delegate void SetMediaScreenThumbnailHandler(MediaScreenThumbnailData data);
        public static event SetMediaScreenThumbnailHandler OnReactSetMediaScreenThumbnail;

        public delegate void ForceUpdateMediaScreenHandler(ForceUpdateMediaScreenData data);
        public static event ForceUpdateMediaScreenHandler OnReactForceUpdateMediaScreen;

        public delegate void PlayMediaScreenVideoHandler(PlayMediaScreenVideoData data);
        public static event PlayMediaScreenVideoHandler OnPlayMediaScreenVideo;

        public delegate void KeyboardCaptureRequestHandler(KeyboardCaptureRequestData data);
        public static event KeyboardCaptureRequestHandler OnKeyboardCaptureRequest;

        public delegate void SetHLSStreamHandler(HLSStreamData data);
        public static event SetHLSStreamHandler OnReactSetHLSStream;

        public delegate void PlacePrefabHandler(PrefabPlacementData data);
        public static event PlacePrefabHandler OnPlacePrefab;

        public delegate void KickPlayerHandler(KickPlayerData data);
        public static event KickPlayerHandler OnReactKickPlayer;

        public delegate void PortalHandler(PortalData data);
        public static event PortalHandler OnReactPortal;

        public delegate void SetPortalImageHandler(PortalData data);
        public static event SetPortalImageHandler OnSetPortalImage;

        public delegate void PlacePortalPrefabHandler(PortalPrefabPlacementData data);
        public static event PlacePortalPrefabHandler OnPlacePortalPrefab;

        public delegate void UpdatePortalTransformHandler(PortalTransformData data);
        public static event UpdatePortalTransformHandler OnUpdatePortalTransform;

        public delegate void SeatingHotspotHandler(SeatingHotspotData data);
        public static event SeatingHotspotHandler OnReactSeatingHotspot;

        public delegate void SetSeatingHotspotModelHandler(SeatingHotspotData data);
        public static event SetSeatingHotspotModelHandler OnSetSeatingHotspotModel;

        public delegate void UpdateSeatingHotspotTransformHandler(SeatingHotspotTransformData data);
        public static event UpdateSeatingHotspotTransformHandler OnUpdateSeatingHotspotTransform;

        public delegate void PlaceVideoCanvasHandler(VideoCanvasData data);
        public static event PlaceVideoCanvasHandler OnPlaceVideoCanvas;

        public delegate void UpdateVideoCanvasHandler(VideoCanvasUpdateData data);
        public static event UpdateVideoCanvasHandler OnUpdateVideoCanvas;

        public delegate void DeleteVideoCanvasHandler(VideoCanvasDeleteData data);
        public static event DeleteVideoCanvasHandler OnDeleteVideoCanvas;

        public delegate void BridgeConnectHandler(BridgeConnectData data);
        public static event BridgeConnectHandler OnBridgeConnect;

        public delegate void BridgeDisconnectHandler(BridgeDisconnectData data);
        public static event BridgeDisconnectHandler OnBridgeDisconnect;

        public delegate void ActorJoinedHandler(ActorJoinedData data);
        public static event ActorJoinedHandler OnActorJoined;

        public delegate void ActorLeftHandler(ActorLeftData data);
        public static event ActorLeftHandler OnActorLeft;

        public delegate void ActorUpdateHandler(ActorUpdateData data);
        public static event ActorUpdateHandler OnActorUpdate;

        public delegate void ObjectSpawnedHandler(ObjectSpawnedData data);
        public static event ObjectSpawnedHandler OnObjectSpawned;

        public delegate void ObjectDespawnedHandler(ObjectDespawnedData data);
        public static event ObjectDespawnedHandler OnObjectDespawned;

        public delegate void ObjectUpdateHandler(ObjectUpdateData data);
        public static event ObjectUpdateHandler OnObjectUpdate;

        #endregion

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                RegisterLegacyHandlers();
                Debug.Log("[ReactIncomingEvent] Instance created with registry pattern.");
            }
            else
            {
                Debug.Log("[ReactIncomingEvent] Instance already exists, destroying duplicate.");
                Destroy(gameObject);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeOnLoad()
        {
            Debug.Log("[ReactIncomingEvent] InitializeOnLoad called.");
            if (Instance == null)
            {
                var managerObject = new GameObject("ReactIncomingEvent");
                managerObject.AddComponent<ReactIncomingEvent>();
                Debug.Log("[ReactIncomingEvent] ReactIncomingEvent GameObject created in InitializeOnLoad.");
            }
        }

        #region Public API

        /// <summary>
        /// Register a handler for a specific event type.
        /// Call this from your MonoBehaviour's OnEnable().
        /// </summary>
        /// <param name="eventName">The event name to handle</param>
        /// <param name="handler">The handler function receiving JSON data</param>
        public static void RegisterHandler(string eventName, Action<string> handler)
        {
            if (_handlers.ContainsKey(eventName))
            {
                Debug.LogWarning($"[ReactIncomingEvent] Overwriting existing handler for: {eventName}");
            }
            _handlers[eventName] = handler;
            Debug.Log($"[ReactIncomingEvent] Registered handler for: {eventName}");
        }

        /// <summary>
        /// Unregister a handler for a specific event type.
        /// Call this from your MonoBehaviour's OnDisable().
        /// </summary>
        /// <param name="eventName">The event name to unregister</param>
        public static void UnregisterHandler(string eventName)
        {
            if (_handlers.Remove(eventName))
            {
                Debug.Log($"[ReactIncomingEvent] Unregistered handler for: {eventName}");
            }
        }

        /// <summary>
        /// Register a typed handler that auto-deserializes JSON to your data type.
        /// </summary>
        /// <typeparam name="T">The data type to deserialize to</typeparam>
        /// <param name="eventName">The event name to handle</param>
        /// <param name="handler">The handler function receiving typed data</param>
        public static void RegisterHandler<T>(string eventName, Action<T> handler)
        {
            RegisterHandler(eventName, (string json) =>
            {
                try
                {
                    T data = JsonUtility.FromJson<T>(json);
                    handler(data);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[ReactIncomingEvent] Failed to deserialize {eventName}: {e.Message}\nJSON: {json}");
                }
            });
        }

        #endregion

        /// <summary>
        /// Called from JavaScript to handle incoming events.
        /// </summary>
        public void HandleEvent(string eventData)
        {
            try
            {
                Debug.Log($"[ReactIncomingEvent] Received event data: {eventData}");

                // Try to parse as combined event first
                try
                {
                    CombinedEventData combinedEventData = JsonUtility.FromJson<CombinedEventData>(eventData);
                    if (combinedEventData != null && !string.IsNullOrEmpty(combinedEventData.eventName))
                    {
                        Debug.Log($"[ReactIncomingEvent] Parsed as combined event: {combinedEventData.eventName}");

                        // Handle double-encoded JSON data
                        string decodedData = combinedEventData.data;
                        if (decodedData.StartsWith("\"") && decodedData.EndsWith("\""))
                        {
                            decodedData = decodedData.Substring(1, decodedData.Length - 2)
                                                    .Replace("\\\"", "\"")
                                                    .Replace("\\\\", "\\");
                        }

                        ProcessEvent(combinedEventData.eventName, decodedData);
                        return;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[ReactIncomingEvent] Failed to parse as combined event: {e.Message}");
                }

                // Handle as direct event
                ProcessEvent("SetPortalImage", eventData);
            }
            catch (Exception e)
            {
                Debug.LogError($"[ReactIncomingEvent] Error processing event data: {e.Message}\nData: {eventData}");
            }
        }

        /// <summary>
        /// Process an event by looking up its handler in the registry.
        /// </summary>
        public void ProcessEvent(string eventName, string data)
        {
            Debug.Log($"[ReactIncomingEvent] Processing event: {eventName}");

            if (_handlers.TryGetValue(eventName, out var handler))
            {
                try
                {
                    handler(data);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[ReactIncomingEvent] Handler error for {eventName}: {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"[ReactIncomingEvent] No handler registered for: {eventName}");
            }
        }

        /// <summary>
        /// Register legacy handlers to maintain backward compatibility.
        /// New code should use RegisterHandler() instead.
        /// </summary>
        private void RegisterLegacyHandlers()
        {
            // Firebase User
            RegisterHandler<FirebaseUserData>("FirebaseUserFromReact", data =>
            {
                Debug.Log($"User: React Sent Firebase User Details: {data.name}");
                OnReceivedFirebaseUser?.Invoke(data);
            });

            // Prefab Placement
            RegisterHandler<PrefabPlacementData>("PlacePrefab", data =>
            {
                Debug.Log("Unity: PlacePrefab event received");
                OnPlacePrefab?.Invoke(data);
            });

            // Hello From React
            RegisterHandler<HelloFromReactData>("HelloFromReact", data =>
            {
                Debug.Log("Unity: HelloFromReact received");
                OnReactHelloFromReact?.Invoke(data);
            });

            // Emote Test
            RegisterHandler<EmoteTestData>("EmoteTest", data =>
            {
                Debug.Log("Unity: EmoteTest received");
                OnReactEmoteTest?.Invoke(data);
            });

            // Avatar URL
            RegisterHandler<AvatarUrlData>("AvatarUrlFromReact", data =>
            {
                Debug.Log("Unity: AvatarUrlFromReact received");
                OnReactAvatarUrlFromReact?.Invoke(data);
            });

            // Thumbnail
            RegisterHandler<ThumbnailData>("SetThumbnail", data =>
            {
                Debug.Log("Unity: SetThumbnail received");
                OnReactSetThumbnail?.Invoke(data);
            });

            // Media Screen
            RegisterHandler<MediaScreenData>("MediaScreen", data =>
            {
                Debug.Log("Unity: MediaScreen event received");
                OnReactMediaScreen?.Invoke(data);
            });

            // Portal
            RegisterHandler<PortalData>("Portal", data =>
            {
                Debug.Log("Unity: Portal event received");
                OnReactPortal?.Invoke(data);
            });

            // Set Portal Image
            RegisterHandler<PortalData>("SetPortalImage", data =>
            {
                Debug.Log("Unity: SetPortalImage event received");
                if (string.IsNullOrEmpty(data.portalId))
                {
                    Debug.LogError("[ReactIncomingEvent] Received SetPortalImage event with empty portalId!");
                    return;
                }
                OnSetPortalImage?.Invoke(data);
            });

            // Media Screen Image
            RegisterHandler<MediaScreenImageData>("SetMediaScreenImage", data =>
            {
                Debug.Log("Unity: SetMediaScreenImage event received");
                OnReactSetMediaScreenImage?.Invoke(data);
            });

            // Media Screen Thumbnail
            RegisterHandler<MediaScreenThumbnailData>("SetMediaScreenThumbnail", data =>
            {
                Debug.Log("Unity: SetMediaScreenThumbnail event received");
                OnReactSetMediaScreenThumbnail?.Invoke(data);
            });

            // Force Update Media Screen
            RegisterHandler<ForceUpdateMediaScreenData>("ForceUpdateMediaScreen", data =>
            {
                Debug.Log("Unity: ForceUpdateMediaScreen event received");
                OnReactForceUpdateMediaScreen?.Invoke(data);
            });

            // Play Media Screen Video
            RegisterHandler<PlayMediaScreenVideoData>("PlayMediaScreenVideo", data =>
            {
                Debug.Log("Unity: PlayMediaScreenVideo event received");
                OnPlayMediaScreenVideo?.Invoke(data);
            });

            // Keyboard Capture Request
            RegisterHandler<KeyboardCaptureRequestData>("KeyboardCaptureRequest", data =>
            {
                Debug.Log("Unity: KeyboardCaptureRequest event received");
                OnKeyboardCaptureRequest?.Invoke(data);
#if UNITY_WEBGL && !UNITY_EDITOR
                WebGLInput.captureAllKeyboardInput = data.captureKeyboard;
                Debug.Log($"Unity: Set WebGLInput.captureAllKeyboardInput to {data.captureKeyboard}");
#endif
            });

            // HLS Stream
            RegisterHandler<HLSStreamData>("SetHLSStream", data =>
            {
                Debug.Log("Unity: SetHLSStream received");
                OnReactSetHLSStream?.Invoke(data);
            });

            // Kick Player
            RegisterHandler<KickPlayerData>("KickPlayer", data =>
            {
                Debug.Log("Unity: KickPlayer event received");
                OnReactKickPlayer?.Invoke(data);
            });

            // Portal Prefab Placement
            RegisterHandler<PortalPrefabPlacementData>("PlacePortalPrefab", data =>
            {
                Debug.Log("Unity: PlacePortalPrefab event received");
                OnPlacePortalPrefab?.Invoke(data);
            });

            // Portal Transform Update
            RegisterHandler<PortalTransformData>("UpdatePortalTransform", data =>
            {
                Debug.Log("Unity: UpdatePortalTransform event received");
                OnUpdatePortalTransform?.Invoke(data);
            });

            // Seating Hotspot
            RegisterHandler<SeatingHotspotData>("SeatingHotspot", data =>
            {
                Debug.Log($"[ReactIncomingEvent] SeatingHotspot - ID: {data.hotspotId}, GLB URL: {data.glbUrl}");
                OnReactSeatingHotspot?.Invoke(data);
            });

            // Set Seating Hotspot Model
            RegisterHandler<SeatingHotspotData>("SetSeatingHotspotModel", data =>
            {
                Debug.Log("Unity: SetSeatingHotspotModel event received");
                if (string.IsNullOrEmpty(data.hotspotId))
                {
                    Debug.LogError("[ReactIncomingEvent] Received SetSeatingHotspotModel with empty hotspotId!");
                    return;
                }
                OnSetSeatingHotspotModel?.Invoke(data);
            });

            // Seating Hotspot Transform Update
            RegisterHandler<SeatingHotspotTransformData>("UpdateSeatingHotspotTransform", data =>
            {
                Debug.Log("Unity: UpdateSeatingHotspotTransform event received");
                OnUpdateSeatingHotspotTransform?.Invoke(data);
            });

            // Catalogue Item Update (uses same transform data)
            RegisterHandler<SeatingHotspotTransformData>("UpdateCatalogueItem", data =>
            {
                Debug.Log("[ReactIncomingEvent] UpdateCatalogueItem event received");
                OnUpdateSeatingHotspotTransform?.Invoke(data);
            });

            // Video Canvas
            RegisterHandler<VideoCanvasData>("PlaceVideoCanvas", data =>
            {
                Debug.Log($"[ReactIncomingEvent] PlaceVideoCanvas - ID: {data.canvasId}, URL: {data.videoUrl}");
                OnPlaceVideoCanvas?.Invoke(data);
            });

            RegisterHandler<VideoCanvasUpdateData>("UpdateVideoCanvas", data =>
            {
                Debug.Log("[ReactIncomingEvent] UpdateVideoCanvas event received");
                OnUpdateVideoCanvas?.Invoke(data);
            });

            RegisterHandler<VideoCanvasDeleteData>("DeleteVideoCanvas", data =>
            {
                Debug.Log("[ReactIncomingEvent] DeleteVideoCanvas event received");
                OnDeleteVideoCanvas?.Invoke(data);
            });

            // Bridge Events (Gen 2 - thin client architecture)
            RegisterHandler<BridgeConnectData>("BridgeConnect", data =>
            {
                Debug.Log("[ReactIncomingEvent] BridgeConnect event received");
                OnBridgeConnect?.Invoke(data);
            });

            RegisterHandler<BridgeDisconnectData>("BridgeDisconnect", data =>
            {
                Debug.Log("[ReactIncomingEvent] BridgeDisconnect event received");
                OnBridgeDisconnect?.Invoke(data);
            });

            RegisterHandler<ActorJoinedData>("ActorJoined", data =>
            {
                Debug.Log("[ReactIncomingEvent] ActorJoined event received");
                OnActorJoined?.Invoke(data);
            });

            RegisterHandler<ActorLeftData>("ActorLeft", data =>
            {
                Debug.Log("[ReactIncomingEvent] ActorLeft event received");
                OnActorLeft?.Invoke(data);
            });

            RegisterHandler<ActorUpdateData>("ActorUpdate", data =>
            {
                OnActorUpdate?.Invoke(data);
            });

            RegisterHandler<ObjectSpawnedData>("ObjectSpawned", data =>
            {
                Debug.Log("[ReactIncomingEvent] ObjectSpawned event received");
                OnObjectSpawned?.Invoke(data);
            });

            RegisterHandler<ObjectDespawnedData>("ObjectDespawned", data =>
            {
                Debug.Log("[ReactIncomingEvent] ObjectDespawned event received");
                OnObjectDespawned?.Invoke(data);
            });

            RegisterHandler<ObjectUpdateData>("ObjectUpdate", data =>
            {
                OnObjectUpdate?.Invoke(data);
            });

            Debug.Log($"[ReactIncomingEvent] Registered {_handlers.Count} legacy handlers");
        }
    }
}
