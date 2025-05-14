using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.EventSystems;
using Spaces.React.Runtime;

namespace Spaces.React.Runtime
{
    [RequireComponent(typeof(MeshRenderer))]
    public class PortalManager : MonoBehaviour, IPointerClickHandler
    {
        // Static dictionary to track all portal instances by their ID
        private static Dictionary<string, PortalManager> portalInstances = new Dictionary<string, PortalManager>();

        [Tooltip("The target mesh where the portal image will be rendered.")]
        public MeshRenderer targetRenderer;

        [Tooltip("Unique identifier for this portal (required for React)")]
        public string portalId = "";

        // Track the current portal state
        private string currentImageUrl = "";
        private bool hasImage = false;
        
        // Track active coroutines for cancellation
        private Coroutine activeImageLoadCoroutine = null;
        private bool isRegistered = false;

        private void Awake()
        {
            // Ensure we have a target renderer
            if (targetRenderer == null)
            {
                targetRenderer = GetComponent<MeshRenderer>();
                Debug.Log($"[PortalManager] Auto-assigned target renderer: {(targetRenderer != null ? "Success" : "Failed")}");
            }
            
            // Ensure we have a collider for click detection
            if (GetComponent<Collider>() == null)
            {
                gameObject.AddComponent<BoxCollider>();
                Debug.Log($"[PortalManager] Added BoxCollider to portal {portalId}");
            }
            
            // Ensure the main camera has a PhysicsRaycaster for click detection
            EnsurePhysicsRaycasterExists();
        }

        // Helper method to ensure a PhysicsRaycaster exists on the main camera
        private void EnsurePhysicsRaycasterExists()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogWarning("No main camera found. Click events may not work.");
                return;
            }

