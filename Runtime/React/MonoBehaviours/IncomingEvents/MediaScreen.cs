using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.EventSystems;
using Spaces.React.Runtime;

namespace Spaces.React.Runtime
{
    public class MediaScreen : MonoBehaviour, IPointerClickHandler
    {
        [Tooltip("The target mesh where the image will be rendered.")]
        public MeshRenderer targetRenderer;
        
        /*[Tooltip("Optional play button overlay for video content.")]
        public GameObject playButtonOverlay;*/

        [Tooltip("Unique identifier for this media screen (required for React)")]
        public string screenId = "";

        // Static counter for generating sequential IDs
        private static int screenCounter = 0;

        // Track the current media state
        private string currentImageUrl = "";
        private string currentVideoUrl = "";  // Store video URL separately
        private string currentThumbnailUrl = "";
        private bool hasImage = false;
        private bool isVideo = false;
        private bool displayAsVideo = false;
        
        // Track the last refresh timestamp for caching control
        private long lastImageRefreshTimestamp = 0;
        private long lastThumbnailRefreshTimestamp = 0;
        
        // Track active coroutines for cancellation
        private Coroutine activeImageLoadCoroutine = null;

        private void Awake()
        {
            // Generate a unique ID if none is provided
            if (string.IsNullOrEmpty(screenId))
            {
                // Use a sequential number for predictable IDs
                screenId = "MediaScreen_" + screenCounter;
                screenCounter++;
            }

            // Do NOT change the GameObject name

            // Ensure we have a target renderer
            if (targetRenderer == null)
            {
                targetRenderer = GetComponent<MeshRenderer>();
                if (targetRenderer == null)
                {
                    Debug.LogError("MediaScreen requires a MeshRenderer component. Please assign one in the inspector or add one to this GameObject.");
                }
            }
            
            /*// Create play button overlay if it doesn't exist
            if (playButtonOverlay == null)
            {
                // You can create a simple play button overlay here or use a prefab
                // This is a simplified example - you might want to use a proper UI or 3D model
                playButtonOverlay = CreatePlayButtonOverlay();
            }
            
            // Initially hide the play button
            if (playButtonOverlay != null)
            {
                playButtonOverlay.SetActive(false);
            }*/
            
            // Ensure we have a collider for click detection
            if (GetComponent<Collider>() == null)
            {
                // Add a box collider if none exists
                BoxCollider collider = gameObject.AddComponent<BoxCollider>();
                
                // If we have a MeshRenderer, size the collider to match
                if (targetRenderer != null && targetRenderer.bounds.size != Vector3.zero)
                {
                    collider.size = targetRenderer.bounds.size;
                    collider.center = targetRenderer.bounds.center - transform.position;
                }
            }
            
            // Ensure the main camera has a PhysicsRaycaster for click detection
            EnsurePhysicsRaycasterExists();
        }

        // Helper method to ensure a PhysicsRaycaster exists on the main camera
        private void EnsurePhysicsRaycasterExists()
        {
            // Check if any camera has a PhysicsRaycaster
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogWarning("No main camera found. Click events may not work.");
                return;
            }

            // Check if the main camera already has a PhysicsRaycaster
            PhysicsRaycaster raycaster = mainCamera.GetComponent<PhysicsRaycaster>();
            if (raycaster == null)
            {
                // Add a PhysicsRaycaster to the main camera
                raycaster = mainCamera.gameObject.AddComponent<PhysicsRaycaster>();
                Debug.Log("Added PhysicsRaycaster to main camera for MediaScreen click detection.");
            }
            
            // Ensure EventSystem exists
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();
                Debug.Log("Created EventSystem for MediaScreen click detection.");
            }
            
