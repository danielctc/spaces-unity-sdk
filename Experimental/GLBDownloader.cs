using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using UnityEditor;

public class GLBDownloader : MonoBehaviour
{
    [SerializeField] private string glbUrl = "https://your-url-here/your-model.glb";
    [SerializeField] private Transform parentTransform; // Optional parent for the loaded model
    [SerializeField] private HotspotDetector hotspotDetector; // Reference to the HotspotDetector
    
    private void Start()
    {
        Debug.Log("GLBDownloader: Starting download process...");
        if (string.IsNullOrEmpty(glbUrl))
        {
            Debug.LogError("GLBDownloader: URL is empty! Please set a valid URL in the inspector.");
            return;
        }
        StartCoroutine(DownloadGLB());
    }

    private System.Collections.IEnumerator DownloadGLB()
    {
        Debug.Log($"GLBDownloader: Attempting to download from: {glbUrl}");
        
        using (UnityWebRequest www = UnityWebRequest.Get(glbUrl))
        {
            www.timeout = 30;
            
            Debug.Log("GLBDownloader: Sending web request...");
            var operation = www.SendWebRequest();
            
            while (!operation.isDone)
            {
                if (www.downloadProgress > 0)
                {
                    Debug.Log($"GLBDownloader: Download progress: {www.downloadProgress * 100:F1}%");
                }
                yield return null;
            }

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"GLBDownloader: Download failed: {www.error}");
                yield break;
            }

            byte[] glbData = www.downloadHandler.data;
            Debug.Log($"GLBDownloader: Download successful! Size: {glbData.Length} bytes");

            // Save to a temporary file in the Assets folder
            string tempPath = "Assets/Temp/temp.glb";
            string directory = Path.GetDirectoryName(tempPath);
            
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            File.WriteAllBytes(tempPath, glbData);
            
            // Force Unity to import the file
            #if UNITY_EDITOR
            AssetDatabase.Refresh();
            
            // Create a container for the model
            GameObject container = new GameObject("GLBContainer");
            if (parentTransform != null)
            {
                container.transform.SetParent(parentTransform);
            }
            container.transform.localPosition = Vector3.zero;
            container.transform.localRotation = Quaternion.identity;
            container.transform.localScale = Vector3.one;

            // Add the GLB file to the hierarchy
            Object glbObject = AssetDatabase.LoadAssetAtPath<Object>(tempPath);
            if (glbObject != null)
            {
                // Create a new GameObject to hold the GLB
                GameObject glbHolder = new GameObject(Path.GetFileNameWithoutExtension(tempPath));
                glbHolder.transform.SetParent(container.transform);
                glbHolder.transform.localPosition = Vector3.zero;
                glbHolder.transform.localRotation = Quaternion.identity;
                glbHolder.transform.localScale = Vector3.one;

                // Add the GLB as a child
                GameObject glbInstance = (GameObject)PrefabUtility.InstantiatePrefab(glbObject);
                glbInstance.transform.SetParent(glbHolder.transform);
                glbInstance.transform.localPosition = Vector3.zero;
                glbInstance.transform.localRotation = Quaternion.identity;
                glbInstance.transform.localScale = Vector3.one;

                Debug.Log($"GLBDownloader: Added GLB to hierarchy: {glbHolder.name}");

                // Wait a frame to ensure the GLB is fully loaded
                yield return null;

                // Detect hotspots in the newly added GLB
                if (hotspotDetector != null)
                {
                    hotspotDetector.DetectHotspots();
                }
                else
                {
                    Debug.LogWarning("GLBDownloader: No HotspotDetector assigned!");
                }
            }
            else
            {
                Debug.LogError($"GLBDownloader: Failed to load GLB file at path: {tempPath}");
                Destroy(container);
            }
            #else
            Debug.LogError("GLBDownloader: Runtime GLB loading is only supported in the Unity Editor");
            #endif
        }
    }
} 