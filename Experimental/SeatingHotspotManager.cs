using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using Spaces.React.Runtime;
using System.Collections.Generic;

public class SeatingHotspotManager : MonoBehaviour, IPointerClickHandler
{
    // Static dictionary to track all hotspot instances by their ID
    private static Dictionary<string, SeatingHotspotManager> hotspotInstances = 
        new Dictionary<string, SeatingHotspotManager>();

    private string hotspotId;
    private GameObject currentModel;
    private bool isRegistered = false;

    private GLBDownloader glbDownloader;
    private HotspotDetector hotspotDetector;

    private void Awake()
    {
        glbDownloader = GetComponent<GLBDownloader>();
        if (glbDownloader == null)
        {
            Debug.LogError("[SeatingHotspotManager] No GLBDownloader component found!");
        }

        // Find HotspotDetector in the scene
        hotspotDetector = FindObjectOfType<HotspotDetector>();
        if (hotspotDetector == null)
        {
            Debug.LogWarning("[SeatingHotspotManager] No HotspotDetector found in scene. Hotspot detection will be disabled.");
        }

        // Ensure we have a collider for click detection
        if (GetComponent<Collider>() == null)
        {
            var collider = gameObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            Debug.Log("[SeatingHotspotManager] Added BoxCollider for click detection");
        }

        // Ensure the main camera has a PhysicsRaycaster
        Camera mainCamera = Camera.main;
        if (mainCamera != null && mainCamera.GetComponent<PhysicsRaycaster>() == null)
        {
            mainCamera.gameObject.AddComponent<PhysicsRaycaster>();
        }

        // Ensure we have an EventSystem
        if (FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }
    }

    private void OnEnable()
    {
        Debug.Log($"[SeatingHotspotManager] OnEnable called for hotspot {hotspotId}");
        ReactIncomingEvent.OnReactSeatingHotspot += HandleSeatingHotspot;
        ReactIncomingEvent.OnSetSeatingHotspotModel += HandleSetSeatingHotspotModel;
        ReactIncomingEvent.OnUpdateSeatingHotspotTransform += HandleUpdateSeatingHotspotTransform;
        
        if (!string.IsNullOrEmpty(hotspotId))
        {
            RegisterWithReact();
        }
    }

    private void OnDisable()
    {
        ReactIncomingEvent.OnReactSeatingHotspot -= HandleSeatingHotspot;
        ReactIncomingEvent.OnSetSeatingHotspotModel -= HandleSetSeatingHotspotModel;
        ReactIncomingEvent.OnUpdateSeatingHotspotTransform -= HandleUpdateSeatingHotspotTransform;
        
        if (!string.IsNullOrEmpty(hotspotId) && hotspotInstances.ContainsKey(hotspotId))
        {
            hotspotInstances.Remove(hotspotId);
        }
    }

    private void HandleSeatingHotspot(SeatingHotspotData data)
    {
        Debug.Log($"[SeatingHotspotManager] Received SeatingHotspot event for ID: {data.hotspotId}");
        
        // Set the hotspot ID first
        SetHotspotId(data.hotspotId);
        
        if (glbDownloader != null)
        {
            Debug.Log($"[SeatingHotspotManager] Setting GLB URL to: {data.glbUrl}");
            
            // Set initial transform
            if (data.position != null && data.rotation != null && data.scale != null)
            {
                Vector3 position = new Vector3(data.position.x, data.position.y, data.position.z);
                Quaternion rotation = Quaternion.Euler(data.rotation.x, data.rotation.y, data.rotation.z);
                Vector3 scale = new Vector3(data.scale.x, data.scale.y, data.scale.z);
                
                Debug.Log($"[SeatingHotspotManager] Setting initial position: {position}, rotation: {rotation.eulerAngles}, scale: {scale}");
                transform.position = position;
                transform.rotation = rotation;
                transform.localScale = scale;
            }
            
            // Set the GLB URL and load the model
            glbDownloader.Url = data.glbUrl;
            
            // Setup click detection after model is loaded
            glbDownloader.OnModelLoaded += () => {
                Debug.Log("[SeatingHotspotManager] GLB model loaded, setting up click detection");
                var modelRoot = glbDownloader.GetModelRoot();
                if (modelRoot != null)
                {
                    // Add collider to the model if it doesn't have one
                    if (modelRoot.GetComponent<Collider>() == null)
                    {
                        var collider = modelRoot.AddComponent<BoxCollider>();
                        collider.isTrigger = true;
                        Debug.Log("[SeatingHotspotManager] Added BoxCollider to GLB model for click detection");
                    }

                    // Make all renderers invisible but keep them for click detection
                    var renderers = modelRoot.GetComponentsInChildren<Renderer>();
                    foreach (var renderer in renderers)
                    {
                        renderer.gameObject.layer = LayerMask.NameToLayer("UI");
                        foreach (var material in renderer.materials)
                        {
                            material.color = new Color(1, 1, 1, 0);
                        }
                    }

                    // Ensure the PhysicsRaycaster is set up to detect clicks on the UI layer
                    Camera mainCamera = Camera.main;
                    if (mainCamera != null)
                    {
                        var raycaster = mainCamera.GetComponent<PhysicsRaycaster>();
                        if (raycaster != null)
                        {
                            raycaster.eventMask |= (1 << LayerMask.NameToLayer("UI"));
                        }
                    }

                    // Update the model's transform to match the manager
                    modelRoot.transform.position = transform.position;
                    modelRoot.transform.rotation = transform.rotation;
                    modelRoot.transform.localScale = transform.localScale;
                    Debug.Log("[SeatingHotspotManager] Updated GLB model transform to match manager");
                }
                else
                {
                    Debug.LogError("[SeatingHotspotManager] Failed to get model root after loading");
                }
            };
        }
        else
        {
            Debug.LogError("[SeatingHotspotManager] No GLBDownloader component found!");
        }
    }

