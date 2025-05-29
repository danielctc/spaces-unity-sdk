using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using UnityEditor;
using System.Collections;
using GLTFast; // Add this for GLB loading
using System.Threading.Tasks;

public class GLBDownloader : MonoBehaviour, IGLBHandler
{
    [SerializeField] private string glbUrl = ""; // Start empty, will be set by React
    [SerializeField] private HotspotDetector hotspotDetector; // Reference to the HotspotDetector
    [SerializeField] private bool testingMode = false; // Toggle for testing in Editor
    
    private GameObject currentModel;
    public bool HasModel => currentModel != null;
    
    // Store position data from React
    private Vector3? targetPosition;
    private Quaternion? targetRotation;
    private Vector3? targetScale;

    // Event for when the model is loaded
    public System.Action OnModelLoaded;
    
    public string Url
    {
        get => glbUrl;
        set
        {
            Debug.Log($"[GLBDownloader] Setting URL to: {value}");
            glbUrl = value;
            if (!string.IsNullOrEmpty(glbUrl))
            {
                StartCoroutine(LoadModel());
            }
        }
    }

    public void SetTargetTransform(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        Debug.Log($"[GLBDownloader] Setting target transform - Position: {position}, Rotation: {rotation.eulerAngles}, Scale: {scale}");
        targetPosition = position;
        targetRotation = rotation;
        targetScale = scale;

        // If we already have a model, update its position immediately
        if (currentModel != null)
        {
            ApplyTransform();
        }
    }

    private void ApplyTransform()
    {
        if (currentModel != null && targetPosition.HasValue)
        {
            Debug.Log($"[GLBDownloader] Applying transform to model - Position: {targetPosition.Value}");
            currentModel.transform.position = targetPosition.Value;
            currentModel.transform.rotation = targetRotation ?? Quaternion.identity;
            currentModel.transform.localScale = targetScale ?? Vector3.one;
        }
    }

    private void Start()
    {
        Debug.Log("[GLBDownloader] Initialized - waiting for URL from React");
    }

    public System.Collections.IEnumerator LoadModel()
    {
        if (string.IsNullOrEmpty(glbUrl))
        {
            Debug.LogError("[GLBDownloader] URL is empty!");
            yield break;
        }

        Debug.Log($"[GLBDownloader] Starting to load model from URL: {glbUrl}");

        // Clean up existing model if any
        if (currentModel != null)
        {
            Debug.Log("[GLBDownloader] Cleaning up existing model");
            Destroy(currentModel);
            currentModel = null;
        }

        yield return StartCoroutine(LoadGLB(glbUrl, transform));
    }

