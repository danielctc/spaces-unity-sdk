using Spaces.React.Runtime;
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
        public void HandleEvent(string combinedData)
        {
            CombinedEventData combinedEventData = JsonUtility.FromJson<CombinedEventData>(combinedData);

            string eventName = combinedEventData.eventName;
            string eventData = combinedEventData.data;

            switch (eventName)
            {
                case "FirebaseUserFromReact":
                    FirebaseUserData firebaseUserData = JsonUtility.FromJson<FirebaseUserData>(eventData);
                    Debug.Log("User: ReactIncomingEvent.cs: React Sent Firebase User Details: " + firebaseUserData.name);
                    OnReceivedFirebaseUser?.Invoke(firebaseUserData);
                    break;

                case "HelloFromReact":
                    Debug.Log("Unity: HelloFromReact received");
                    HelloFromReactData helloFromReactData = JsonUtility.FromJson<HelloFromReactData>(eventData);
                    OnReactHelloFromReact?.Invoke(helloFromReactData);
                    break;

                case "EmoteTest":
                    Debug.Log("Unity: EmoteTest received");
                    EmoteTestData emoteTestData = JsonUtility.FromJson<EmoteTestData>(eventData);
                    OnReactEmoteTest?.Invoke(emoteTestData);
                    break;

                // Handle the new AvatarUrlFromReact event
                case "AvatarUrlFromReact":
                    Debug.Log("Unity: AvatarUrlFromReact received");
                    AvatarUrlData avatarUrlData = JsonUtility.FromJson<AvatarUrlData>(eventData);
                    OnReactAvatarUrlFromReact?.Invoke(avatarUrlData);
                    break;

                // Handle the SetThumbnail event
                case "SetThumbnail":
                    Debug.Log("Unity: SetThumbnail received");
                    ThumbnailData thumbnailData = JsonUtility.FromJson<ThumbnailData>(eventData);
                    OnReactSetThumbnail?.Invoke(thumbnailData);
                    break;

                // Handle the MediaScreen event
                case "MediaScreen":
                    Debug.Log("Unity: MediaScreen event received");
                    MediaScreenData mediaScreenData = JsonUtility.FromJson<MediaScreenData>(eventData);
                    OnReactMediaScreen?.Invoke(mediaScreenData);
                    break;
                    
                // Handle the SetMediaScreenImage event
                case "SetMediaScreenImage":
                    Debug.Log("Unity: SetMediaScreenImage event received");
                    MediaScreenImageData mediaScreenImageData = JsonUtility.FromJson<MediaScreenImageData>(eventData);
                    OnReactSetMediaScreenImage?.Invoke(mediaScreenImageData);
                    break;
                    
                // Handle the SetMediaScreenThumbnail event
                case "SetMediaScreenThumbnail":
                    Debug.Log("Unity: SetMediaScreenThumbnail event received");
                    MediaScreenThumbnailData mediaScreenThumbnailData = JsonUtility.FromJson<MediaScreenThumbnailData>(eventData);
                    OnReactSetMediaScreenThumbnail?.Invoke(mediaScreenThumbnailData);
                    break;
                    
                // Handle the ForceUpdateMediaScreen event
                case "ForceUpdateMediaScreen":
                    Debug.Log("Unity: ForceUpdateMediaScreen event received");
                    ForceUpdateMediaScreenData forceUpdateMediaScreenData = JsonUtility.FromJson<ForceUpdateMediaScreenData>(eventData);
                    OnReactForceUpdateMediaScreen?.Invoke(forceUpdateMediaScreenData);
                    break;
                    
                // Handle the PlayMediaScreenVideo event
                case "PlayMediaScreenVideo":
                    Debug.Log("Unity: PlayMediaScreenVideo event received");
                    PlayMediaScreenVideoData playMediaScreenVideoData = JsonUtility.FromJson<PlayMediaScreenVideoData>(eventData);
                    OnPlayMediaScreenVideo?.Invoke(playMediaScreenVideoData);
                    break;

                // Handle the KeyboardCaptureRequest event
                case "KeyboardCaptureRequest":
                    Debug.Log("Unity: KeyboardCaptureRequest event received");
                    KeyboardCaptureRequestData keyboardCaptureRequestData = JsonUtility.FromJson<KeyboardCaptureRequestData>(eventData);
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
                    HLSStreamData hlsStreamData = JsonUtility.FromJson<HLSStreamData>(eventData);
                    OnReactSetHLSStream?.Invoke(hlsStreamData);
                    break;

                default:
                    Debug.LogWarning("Unknown event: " + eventName);
                    break;
            }
        }
    }
}