            if (mainCamera.GetComponent<PhysicsRaycaster>() == null)
            {
                mainCamera.gameObject.AddComponent<PhysicsRaycaster>();
                Debug.Log("Added PhysicsRaycaster to main camera for Portal click detection.");
            }
            
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();
                Debug.Log("Created EventSystem for Portal click detection.");
            }
        }

        private void OnEnable()
        {
            Debug.Log($"[PortalManager] OnEnable called for portal {portalId}");
            
            // Subscribe to events
            ReactIncomingEvent.OnReactPortal += HandlePortal;
            ReactIncomingEvent.OnSetPortalImage += HandlePortal;
            ReactIncomingEvent.OnUpdatePortalTransform += HandleTransformUpdate;
            
            // Only register if we have a portalId
            if (!string.IsNullOrEmpty(portalId))
            {
                RegisterWithReact();
            }
        }

        private void OnDisable()
        {
            // Unsubscribe from events
            ReactIncomingEvent.OnReactPortal -= HandlePortal;
            ReactIncomingEvent.OnSetPortalImage -= HandlePortal;
            ReactIncomingEvent.OnUpdatePortalTransform -= HandleTransformUpdate;
            
            // Unregister this portal instance
            if (!string.IsNullOrEmpty(portalId) && portalInstances.ContainsKey(portalId))
            {
                portalInstances.Remove(portalId);
                Debug.Log($"[PortalManager] Unregistered portal {portalId} from portal dictionary");
            }
            
            // Cancel any active coroutines
            if (activeImageLoadCoroutine != null)
            {
                StopCoroutine(activeImageLoadCoroutine);
                activeImageLoadCoroutine = null;
            }
        }

        // Public method to set the portal ID and register with React
        public void SetPortalId(string newPortalId)
        {
            if (string.IsNullOrEmpty(newPortalId))
            {
                Debug.LogError("[PortalManager] Cannot set empty portalId!");
                return;
            }

            // If we already have a portalId, unregister the old one
            if (!string.IsNullOrEmpty(portalId) && portalInstances.ContainsKey(portalId))
            {
                portalInstances.Remove(portalId);
            }

            portalId = newPortalId;
            Debug.Log($"[PortalManager] Set portal ID to: {portalId}");

            // Register this portal instance in the static dictionary
            portalInstances[portalId] = this;
            Debug.Log($"[PortalManager] Registered portal {portalId} in portal dictionary");

            // Register with React
            RegisterWithReact();
        }

        // Static method to find a portal by ID
        public static PortalManager GetPortalById(string id)
        {
            if (portalInstances.TryGetValue(id, out PortalManager portal))
            {
                return portal;
            }
            Debug.LogWarning($"[PortalManager] No portal found with ID: {id}");
            return null;
        }

        public void RegisterWithReact()
        {
            if (string.IsNullOrEmpty(portalId))
            {
                Debug.LogError("[PortalManager] Cannot register portal with React: portalId is empty!");
                return;
            }

            PortalRegistrationData data = new PortalRegistrationData
            {
                portalId = portalId,
                position = JsonUtility.ToJson(transform.position),
                rotation = JsonUtility.ToJson(transform.rotation),
                scale = JsonUtility.ToJson(transform.localScale),
                currentImageUrl = currentImageUrl,
                hasImage = hasImage
            };

            Debug.Log($"[PortalManager] Registering Portal {portalId} with React: Position={transform.position}, HasImage={hasImage}, ImageUrl={currentImageUrl}");
            PortalRegistration.RegisterPortal(data);
            isRegistered = true;
        }

        private void HandlePortal(PortalData data)
        {
            Debug.Log($"[PortalManager] Received portal update - This portal ID: {portalId}, Incoming portal ID: {data.portalId}");
            
            if (data.portalId == portalId)
            {
                Debug.Log($"[PortalManager] Portal {portalId} received image update: {data.imageUrl}");
                
                // Store the current image URL
                currentImageUrl = data.imageUrl;
                
                // Use a timestamp for cache-busting by default for portals
                long timestamp = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                LoadImageWithCacheControl(data.imageUrl, true, timestamp);
            }
            else
            {
                Debug.Log($"[PortalManager] Portal {portalId} received update for different portal {data.portalId} - ignoring");
            }
        }
        
        // Helper method to load images with cache control
        private void LoadImageWithCacheControl(string imageUrl, bool forceRefresh, long timestamp)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                return;
            }
            
            if (activeImageLoadCoroutine != null)
            {
                StopCoroutine(activeImageLoadCoroutine);
                activeImageLoadCoroutine = null;
            }
            
            activeImageLoadCoroutine = StartCoroutine(LoadImage(imageUrl, forceRefresh, timestamp));
        }

        private IEnumerator LoadImage(string imageUrl, bool forceRefresh, long timestamp)
        {
            string urlToLoad = imageUrl;
            if (forceRefresh)
            {
                char separator = imageUrl.Contains("?") ? '&' : '?';
                urlToLoad = $"{imageUrl}{separator}t={timestamp}";
            }
            
            Debug.Log($"[PortalManager] Loading image for Portal {portalId} from URL: {urlToLoad}");
            
            using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(urlToLoad))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(www);
                    
                    // Get or create material with our custom shader
                    if (targetRenderer.material == null)
                    {
                        targetRenderer.material = new Material(Shader.Find("Custom/BillboardPortal"));
                        Debug.Log($"[PortalManager] Created new BillboardPortal material for Portal {portalId}");
                    }
                    else if (targetRenderer.material.shader.name != "Custom/BillboardPortal")
                    {
                        // If material exists but has wrong shader, create new one
                        Material newMaterial = new Material(Shader.Find("Custom/BillboardPortal"));
                        // Copy any existing properties
                        if (targetRenderer.material.HasProperty("_MainTex"))
                            newMaterial.mainTexture = targetRenderer.material.mainTexture;
                        if (targetRenderer.material.HasProperty("_EmissionColor"))
                            newMaterial.SetColor("_EmissionColor", targetRenderer.material.GetColor("_EmissionColor"));
                        if (targetRenderer.material.HasProperty("_GlowColor"))
                            newMaterial.SetColor("_GlowColor", targetRenderer.material.GetColor("_GlowColor"));
                        if (targetRenderer.material.HasProperty("_GlowPower"))
                            newMaterial.SetFloat("_GlowPower", targetRenderer.material.GetFloat("_GlowPower"));
                        if (targetRenderer.material.HasProperty("_GlowScale"))
                            newMaterial.SetFloat("_GlowScale", targetRenderer.material.GetFloat("_GlowScale"));
                        
                        targetRenderer.material = newMaterial;
                        Debug.Log($"[PortalManager] Updated material to use BillboardPortal shader for Portal {portalId}");
                    }

                    // Apply the texture
                    targetRenderer.material.mainTexture = texture;
                    hasImage = true;

                    Debug.Log($"Successfully applied texture to portal material");
                }
                else
                {
                    Debug.LogError($"Failed to load image: {www.error}");
                }
            }
        }
        
        // Public method to set an image directly (e.g., for testing or non-React scenarios)
        public void SetImage(string imageUrl)
        {
            if (!string.IsNullOrEmpty(imageUrl))
            {
                currentImageUrl = imageUrl;
                long timestamp = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                LoadImageWithCacheControl(imageUrl, true, timestamp);
            }
        }
        
        // --- Click Handling --- 
        
        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log($"OnPointerClick detected on Portal {portalId}");
            HandleClick();
        }
        
        private void OnMouseDown()
        {
             // Optional: Add check if EventSystem handled the click already
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
            
            Debug.Log($"OnMouseDown detected on Portal {portalId}");
            HandleClick();
        }
        
        // Fallback for WebGL
        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                // Optional: Add check if EventSystem handled the click already
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
                
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                
                if (Physics.Raycast(ray, out hit) && hit.transform == transform)
                {
                    Debug.Log($"Manual raycast detected click on Portal {portalId}");
                    HandleClick();
                }
            }
        }
        
        private void HandleClick()
        {
            Debug.Log($"Sending click event for Portal: {portalId}");
            PortalClickData clickData = new PortalClickData
            {
                portalId = portalId,
                position = transform.position,
                rotation = transform.rotation.eulerAngles,
                scale = transform.localScale
            };
            PortalClick.SendPortalClick(clickData);
        }

        // Add new method to handle transform updates
        private void HandleTransformUpdate(PortalTransformData data)
        {
            if (data.portalId == portalId)
            {
                // Log the incoming data
                Debug.Log($"Received transform update for Portal {portalId}:");
                Debug.Log($"Incoming position: {data.position.x}, {data.position.y}, {data.position.z}");
                Debug.Log($"Current position: {transform.position}");
                
                // Update transform properties
                Vector3 newPosition = data.position.ToVector3();
                transform.position = newPosition;
                transform.rotation = Quaternion.Euler(data.rotation.ToVector3());
                transform.localScale = data.scale.ToVector3();
                
                // Log the result
                Debug.Log($"Updated position to: {transform.position}");
                Debug.Log($"Updated transform for Portal {portalId}: Position={transform.position}, Rotation={transform.rotation.eulerAngles}, Scale={transform.localScale}");
            }
        }
    }
} 