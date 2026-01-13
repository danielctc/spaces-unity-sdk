using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.EventSystems;

namespace Spaces.React.Runtime
{
    /// <summary>
    /// Manages video canvas instances in the scene.
    /// Listens for PlaceVideoCanvas, UpdateVideoCanvas, DeleteVideoCanvas events from React.
    /// </summary>
    public class VideoCanvasRenderer : MonoBehaviour
    {
        [Header("Prefab Settings")]
        [Tooltip("Prefab to instantiate for video canvases. Should have a MeshRenderer component.")]
        public GameObject videoCanvasPrefab;

        [Header("Default Material")]
        [Tooltip("Default material for video canvas. Uses Standard shader if not set.")]
        public Material defaultMaterial;

        // Track all instantiated video canvases
        private Dictionary<string, GameObject> videoCanvases = new Dictionary<string, GameObject>();
        private Dictionary<string, VideoCanvasInstance> canvasInstances = new Dictionary<string, VideoCanvasInstance>();

        private void Awake()
        {
            // Create a default prefab if none is assigned
            if (videoCanvasPrefab == null)
            {
                videoCanvasPrefab = CreateDefaultVideoPrefab();
            }
        }

        private void OnEnable()
        {
            // Subscribe to video canvas events
            ReactIncomingEvent.OnPlaceVideoCanvas += HandlePlaceVideoCanvas;
            ReactIncomingEvent.OnUpdateVideoCanvas += HandleUpdateVideoCanvas;
            ReactIncomingEvent.OnDeleteVideoCanvas += HandleDeleteVideoCanvas;

            Debug.Log("[VideoCanvasRenderer] Subscribed to video canvas events");
        }

        private void OnDisable()
        {
            // Unsubscribe from events
            ReactIncomingEvent.OnPlaceVideoCanvas -= HandlePlaceVideoCanvas;
            ReactIncomingEvent.OnUpdateVideoCanvas -= HandleUpdateVideoCanvas;
            ReactIncomingEvent.OnDeleteVideoCanvas -= HandleDeleteVideoCanvas;
        }

        /// <summary>
        /// Handle PlaceVideoCanvas event from React
        /// </summary>
        private void HandlePlaceVideoCanvas(VideoCanvasData data)
        {
            if (string.IsNullOrEmpty(data.canvasId))
            {
                Debug.LogError("[VideoCanvasRenderer] Received PlaceVideoCanvas with empty canvasId");
                return;
            }

            Debug.Log($"[VideoCanvasRenderer] Placing video canvas: {data.canvasId}");

            // Check if canvas already exists
            if (videoCanvases.ContainsKey(data.canvasId))
            {
                Debug.Log($"[VideoCanvasRenderer] Video canvas {data.canvasId} already exists, updating instead");
                HandleUpdateVideoCanvas(new VideoCanvasUpdateData
                {
                    canvasId = data.canvasId,
                    videoUrl = data.videoUrl,
                    videoType = data.videoType,
                    aspectRatio = data.aspectRatio,
                    autoplay = data.autoplay,
                    loop = data.loop,
                    muted = data.muted,
                    position = data.position,
                    rotation = data.rotation,
                    scale = data.scale
                });
                return;
            }

            // Instantiate the video canvas prefab
            GameObject canvasObj = Instantiate(videoCanvasPrefab, transform);
            canvasObj.name = $"VideoCanvas_{data.canvasId}";

            // Set transform
            if (data.position != null)
                canvasObj.transform.position = data.position.ToVector3();
            if (data.rotation != null)
                canvasObj.transform.eulerAngles = data.rotation.ToVector3();
            if (data.scale != null)
                canvasObj.transform.localScale = GetScaleForAspectRatio(data.aspectRatio, data.scale.ToVector3());

            // Create canvas instance tracker
            var instance = canvasObj.AddComponent<VideoCanvasInstance>();
            instance.Initialize(data.canvasId, data.videoUrl, data.videoType, data.aspectRatio, data.autoplay, data.loop, data.muted);

            // Store references
            videoCanvases[data.canvasId] = canvasObj;
            canvasInstances[data.canvasId] = instance;

            Debug.Log($"[VideoCanvasRenderer] Video canvas {data.canvasId} created at position {canvasObj.transform.position}");
        }