            // Log raycasting setup for debugging
            LogRaycastingSetup();
        }
        
        // Debug method to log raycasting setup
        private void LogRaycastingSetup()
        {
            // Log camera setup
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                Debug.Log($"Main camera found: {mainCamera.name}, position: {mainCamera.transform.position}");
                
                PhysicsRaycaster raycaster = mainCamera.GetComponent<PhysicsRaycaster>();
                if (raycaster != null)
                {
                    Debug.Log($"PhysicsRaycaster found on camera: {mainCamera.name}, eventMask: {raycaster.eventMask}");
                }
                else
                {
                    Debug.LogWarning($"No PhysicsRaycaster found on camera: {mainCamera.name}");
                }
            }
            else
            {
                Debug.LogWarning("No main camera found.");
            }
            
            // Log EventSystem setup
            EventSystem eventSystem = FindAnyObjectByType<EventSystem>();
            if (eventSystem != null)
            {
                Debug.Log($"EventSystem found: {eventSystem.name}, isActive: {eventSystem.isActiveAndEnabled}");
                
                StandaloneInputModule inputModule = eventSystem.GetComponent<StandaloneInputModule>();
                if (inputModule != null)
                {
                    Debug.Log($"StandaloneInputModule found on EventSystem: {eventSystem.name}");
                }
                else
                {
                    Debug.LogWarning($"No StandaloneInputModule found on EventSystem: {eventSystem.name}");
                }
            }
            else
            {
                Debug.LogWarning("No EventSystem found.");
            }
            
            // Log this object's setup
            Collider collider = GetComponent<Collider>();
            if (collider != null)
            {
                Debug.Log($"Collider found on {gameObject.name}: type: {collider.GetType().Name}, enabled: {collider.enabled}, isTrigger: {collider.isTrigger}");
            }
            else
            {
                Debug.LogWarning($"No Collider found on {gameObject.name}");
            }
        }

        private void OnEnable()
        {
            // Subscribe to the OnReactMediaScreen event
            ReactIncomingEvent.OnReactMediaScreen += HandleMediaScreen;
            
            // Subscribe to the OnReactSetMediaScreenImage event
            ReactIncomingEvent.OnReactSetMediaScreenImage += HandleSetMediaScreenImage;
            
            // Subscribe to the OnReactSetMediaScreenThumbnail event
            ReactIncomingEvent.OnReactSetMediaScreenThumbnail += HandleSetMediaScreenThumbnail;
            
            // Subscribe to the OnReactForceUpdateMediaScreen event
            ReactIncomingEvent.OnReactForceUpdateMediaScreen += HandleForceUpdateMediaScreen;
            
            // Register this MediaScreen with React
            RegisterWithReact();
        }

        private void OnDisable()
        {
            // Unsubscribe from the OnReactMediaScreen event
            ReactIncomingEvent.OnReactMediaScreen -= HandleMediaScreen;
            
            // Unsubscribe from the OnReactSetMediaScreenImage event
            ReactIncomingEvent.OnReactSetMediaScreenImage -= HandleSetMediaScreenImage;
            
            // Unsubscribe from the OnReactSetMediaScreenThumbnail event
            ReactIncomingEvent.OnReactSetMediaScreenThumbnail -= HandleSetMediaScreenThumbnail;
            
            // Unsubscribe from the OnReactForceUpdateMediaScreen event
            ReactIncomingEvent.OnReactForceUpdateMediaScreen -= HandleForceUpdateMediaScreen;
            
            // Cancel any active coroutines
            if (activeImageLoadCoroutine != null)
            {
                StopCoroutine(activeImageLoadCoroutine);
                activeImageLoadCoroutine = null;
            }
        }

        public void RegisterWithReact()
        {
            // Create registration data
            MediaScreenRegistrationData data = new MediaScreenRegistrationData
            {
                mediaScreenId = screenId,
                position = JsonUtility.ToJson(transform.position),
                rotation = JsonUtility.ToJson(transform.rotation),
                scale = JsonUtility.ToJson(transform.localScale),
                currentImageUrl = currentImageUrl,
                hasImage = hasImage
            };

            // Register with React
            MediaScreenRegistration.RegisterMediaScreen(data);
        }

        private void HandleMediaScreen(MediaScreenData data)
        {
            // Check if the incoming data matches this MediaScreen's ID
            if (data.mediaScreenId == screenId)
            {
                Debug.Log($"Setting image for {screenId} with URL: {data.imageUrl}");
                
                // Update the current image URL
                currentImageUrl = data.imageUrl;
                
                // Start loading the image as a texture
                LoadImageWithCacheControl(data.imageUrl, false, 0);
                
                /*// Hide play button for regular images
                if (playButtonOverlay != null)
                {
                    playButtonOverlay.SetActive(false);
                }*/
            }
        }
        
        private void HandleSetMediaScreenImage(MediaScreenImageData data)
        {
            // Check if the incoming data matches this MediaScreen's ID
            if (data.mediaScreenId == screenId)
            {
                Debug.Log($"Setting media for {screenId} with URL: {data.imageUrl}, type: {data.mediaType}, displayAsVideo: {data.displayAsVideo}, refreshTimestamp: {data.refreshTimestamp}");
                
                // Check if we need to clear the current state due to mode change
                bool modeChanged = (isVideo != (data.mediaType == "video")) || (displayAsVideo != data.displayAsVideo);
                
                // Store both URLs
                currentImageUrl = data.imageUrl;
                currentVideoUrl = data.videoUrl;  // Store the video URL
                isVideo = data.mediaType == "video";
                displayAsVideo = data.displayAsVideo;
                
                // If mode changed or refresh timestamp is newer, clear the current texture
                bool shouldForceRefresh = modeChanged || (data.refreshTimestamp > lastImageRefreshTimestamp);
                
                if (shouldForceRefresh && targetRenderer != null && targetRenderer.material != null)
                {
                    // Clear the current texture
                    targetRenderer.material.mainTexture = null;
                    hasImage = false;
                    
                    // Update the timestamp
                    lastImageRefreshTimestamp = data.refreshTimestamp;
                }
                
                // For images, load directly
                if (data.mediaType == "image")
                {
                    // Start loading the image as a texture
                    LoadImageWithCacheControl(data.imageUrl, shouldForceRefresh, data.refreshTimestamp);
                    
                    /*// Hide play button for regular images
                    if (playButtonOverlay != null)
                    {
                        playButtonOverlay.SetActive(false);
                    }*/
                }
                // For videos, we'll wait for the thumbnail event
                else
                {
                    // Don't try to load the video URL directly
                    // Just update the state and wait for the thumbnail
                    
                    /*// Show play button for videos if displayAsVideo is true
                    if (playButtonOverlay != null)
                    {
                        playButtonOverlay.SetActive(displayAsVideo);
                    }*/
                }
            }
        }
        
        private void HandleSetMediaScreenThumbnail(MediaScreenThumbnailData data)
        {
            // Check if the incoming data matches this MediaScreen's ID
            if (data.mediaScreenId == screenId)
            {
                Debug.Log($"Setting thumbnail for {screenId} with URL: {data.thumbnailUrl}, displayAsVideo: {data.displayAsVideo}, refreshTimestamp: {data.refreshTimestamp}");
                
                // Update the current thumbnail URL
                currentThumbnailUrl = data.thumbnailUrl;
                displayAsVideo = data.displayAsVideo;
                isVideo = true;
                
                // Check if we should force a refresh
                bool shouldForceRefresh = data.refreshTimestamp > lastThumbnailRefreshTimestamp;
                
                if (shouldForceRefresh && targetRenderer != null && targetRenderer.material != null)
                {
                    // Clear the current texture
                    targetRenderer.material.mainTexture = null;
                    
                    // Update the timestamp
                    lastThumbnailRefreshTimestamp = data.refreshTimestamp;
                }
                
                // Start loading the thumbnail as a texture
                LoadImageWithCacheControl(data.thumbnailUrl, shouldForceRefresh, data.refreshTimestamp);
                
                /*// Show play button based on displayAsVideo flag
                if (playButtonOverlay != null)
                {
                    playButtonOverlay.SetActive(displayAsVideo);
                }*/
            }
        }
        
        private void HandleForceUpdateMediaScreen(ForceUpdateMediaScreenData data)
        {
            // Check if the incoming data matches this MediaScreen's ID
            if (data.mediaScreenId == screenId)
            {
                Debug.Log($"Force updating media screen {screenId}, displayAsVideo: {data.displayAsVideo}, refreshTimestamp: {data.refreshTimestamp}");
                
                // Update display mode
                displayAsVideo = data.displayAsVideo;
                
                // Clear current texture
                if (targetRenderer != null && targetRenderer.material != null)
                {
                    targetRenderer.material.mainTexture = null;
                    hasImage = false;
                }
                
                // Update timestamps
                lastImageRefreshTimestamp = data.refreshTimestamp;
                lastThumbnailRefreshTimestamp = data.refreshTimestamp;
                
                // Reload the appropriate content based on the current state
                if (isVideo && displayAsVideo)
                {
                    // For video mode, load the thumbnail
                    if (!string.IsNullOrEmpty(currentThumbnailUrl))
                    {
                        LoadImageWithCacheControl(currentThumbnailUrl, true, data.refreshTimestamp);
                    }
                    
                    /*// Show play button
                    if (playButtonOverlay != null)
                    {
                        playButtonOverlay.SetActive(true);
                    }*/
                }
                else
                {
                    // For image mode, load the image
                    if (!string.IsNullOrEmpty(currentImageUrl))
                    {
                        LoadImageWithCacheControl(currentImageUrl, true, data.refreshTimestamp);
                    }
                    
                    /*// Hide play button
                    if (playButtonOverlay != null)
                    {
                        playButtonOverlay.SetActive(false);
                    }*/
                }
            }
        }
        
        // Helper method to load images with cache control
        private void LoadImageWithCacheControl(string imageUrl, bool forceRefresh, long timestamp)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                return;
            }
            
            // Cancel any active image loading coroutine
            if (activeImageLoadCoroutine != null)
            {
                StopCoroutine(activeImageLoadCoroutine);
                activeImageLoadCoroutine = null;
            }
            
            // Start a new coroutine to load the image
            activeImageLoadCoroutine = StartCoroutine(LoadImage(imageUrl, forceRefresh, timestamp));
        }

        private IEnumerator LoadImage(string imageUrl, bool forceRefresh, long timestamp)
        {
            // Add cache-busting parameter if forcing refresh
            string urlToLoad = imageUrl;
            if (forceRefresh)
            {
                // Add a timestamp parameter to bypass caching
                char separator = imageUrl.Contains("?") ? '&' : '?';
                urlToLoad = $"{imageUrl}{separator}t={timestamp}";
            }
            
            Debug.Log($"Loading image from URL: {urlToLoad} (forceRefresh: {forceRefresh})");
            
            using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(urlToLoad))
            {
                // Set no-cache headers if forcing refresh
                if (forceRefresh)
                {
                    uwr.SetRequestHeader("Cache-Control", "no-cache, no-store, must-revalidate");
                    uwr.SetRequestHeader("Pragma", "no-cache");
                    uwr.SetRequestHeader("Expires", "0");
                }
                
                yield return uwr.SendWebRequest();

                if (uwr.result == UnityWebRequest.Result.Success)
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(uwr);

                    // Apply the texture to the material
                    if (targetRenderer != null)
                    {
                        if (targetRenderer.material == null)
                            targetRenderer.material = new Material(Shader.Find("Standard"));
                        
                        targetRenderer.material.mainTexture = texture;
                        hasImage = true;
                    }
                    else
                    {
                        Debug.LogError($"No MeshRenderer found on MediaScreen '{screenId}'.");
                    }
                }
                else
                {
                    Debug.LogError($"Failed to download image: {uwr.error}");
                }
                
                // Clear the active coroutine reference
                activeImageLoadCoroutine = null;
            }
        }
        
        // Public method to set an image directly
        public void SetImage(string imageUrl, bool isVideoContent = false, bool showPlayButton = false)
        {
            if (!string.IsNullOrEmpty(imageUrl))
            {
                // Update the current image URL and media type
                currentImageUrl = imageUrl;
                isVideo = isVideoContent;
                displayAsVideo = showPlayButton;
                
                // Generate a timestamp for cache control
                long timestamp = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                
                // Start loading the image as a texture
                LoadImageWithCacheControl(imageUrl, true, timestamp);
                
                /*// Show/hide play button based on displayAsVideo flag
                if (playButtonOverlay != null)
                {
                    playButtonOverlay.SetActive(displayAsVideo);
                }*/
            }
        }
        
        // Public method to set a thumbnail for video content
        public void SetThumbnail(string thumbnailUrl, bool showPlayButton = true)
        {
            if (!string.IsNullOrEmpty(thumbnailUrl))
            {
                // Update the current thumbnail URL
                currentThumbnailUrl = thumbnailUrl;
                isVideo = true;
                displayAsVideo = showPlayButton;
                
                // Generate a timestamp for cache control
                long timestamp = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                
                // Start loading the thumbnail as a texture
                LoadImageWithCacheControl(thumbnailUrl, true, timestamp);
                
                /*// Show/hide play button based on displayAsVideo flag
                if (playButtonOverlay != null)
                {
                    playButtonOverlay.SetActive(displayAsVideo);
                }*/
            }
        }
        
        // Public method to force a refresh of the current content
        public void ForceRefresh()
        {
            // Generate a timestamp for cache control
            long timestamp = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            
            // Create force update data
            ForceUpdateMediaScreenData data = new ForceUpdateMediaScreenData
            {
                mediaScreenId = screenId,
                displayAsVideo = displayAsVideo,
                refreshTimestamp = timestamp
            };
            
            // Handle the force update locally
            HandleForceUpdateMediaScreen(data);
        }
        
        /*// Create a simple play button overlay
        private GameObject CreatePlayButtonOverlay()
        {
            GameObject playButton = new GameObject("PlayButton");
            playButton.transform.SetParent(transform);
            
            // Position the play button in front of the screen
            playButton.transform.localPosition = new Vector3(0, 0, -0.01f);
            
            // Add a quad for the play button
            MeshFilter meshFilter = playButton.AddComponent<MeshFilter>();
            meshFilter.mesh = CreatePlayButtonMesh();
            
            // Add a mesh renderer with a play button material
            MeshRenderer meshRenderer = playButton.AddComponent<MeshRenderer>();
            meshRenderer.material = new Material(Shader.Find("Standard"));
            meshRenderer.material.color = new Color(1, 1, 1, 0.8f);
            
            // You could load a play button texture here
            // meshRenderer.material.mainTexture = Resources.Load<Texture2D>("PlayButton");
            
            return playButton;
        }
        
        // Create a simple play button mesh (a quad with a triangle)
        private Mesh CreatePlayButtonMesh()
        {
            Mesh mesh = new Mesh();
            
            // Simple quad vertices
            Vector3[] vertices = new Vector3[4]
            {
                new Vector3(-0.2f, -0.2f, 0),
                new Vector3(0.2f, -0.2f, 0),
                new Vector3(0.2f, 0.2f, 0),
                new Vector3(-0.2f, 0.2f, 0)
            };
            
            // Simple quad triangles
            int[] triangles = new int[6]
            {
                0, 2, 1,
                0, 3, 2
            };
            
            // Simple quad UVs
            Vector2[] uv = new Vector2[4]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(1, 1),
                new Vector2(0, 1)
            };
            
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uv;
            mesh.RecalculateNormals();
            
            return mesh;
        }*/
        
        // Support for Unity's EventSystem click detection
        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log($"OnPointerClick detected on {screenId}");
            HandleClick();
        }
        
        // Support for Unity's built-in click detection
        private void OnMouseDown()
        {
            Debug.Log($"OnMouseDown detected on {screenId}");
            HandleClick();
        }
        
        // Fallback for WebGL where raycasting might be inconsistent
        private void Update()
        {
            // Check for mouse click
            if (Input.GetMouseButtonDown(0))
            {
                // Cast a ray from the camera to the mouse position
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                
                // Check if the ray hits this object
                if (Physics.Raycast(ray, out hit) && hit.transform == transform)
                {
                    Debug.Log($"Manual raycast detected click on {screenId}");
                    HandleClick();
                }
            }
        }
        
        // Common method to handle clicks
        private void HandleClick()
        {
            Debug.Log($"MediaScreen clicked: {screenId}, isVideo: {isVideo}, displayAsVideo: {displayAsVideo}");
            
            if (isVideo && displayAsVideo)
            {
                // If it's a video and should be displayed as video, send play video event
                Debug.Log($"Playing video for MediaScreen: {screenId}");
                PlayMediaScreenVideoData playVideoData = new PlayMediaScreenVideoData
                {
                    mediaScreenId = screenId,
                    videoUrl = currentVideoUrl  // Include the video URL in the event
                };
                PlayMediaScreenVideo.SendPlayMediaScreenVideo(playVideoData);
            }
            else
            {
                // Otherwise, just send a click event
                Debug.Log($"Sending click event for MediaScreen: {screenId}");
                MediaScreenClickData clickData = new MediaScreenClickData
                {
                    mediaScreenId = screenId,
                    position = JsonUtility.ToJson(transform.position)
                };
                MediaScreenClick.SendMediaScreenClick(clickData);
            }
        }
    }
} 