using Spaces.React.Runtime;
using Spaces.React.Runtime.Bridge;
using UnityEngine;

namespace Spaces.React.Runtime
{
    public class ReactIncomingEvent : MonoBehaviour
    {
        public static ReactIncomingEvent Instance { get; private set; }

        // Define events for different types of data you might receive from React
        public delegate void ReceivedFirebaseUserHandler(FirebaseUserData data);
        public static event ReceivedFirebaseUserHandler OnReceivedFirebaseUser;

        public delegate void HelloFromReactHandler(HelloFromReactData data);
        public static event HelloFromReactHandler OnReactHelloFromReact;

        public delegate void EmoteTestHandler(EmoteTestData data);
        public static event EmoteTestHandler OnReactEmoteTest;

        // New delegate and event for Avatar URL
        public delegate void AvatarUrlFromReactHandler(AvatarUrlData data);
        public static event AvatarUrlFromReactHandler OnReactAvatarUrlFromReact;

        // New delegate and event for Vimeo Thumbnail
        public delegate void DisplayVimeoThumbnailHandler(VimeoThumbnailData data);
        public static event DisplayVimeoThumbnailHandler OnReactDisplayVimeoThumbnail;

        // Add delegate and event for SetThumbnail
        public delegate void SetThumbnailHandler(ThumbnailData data);
        public static event SetThumbnailHandler OnReactSetThumbnail;

        // Add delegate and event for MediaScreen
        public delegate void MediaScreenHandler(MediaScreenData data);
        public static event MediaScreenHandler OnReactMediaScreen;
        
        // Add delegate and event for SetMediaScreenImage
        public delegate void SetMediaScreenImageHandler(MediaScreenImageData data);
        public static event SetMediaScreenImageHandler OnReactSetMediaScreenImage;
        
        // Add delegate and event for SetMediaScreenThumbnail
        public delegate void SetMediaScreenThumbnailHandler(MediaScreenThumbnailData data);
        public static event SetMediaScreenThumbnailHandler OnReactSetMediaScreenThumbnail;
        
        // Add delegate and event for ForceUpdateMediaScreen
        public delegate void ForceUpdateMediaScreenHandler(ForceUpdateMediaScreenData data);
        public static event ForceUpdateMediaScreenHandler OnReactForceUpdateMediaScreen;
        
        // Add delegate and event for PlayMediaScreenVideo
        public delegate void PlayMediaScreenVideoHandler(PlayMediaScreenVideoData data);
        public static event PlayMediaScreenVideoHandler OnPlayMediaScreenVideo;

        // Add delegate and event for KeyboardCaptureRequest
        public delegate void KeyboardCaptureRequestHandler(KeyboardCaptureRequestData data);
        public static event KeyboardCaptureRequestHandler OnKeyboardCaptureRequest;

        // Add delegate and event for HLSStream
        public delegate void SetHLSStreamHandler(HLSStreamData data);
        public static event SetHLSStreamHandler OnReactSetHLSStream;

        // Add delegate and event for PlacePrefab
        public delegate void PlacePrefabHandler(PrefabPlacementData data);
        public static event PlacePrefabHandler OnPlacePrefab;

        // Add delegate and event for KickPlayer
        public delegate void KickPlayerHandler(KickPlayerData data);
        public static event KickPlayerHandler OnReactKickPlayer;

        // Add delegate and event for Portal
        public delegate void PortalHandler(PortalData data);
        public static event PortalHandler OnReactPortal;

        // Add delegate and event for SetPortalImage
        public delegate void SetPortalImageHandler(PortalData data);
        public static event SetPortalImageHandler OnSetPortalImage;

        // New delegate and event for Portal Prefab Placement
        public delegate void PlacePortalPrefabHandler(PortalPrefabPlacementData data);
        public static event PlacePortalPrefabHandler OnPlacePortalPrefab;

        // New delegate and event for Portal Transform Updates
        public delegate void UpdatePortalTransformHandler(PortalTransformData data);
        public static event UpdatePortalTransformHandler OnUpdatePortalTransform;