        /// <summary>
        /// Handle UpdateVideoCanvas event from React
        /// </summary>
        private void HandleUpdateVideoCanvas(VideoCanvasUpdateData data)
        {
            if (string.IsNullOrEmpty(data.canvasId))
            {
                Debug.LogError("[VideoCanvasRenderer] Received UpdateVideoCanvas with empty canvasId");
                return;
            }

            if (!videoCanvases.TryGetValue(data.canvasId, out GameObject canvasObj))
            {
                Debug.LogWarning($"[VideoCanvasRenderer] Video canvas {data.canvasId} not found for update");
                return;
            }

            Debug.Log($"[VideoCanvasRenderer] Updating video canvas: {data.canvasId}");

            // Update transform
            if (data.position != null)
                canvasObj.transform.position = data.position.ToVector3();
            if (data.rotation != null)
                canvasObj.transform.eulerAngles = data.rotation.ToVector3();
            if (data.scale != null)
                canvasObj.transform.localScale = GetScaleForAspectRatio(data.aspectRatio, data.scale.ToVector3());

            // Update video settings
            if (canvasInstances.TryGetValue(data.canvasId, out VideoCanvasInstance instance))
            {
                instance.UpdateSettings(data.videoUrl, data.videoType, data.aspectRatio, data.autoplay, data.loop, data.muted);
            }
        }

        /// <summary>
        /// Handle DeleteVideoCanvas event from React
        /// </summary>
        private void HandleDeleteVideoCanvas(VideoCanvasDeleteData data)
        {
            if (string.IsNullOrEmpty(data.canvasId))
            {
                Debug.LogError("[VideoCanvasRenderer] Received DeleteVideoCanvas with empty canvasId");
                return;
            }

            Debug.Log($"[VideoCanvasRenderer] Deleting video canvas: {data.canvasId}");

            if (videoCanvases.TryGetValue(data.canvasId, out GameObject canvasObj))
            {
                Destroy(canvasObj);
                videoCanvases.Remove(data.canvasId);
                canvasInstances.Remove(data.canvasId);
                Debug.Log($"[VideoCanvasRenderer] Video canvas {data.canvasId} destroyed");
            }
            else
            {
                Debug.LogWarning($"[VideoCanvasRenderer] Video canvas {data.canvasId} not found for deletion");
            }
        }

        /// <summary>
        /// Create a default quad prefab for video canvases
        /// </summary>
        private GameObject CreateDefaultVideoPrefab()
        {
            GameObject prefab = GameObject.CreatePrimitive(PrimitiveType.Quad);
            prefab.name = "VideoCanvasPrefab";

            // Set up material
            MeshRenderer renderer = prefab.GetComponent<MeshRenderer>();
            if (defaultMaterial != null)
            {
                renderer.material = defaultMaterial;
            }
            else
            {
                renderer.material = new Material(Shader.Find("Standard"));
                renderer.material.color = Color.black;
            }

            // Add collider for click detection
            if (prefab.GetComponent<Collider>() == null)
            {
                prefab.AddComponent<BoxCollider>();
            }

            // Disable initially (it's a template)
            prefab.SetActive(false);

            return prefab;
        }

        /// <summary>
        /// Calculate scale based on aspect ratio
        /// </summary>
        private Vector3 GetScaleForAspectRatio(string aspectRatio, Vector3 baseScale)
        {
            float ratio = 16f / 9f; // Default 16:9

            switch (aspectRatio)
            {
                case "16:9":
                    ratio = 16f / 9f;
                    break;
                case "4:3":
                    ratio = 4f / 3f;
                    break;
                case "1:1":
                    ratio = 1f;
                    break;
                case "9:16":
                    ratio = 9f / 16f;
                    break;
            }

            // Apply ratio to X scale, keep Y and Z as specified
            return new Vector3(baseScale.x * ratio, baseScale.y, baseScale.z);
        }
    }

    /// <summary>
    /// Component attached to each video canvas instance
    /// Handles click detection and video playback state
    /// </summary>
    public class VideoCanvasInstance : MonoBehaviour, IPointerClickHandler
    {
        public string CanvasId { get; private set; }
        public string VideoUrl { get; private set; }
        public string VideoType { get; private set; }
        public string AspectRatio { get; private set; }
        public bool Autoplay { get; private set; }
        public bool Loop { get; private set; }
        public bool Muted { get; private set; }

        private MeshRenderer meshRenderer;
        private Coroutine thumbnailLoadCoroutine;

        public void Initialize(string canvasId, string videoUrl, string videoType, string aspectRatio, bool autoplay, bool loop, bool muted)
        {
            CanvasId = canvasId;
            VideoUrl = videoUrl;
            VideoType = videoType;
            AspectRatio = aspectRatio;
            Autoplay = autoplay;
            Loop = loop;
            Muted = muted;

            meshRenderer = GetComponent<MeshRenderer>();

            // Load thumbnail based on video type
            LoadThumbnail();

            Debug.Log($"[VideoCanvasInstance] Initialized canvas {canvasId} with video {videoUrl}");
        }