    private void HandleSetSeatingHotspotModel(SeatingHotspotData data)
    {
        if (data.hotspotId == hotspotId && glbDownloader != null)
        {
            Debug.Log($"[SeatingHotspotManager] Updating model for hotspot {hotspotId}");
            glbDownloader.Url = data.glbUrl;
        }
    }

    private void HandleUpdateSeatingHotspotTransform(SeatingHotspotTransformData data)
    {
        if (data.hotspotId == hotspotId)
        {
            Debug.Log($"[SeatingHotspotManager] Updating transform for hotspot {hotspotId}");
            UpdateTransform(data);
        }
    }

    private void UpdateTransform(SeatingHotspotTransformData data)
    {
        // Update the manager's transform
        transform.position = new Vector3(data.position.x, data.position.y, data.position.z);
        transform.rotation = Quaternion.Euler(data.rotation.x, data.rotation.y, data.rotation.z);
        transform.localScale = new Vector3(data.scale.x, data.scale.y, data.scale.z);

        // Update the GLB model's transform if it exists
        if (glbDownloader != null && glbDownloader.HasModel)
        {
            var modelRoot = glbDownloader.GetModelRoot();
            if (modelRoot != null)
            {
                modelRoot.transform.position = transform.position;
                modelRoot.transform.rotation = transform.rotation;
                modelRoot.transform.localScale = transform.localScale;
                Debug.Log($"[SeatingHotspotManager] Updated GLB model transform");
            }
        }
    }

    public void SetHotspotId(string id)
    {
        if (hotspotId != id)
        {
            hotspotId = id;
            if (gameObject.activeInHierarchy)
            {
                RegisterWithReact();
            }
        }
    }

    private void RegisterWithReact()
    {
        if (string.IsNullOrEmpty(hotspotId) || isRegistered)
            return;

        var positionData = new Vector3Data { x = transform.position.x, y = transform.position.y, z = transform.position.z };
        var rotationData = new Vector3Data { x = transform.eulerAngles.x, y = transform.eulerAngles.y, z = transform.eulerAngles.z };
        var scaleData = new Vector3Data { x = transform.localScale.x, y = transform.localScale.y, z = transform.localScale.z };

        var registrationData = new SeatingHotspotRegistrationData
        {
            hotspotId = hotspotId,
            position = JsonUtility.ToJson(positionData),
            rotation = JsonUtility.ToJson(rotationData),
            scale = JsonUtility.ToJson(scaleData),
            currentGlbUrl = glbDownloader != null ? glbDownloader.Url : null,
            hasModel = glbDownloader != null && glbDownloader.HasModel
        };

        ReactRaiseEvent.RegisterSeatingHotspot(registrationData);
        hotspotInstances[hotspotId] = this;
        isRegistered = true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"[SeatingHotspotManager] Click detected on hotspot {hotspotId}");
        var clickData = new SeatingHotspotClickData
        {
            hotspotId = hotspotId
        };
        ReactRaiseEvent.SeatingHotspotClicked(clickData);
    }

    private void OnMouseDown()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        
        Debug.Log($"[SeatingHotspotManager] Mouse down detected on hotspot {hotspotId}");
        var clickData = new SeatingHotspotClickData
        {
            hotspotId = hotspotId
        };
        ReactRaiseEvent.SeatingHotspotClicked(clickData);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
            
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit))
            {
                // Check if we hit either the manager or the GLB model
                if (hit.transform == transform || 
                    (glbDownloader != null && glbDownloader.HasModel && hit.transform == glbDownloader.GetModelRoot().transform))
                {
                    Debug.Log($"[SeatingHotspotManager] Raycast hit detected on hotspot {hotspotId}");
                    var clickData = new SeatingHotspotClickData
                    {
                        hotspotId = hotspotId
                    };
                    ReactRaiseEvent.SeatingHotspotClicked(clickData);
                }
            }
        }
    }
} 