        // Add delegate and event for SeatingHotspot
        public delegate void SeatingHotspotHandler(SeatingHotspotData data);
        public static event SeatingHotspotHandler OnReactSeatingHotspot;

        // Add delegate and event for SetSeatingHotspotModel
        public delegate void SetSeatingHotspotModelHandler(SeatingHotspotData data);
        public static event SetSeatingHotspotModelHandler OnSetSeatingHotspotModel;

        // Add delegate and event for UpdateSeatingHotspotTransform
        public delegate void UpdateSeatingHotspotTransformHandler(SeatingHotspotTransformData data);
        public static event UpdateSeatingHotspotTransformHandler OnUpdateSeatingHotspotTransform;

        // Video Canvas delegates and events
        public delegate void PlaceVideoCanvasHandler(VideoCanvasData data);
        public static event PlaceVideoCanvasHandler OnPlaceVideoCanvas;

        public delegate void UpdateVideoCanvasHandler(VideoCanvasUpdateData data);
        public static event UpdateVideoCanvasHandler OnUpdateVideoCanvas;

        public delegate void DeleteVideoCanvasHandler(VideoCanvasDeleteData data);
        public static event DeleteVideoCanvasHandler OnDeleteVideoCanvas;

        // Bridge delegates and events (Gen 2 - thin client architecture)
        public delegate void BridgeConnectHandler(Bridge.BridgeConnectData data);
        public static event BridgeConnectHandler OnBridgeConnect;

        public delegate void BridgeDisconnectHandler(Bridge.BridgeDisconnectData data);
        public static event BridgeDisconnectHandler OnBridgeDisconnect;

        public delegate void ActorJoinedHandler(Bridge.ActorJoinedData data);
        public static event ActorJoinedHandler OnActorJoined;

        public delegate void ActorLeftHandler(Bridge.ActorLeftData data);
        public static event ActorLeftHandler OnActorLeft;

        public delegate void ActorUpdateHandler(Bridge.ActorUpdateData data);
        public static event ActorUpdateHandler OnActorUpdate;

        public delegate void ObjectSpawnedHandler(Bridge.ObjectSpawnedData data);
        public static event ObjectSpawnedHandler OnObjectSpawned;

        public delegate void ObjectDespawnedHandler(Bridge.ObjectDespawnedData data);
        public static event ObjectDespawnedHandler OnObjectDespawned;

        public delegate void ObjectUpdateHandler(Bridge.ObjectUpdateData data);
        public static event ObjectUpdateHandler OnObjectUpdate;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Debug.Log("[ReactIncomingEvent] Instance created and set to DontDestroyOnLoad.");
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

        // This function will be directly invoked from JavaScript
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
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[ReactIncomingEvent] Failed to parse as combined event: {e.Message}");
                }