        public void UpdateSettings(string videoUrl, string videoType, string aspectRatio, bool autoplay, bool loop, bool muted)
        {
            bool videoChanged = VideoUrl != videoUrl;

            VideoUrl = videoUrl;
            VideoType = videoType;
            AspectRatio = aspectRatio;
            Autoplay = autoplay;
            Loop = loop;
            Muted = muted;

            if (videoChanged)
            {
                LoadThumbnail();
            }
        }

        private void LoadThumbnail()
        {
            if (string.IsNullOrEmpty(VideoUrl)) return;

            // Cancel any existing thumbnail load
            if (thumbnailLoadCoroutine != null)
            {
                StopCoroutine(thumbnailLoadCoroutine);
            }

            string thumbnailUrl = GetThumbnailUrl();
            if (!string.IsNullOrEmpty(thumbnailUrl))
            {
                thumbnailLoadCoroutine = StartCoroutine(LoadThumbnailCoroutine(thumbnailUrl));
            }
        }

        private string GetThumbnailUrl()
        {
            switch (VideoType)
            {
                case "youtube":
                    string videoId = ExtractYouTubeVideoId(VideoUrl);
                    if (!string.IsNullOrEmpty(videoId))
                    {
                        return $"https://img.youtube.com/vi/{videoId}/maxresdefault.jpg";
                    }
                    break;
                case "vimeo":
                    // Vimeo thumbnails require API call, handled by React
                    break;
                case "direct":
                case "hls":
                    // No automatic thumbnail for direct videos
                    break;
            }
            return null;
        }

        private string ExtractYouTubeVideoId(string url)
        {
            if (string.IsNullOrEmpty(url)) return null;

            // Handle various YouTube URL formats
            if (url.Contains("youtube.com/watch"))
            {
                int startIndex = url.IndexOf("v=") + 2;
                int endIndex = url.IndexOf("&", startIndex);
                if (endIndex == -1) endIndex = url.Length;
                return url.Substring(startIndex, endIndex - startIndex);
            }
            else if (url.Contains("youtu.be/"))
            {
                int startIndex = url.IndexOf("youtu.be/") + 9;
                int endIndex = url.IndexOf("?", startIndex);
                if (endIndex == -1) endIndex = url.Length;
                return url.Substring(startIndex, endIndex - startIndex);
            }

            return null;
        }

        private IEnumerator LoadThumbnailCoroutine(string thumbnailUrl)
        {
            Debug.Log($"[VideoCanvasInstance] Loading thumbnail from {thumbnailUrl}");

            using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(thumbnailUrl))
            {
                yield return uwr.SendWebRequest();

                if (uwr.result == UnityWebRequest.Result.Success)
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(uwr);

                    if (meshRenderer != null)
                    {
                        if (meshRenderer.material == null)
                        {
                            meshRenderer.material = new Material(Shader.Find("Standard"));
                        }
                        meshRenderer.material.mainTexture = texture;
                        Debug.Log($"[VideoCanvasInstance] Thumbnail loaded for {CanvasId}");
                    }
                }
                else
                {
                    Debug.LogWarning($"[VideoCanvasInstance] Failed to load thumbnail: {uwr.error}");
                }
            }

            thumbnailLoadCoroutine = null;
        }

        // Click detection via EventSystem
        public void OnPointerClick(PointerEventData eventData)
        {
            HandleClick();
        }

        // Click detection via OnMouseDown
        private void OnMouseDown()
        {
            HandleClick();
        }

        private void HandleClick()
        {
            Debug.Log($"[VideoCanvasInstance] Video canvas clicked: {CanvasId}");

            // Send click event to React
            VideoCanvasClickData clickData = new VideoCanvasClickData
            {
                canvasId = CanvasId,
                videoUrl = VideoUrl,
                position = JsonUtility.ToJson(transform.position)
            };

            // Use ReactRaiseEvent pattern to send to React
            VideoCanvasClick.SendVideoCanvasClick(clickData);
        }

        private void OnDisable()
        {
            if (thumbnailLoadCoroutine != null)
            {
                StopCoroutine(thumbnailLoadCoroutine);
                thumbnailLoadCoroutine = null;
            }
        }
    }

    /// <summary>
    /// Static class for sending video canvas click events to React
    /// </summary>
    public static class VideoCanvasClick
    {
        public static void SendVideoCanvasClick(VideoCanvasClickData data)
        {
            string jsonData = JsonUtility.ToJson(data);
            Debug.Log($"[VideoCanvasClick] Sending click event: {jsonData}");

            #if UNITY_WEBGL && !UNITY_EDITOR
            ReactRaiseEvent.JsVideoCanvasClicked(jsonData);
            #else
            Debug.Log($"[VideoCanvasClick] Would send to React: VideoCanvasClicked with data {jsonData}");
            #endif
        }
    }
}