    public System.Collections.IEnumerator LoadGLB(string glbPath, Transform parentTransform = null)
    {
        Debug.Log($"[GLBDownloader] Attempting to download from: {glbPath}");
        
        using (UnityWebRequest www = UnityWebRequest.Get(glbPath))
        {
            www.timeout = 30;
            
            Debug.Log("[GLBDownloader] Sending web request...");
            var operation = www.SendWebRequest();
            
            while (!operation.isDone)
            {
                if (www.downloadProgress > 0)
                {
                    Debug.Log($"[GLBDownloader] Download progress: {www.downloadProgress * 100:F1}%");
                }
                yield return null;
            }

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[GLBDownloader] Download failed: {www.error}");
                yield break;
            }

            byte[] glbData = www.downloadHandler.data;
            Debug.Log($"[GLBDownloader] Download successful! Size: {glbData.Length} bytes");

#if UNITY_EDITOR
            if (testingMode)
            {
                Debug.Log("[GLBDownloader] Running in Editor testing mode");
                // Save to a temporary file in the Assets folder for testing
                string tempPath = "Assets/Temp/temp.glb";
                string directory = Path.GetDirectoryName(tempPath);
                
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                File.WriteAllBytes(tempPath, glbData);
                
                AssetDatabase.Refresh();
                
                // Create a container for the model
                GameObject container = new GameObject("OnlineGLBContainer");
                container.transform.position = transform.position;
                container.transform.rotation = transform.rotation;
                container.transform.localScale = Vector3.one;

                // Load the downloaded GLB
                Object glbObject = AssetDatabase.LoadAssetAtPath<Object>(tempPath);
                if (glbObject != null)
                {
                    GameObject glbHolder = new GameObject(Path.GetFileNameWithoutExtension(tempPath));
                    glbHolder.transform.SetParent(container.transform);
                    glbHolder.transform.localPosition = Vector3.zero;
                    glbHolder.transform.localRotation = Quaternion.identity;
                    glbHolder.transform.localScale = Vector3.one;

                    GameObject glbInstance = (GameObject)PrefabUtility.InstantiatePrefab(glbObject);
                    glbInstance.transform.SetParent(glbHolder.transform);
                    glbInstance.transform.localPosition = Vector3.zero;
                    glbInstance.transform.localRotation = Quaternion.identity;
                    glbInstance.transform.localScale = Vector3.one;

                    currentModel = container;
                    Debug.Log($"[GLBDownloader] Added GLB to hierarchy: {glbHolder.name}");
                    
                    yield return null;
                    
                    OnGLBLoaded(glbInstance);
                }
                else
                {
                    Debug.LogError($"[GLBDownloader] Failed to load GLB file at path: {tempPath}");
                    Destroy(container);
                }
            }
            else
            {
                Debug.LogWarning("[GLBDownloader] Testing mode is disabled. GLB loading is only supported in WebGL builds.");
            }
#else
            Debug.Log("[GLBDownloader] Running in WebGL build");
            
            // Create a container for the model at the target position
            GameObject container = new GameObject("WebGL_GLBContainer");
            
            // If we have target position data, use it immediately
            if (targetPosition.HasValue)
            {
                container.transform.position = targetPosition.Value;
                container.transform.rotation = targetRotation ?? Quaternion.identity;
                container.transform.localScale = targetScale ?? Vector3.one;
            }
            else
            {
                container.transform.position = Vector3.zero;
                container.transform.rotation = Quaternion.identity;
                container.transform.localScale = Vector3.one;
            }

            // Load the GLB using GLTFast
            var gltf = new GltfImport();
            bool loadSuccess = false;
            bool instantiateSuccess = false;

            // Load the GLB data directly from the URL
            var loadTask = gltf.Load(glbPath);
            while (!loadTask.IsCompleted)
            {
                yield return null;
            }
            loadSuccess = loadTask.Result;

            if (!loadSuccess)
            {
                Debug.LogError("[GLBDownloader] Failed to load GLB data in WebGL build");
                Destroy(container);
                yield break;
            }

            // Create a holder for the model
            GameObject modelHolder = new GameObject("GLBModel");
            modelHolder.transform.SetParent(container.transform);
            modelHolder.transform.localPosition = Vector3.zero;
            modelHolder.transform.localRotation = Quaternion.identity;
            modelHolder.transform.localScale = Vector3.one;

            // Instantiate the model
            var instantiateTask = gltf.InstantiateMainSceneAsync(modelHolder.transform);
            while (!instantiateTask.IsCompleted)
            {
                yield return null;
            }
            instantiateSuccess = instantiateTask.Result;

            if (!instantiateSuccess)
            {
                Debug.LogError("[GLBDownloader] Failed to instantiate GLB in WebGL build");
                Destroy(container);
                yield break;
            }

            // Success! Clean up and notify
            currentModel = container;
            Debug.Log($"[GLBDownloader] Successfully loaded GLB in WebGL: {container.name}");
            OnGLBLoaded(modelHolder);
#endif
        }
    }

    public void OnGLBLoaded(GameObject glbInstance)
    {
        Debug.Log($"[GLBDownloader] GLB loaded: {glbInstance.name}");
        if (hotspotDetector != null)
        {
            Debug.Log("[GLBDownloader] Triggering hotspot detection");
            hotspotDetector.DetectHotspots();
        }
        else
        {
            Debug.LogWarning("[GLBDownloader] No HotspotDetector assigned!");
        }

        // Trigger the OnModelLoaded event
        OnModelLoaded?.Invoke();
    }

    public GameObject GetModelRoot()
    {
        if (currentModel == null) return null;
        
        // In WebGL build, the model is a child of the container
        #if UNITY_WEBGL && !UNITY_EDITOR
        if (currentModel.transform.childCount > 0)
        {
            var modelHolder = currentModel.transform.GetChild(0);
            if (modelHolder.childCount > 0)
            {
                return modelHolder.GetChild(0).gameObject;
            }
            return modelHolder.gameObject;
        }
        #else
        // In Editor, the model is a child of the container
        if (currentModel.transform.childCount > 0)
        {
            var glbHolder = currentModel.transform.GetChild(0);
            if (glbHolder.childCount > 0)
            {
                return glbHolder.GetChild(0).gameObject;
            }
            return glbHolder.gameObject;
        }
        #endif
        
        return currentModel;
    }
} 