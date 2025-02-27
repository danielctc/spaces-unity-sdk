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

        
        // New delegate and event for ToggleEditMode
        public delegate void ToggleEditModeHandler(bool isEditMode);
        public static event ToggleEditModeHandler OnToggleEditMode;

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


                // Handle the ToggleEditMode event
                case "ToggleEditMode":
                    Debug.Log("Unity: ToggleEditMode received");
                    bool isEditMode = bool.Parse(eventData);
                    OnToggleEditMode?.Invoke(isEditMode);
                    break;

                default:
                    Debug.LogWarning("Unknown event: " + eventName);
                    break;
            }
        }
    }
}