                // Handle as direct event
                ProcessEvent("SetPortalImage", eventData);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ReactIncomingEvent] Error processing event data: {e.Message}\nData: {eventData}");
            }
        }

        public void ProcessEvent(string eventName, string data)
        {
            Debug.Log($"[ReactIncomingEvent] Processing event: {eventName} with data: {data}");
            
            switch (eventName)
            {
                case "FirebaseUserFromReact":
                    FirebaseUserData firebaseUserData = JsonUtility.FromJson<FirebaseUserData>(data);
                    Debug.Log("User: ReactIncomingEvent.cs: React Sent Firebase User Details: " + firebaseUserData.name);
                    OnReceivedFirebaseUser?.Invoke(firebaseUserData);
                    break;

                case "PlacePrefab":
                    Debug.Log("Unity: PlacePrefab event received");
                    PrefabPlacementData prefabData = JsonUtility.FromJson<PrefabPlacementData>(data);
                    OnPlacePrefab?.Invoke(prefabData);
                    break;

                case "HelloFromReact":
                    Debug.Log("Unity: HelloFromReact received");
                    HelloFromReactData helloFromReactData = JsonUtility.FromJson<HelloFromReactData>(data);
                    OnReactHelloFromReact?.Invoke(helloFromReactData);
                    break;

                case "EmoteTest":
                    Debug.Log("Unity: EmoteTest received");
                    EmoteTestData emoteTestData = JsonUtility.FromJson<EmoteTestData>(data);
                    OnReactEmoteTest?.Invoke(emoteTestData);
                    break;

                // Handle the new AvatarUrlFromReact event
                case "AvatarUrlFromReact":
                    Debug.Log("Unity: AvatarUrlFromReact received");
                    AvatarUrlData avatarUrlData = JsonUtility.FromJson<AvatarUrlData>(data);
                    OnReactAvatarUrlFromReact?.Invoke(avatarUrlData);
                    break;

                // Handle the SetThumbnail event
                case "SetThumbnail":
                    Debug.Log("Unity: SetThumbnail received");
                    ThumbnailData thumbnailData = JsonUtility.FromJson<ThumbnailData>(data);
                    OnReactSetThumbnail?.Invoke(thumbnailData);
                    break;

                // Handle the MediaScreen event
                case "MediaScreen":
                    Debug.Log("Unity: MediaScreen event received");
                    MediaScreenData mediaScreenData = JsonUtility.FromJson<MediaScreenData>(data);
                    OnReactMediaScreen?.Invoke(mediaScreenData);
                    break;
                    
                // Handle the Portal event
                case "Portal":
                    Debug.Log("Unity: Portal event received");
                    PortalData portalData = JsonUtility.FromJson<PortalData>(data);
                    OnReactPortal?.Invoke(portalData);
                    break;
                    
                // Handle the SetPortalImage event
                case "SetPortalImage":
                    Debug.Log("Unity: SetPortalImage event received");
                    PortalData setPortalImageData = JsonUtility.FromJson<PortalData>(data);
                    if (string.IsNullOrEmpty(setPortalImageData.portalId))
                    {
                        Debug.LogError("[ReactIncomingEvent] Received SetPortalImage event with empty portalId!");
                        return;
                    }
                    OnSetPortalImage?.Invoke(setPortalImageData);
                    break;
                    
                // Handle the SetMediaScreenImage event
                case "SetMediaScreenImage":
                    Debug.Log("Unity: SetMediaScreenImage event received");
                    MediaScreenImageData mediaScreenImageData = JsonUtility.FromJson<MediaScreenImageData>(data);
                    OnReactSetMediaScreenImage?.Invoke(mediaScreenImageData);
                    break;
                    
                // Handle the SetMediaScreenThumbnail event
                case "SetMediaScreenThumbnail":
                    Debug.Log("Unity: SetMediaScreenThumbnail event received");
                    MediaScreenThumbnailData mediaScreenThumbnailData = JsonUtility.FromJson<MediaScreenThumbnailData>(data);
                    OnReactSetMediaScreenThumbnail?.Invoke(mediaScreenThumbnailData);
                    break;
                    
                // Handle the ForceUpdateMediaScreen event
                case "ForceUpdateMediaScreen":
                    Debug.Log("Unity: ForceUpdateMediaScreen event received");
                    ForceUpdateMediaScreenData forceUpdateMediaScreenData = JsonUtility.FromJson<ForceUpdateMediaScreenData>(data);
                    OnReactForceUpdateMediaScreen?.Invoke(forceUpdateMediaScreenData);
                    break;
                    
                // Handle the PlayMediaScreenVideo event
                case "PlayMediaScreenVideo":
                    Debug.Log("Unity: PlayMediaScreenVideo event received");
                    PlayMediaScreenVideoData playMediaScreenVideoData = JsonUtility.FromJson<PlayMediaScreenVideoData>(data);
                    OnPlayMediaScreenVideo?.Invoke(playMediaScreenVideoData);
                    break;

                // Handle the KeyboardCaptureRequest event
                case "KeyboardCaptureRequest":
                    Debug.Log("Unity: KeyboardCaptureRequest event received");
                    KeyboardCaptureRequestData keyboardCaptureRequestData = JsonUtility.FromJson<KeyboardCaptureRequestData>(data);
                    OnKeyboardCaptureRequest?.Invoke(keyboardCaptureRequestData);
                    
                    // Process immediately since this is important for input handling
                    #if UNITY_WEBGL && !UNITY_EDITOR
                    WebGLInput.captureAllKeyboardInput = keyboardCaptureRequestData.captureKeyboard;
                    Debug.Log($"Unity: Set WebGLInput.captureAllKeyboardInput to {keyboardCaptureRequestData.captureKeyboard}");
                    #endif
                    break;

                // Handle the SetHLSStream event
                case "SetHLSStream":
                    Debug.Log("Unity: SetHLSStream received");
                    HLSStreamData hlsStreamData = JsonUtility.FromJson<HLSStreamData>(data);
                    OnReactSetHLSStream?.Invoke(hlsStreamData);
                    break;

                // Handle the KickPlayer event
                case "KickPlayer":
                    Debug.Log("Unity: KickPlayer event received");
                    KickPlayerData kickPlayerData = JsonUtility.FromJson<KickPlayerData>(data);
                    OnReactKickPlayer?.Invoke(kickPlayerData);
                    break;

                // Handle the new PlacePortalPrefab event
                case "PlacePortalPrefab":
                    Debug.Log("Unity: PlacePortalPrefab event received");
                    PortalPrefabPlacementData portalPrefabData = JsonUtility.FromJson<PortalPrefabPlacementData>(data);
                    OnPlacePortalPrefab?.Invoke(portalPrefabData);
                    break;

                // Handle the UpdatePortalTransform event
                case "UpdatePortalTransform":
                    Debug.Log("Unity: UpdatePortalTransform event received");
                    PortalTransformData transformData = JsonUtility.FromJson<PortalTransformData>(data);
                    OnUpdatePortalTransform?.Invoke(transformData);
                    break;

                // Handle the SeatingHotspot event
                case "SeatingHotspot":
                    Debug.Log("[ReactIncomingEvent] SeatingHotspot event received");
                    try
                    {
                        var hotspotData = JsonUtility.FromJson<SeatingHotspotData>(data);
                        Debug.Log($"[ReactIncomingEvent] Parsed SeatingHotspot data - ID: {hotspotData.hotspotId}, GLB URL: {hotspotData.glbUrl}");
                        OnReactSeatingHotspot?.Invoke(hotspotData);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"[ReactIncomingEvent] Failed to parse SeatingHotspot data: {e.Message}\nData: {data}");
                    }
                    break;

                // Handle the SetSeatingHotspotModel event
                case "SetSeatingHotspotModel":
                    Debug.Log("Unity: SetSeatingHotspotModel event received");
                    SeatingHotspotData setSeatingHotspotModelData = JsonUtility.FromJson<SeatingHotspotData>(data);
                    if (string.IsNullOrEmpty(setSeatingHotspotModelData.hotspotId))
                    {
                        Debug.LogError("[ReactIncomingEvent] Received SetSeatingHotspotModel event with empty hotspotId!");
                        return;
                    }
                    OnSetSeatingHotspotModel?.Invoke(setSeatingHotspotModelData);
                    break;

                // Handle the UpdateSeatingHotspotTransform event
                case "UpdateSeatingHotspotTransform":
                    Debug.Log("Unity: UpdateSeatingHotspotTransform event received");
                    SeatingHotspotTransformData seatingHotspotTransformData = JsonUtility.FromJson<SeatingHotspotTransformData>(data);
                    OnUpdateSeatingHotspotTransform?.Invoke(seatingHotspotTransformData);
                    break;

                // Handle the UpdateCatalogueItem event
                case "UpdateCatalogueItem":
                    Debug.Log("[ReactIncomingEvent] UpdateCatalogueItem event received");
                    SeatingHotspotTransformData updateData = JsonUtility.FromJson<SeatingHotspotTransformData>(data);
                    OnUpdateSeatingHotspotTransform?.Invoke(updateData);
                    break;

                // Handle the PlaceVideoCanvas event
                case "PlaceVideoCanvas":
                    Debug.Log("[ReactIncomingEvent] PlaceVideoCanvas event received");
                    try
                    {
                        VideoCanvasData videoCanvasData = JsonUtility.FromJson<VideoCanvasData>(data);
                        Debug.Log($"[ReactIncomingEvent] Parsed PlaceVideoCanvas data - ID: {videoCanvasData.canvasId}, URL: {videoCanvasData.videoUrl}");
                        OnPlaceVideoCanvas?.Invoke(videoCanvasData);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"[ReactIncomingEvent] Failed to parse PlaceVideoCanvas data: {e.Message}\nData: {data}");
                    }
                    break;

                // Handle the UpdateVideoCanvas event
                case "UpdateVideoCanvas":
                    Debug.Log("[ReactIncomingEvent] UpdateVideoCanvas event received");
                    try
                    {
                        VideoCanvasUpdateData updateVideoCanvasData = JsonUtility.FromJson<VideoCanvasUpdateData>(data);
                        OnUpdateVideoCanvas?.Invoke(updateVideoCanvasData);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"[ReactIncomingEvent] Failed to parse UpdateVideoCanvas data: {e.Message}\nData: {data}");
                    }
                    break;

                // Handle the DeleteVideoCanvas event
                case "DeleteVideoCanvas":
                    Debug.Log("[ReactIncomingEvent] DeleteVideoCanvas event received");
                    try
                    {
                        VideoCanvasDeleteData deleteVideoCanvasData = JsonUtility.FromJson<VideoCanvasDeleteData>(data);
                        OnDeleteVideoCanvas?.Invoke(deleteVideoCanvasData);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"[ReactIncomingEvent] Failed to parse DeleteVideoCanvas data: {e.Message}\nData: {data}");
                    }
                    break;

                // Bridge events (Gen 2 - thin client architecture)
                case "BridgeConnect":
                    Debug.Log("[ReactIncomingEvent] BridgeConnect event received");
                    var connectData = JsonUtility.FromJson<BridgeConnectData>(data);
                    OnBridgeConnect?.Invoke(connectData);
                    break;

                case "BridgeDisconnect":
                    Debug.Log("[ReactIncomingEvent] BridgeDisconnect event received");
                    var disconnectData = JsonUtility.FromJson<BridgeDisconnectData>(data);
                    OnBridgeDisconnect?.Invoke(disconnectData);
                    break;

                case "ActorJoined":
                    Debug.Log("[ReactIncomingEvent] ActorJoined event received");
                    var actorJoinedData = JsonUtility.FromJson<ActorJoinedData>(data);
                    OnActorJoined?.Invoke(actorJoinedData);
                    break;

                case "ActorLeft":
                    Debug.Log("[ReactIncomingEvent] ActorLeft event received");
                    var actorLeftData = JsonUtility.FromJson<ActorLeftData>(data);
                    OnActorLeft?.Invoke(actorLeftData);
                    break;

                case "ActorUpdate":
                    var actorUpdateData = JsonUtility.FromJson<ActorUpdateData>(data);
                    OnActorUpdate?.Invoke(actorUpdateData);
                    break;

                case "ObjectSpawned":
                    Debug.Log("[ReactIncomingEvent] ObjectSpawned event received");
                    var objectSpawnedData = JsonUtility.FromJson<ObjectSpawnedData>(data);
                    OnObjectSpawned?.Invoke(objectSpawnedData);
                    break;

                case "ObjectDespawned":
                    Debug.Log("[ReactIncomingEvent] ObjectDespawned event received");
                    var objectDespawnedData = JsonUtility.FromJson<ObjectDespawnedData>(data);
                    OnObjectDespawned?.Invoke(objectDespawnedData);
                    break;

                case "ObjectUpdate":
                    var objectUpdateData = JsonUtility.FromJson<ObjectUpdateData>(data);
                    OnObjectUpdate?.Invoke(objectUpdateData);
                    break;

                default:
                    Debug.LogWarning($"Unknown event: {eventName}");
                    break;
            }
        }
    }
}
