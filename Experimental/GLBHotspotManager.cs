using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Networking;

public class GLBHotspotManager : MonoBehaviour
{
    [SerializeField] private GameObject testPrefab;
    [SerializeField] private string glbUrl; // URL to your GLB file
    
    private void Start()
    {
        LoadGLBAndSetupHotspots();
    }
    
    private void LoadGLBAndSetupHotspots()
    {
        // Create a container for the model
        GameObject container = new GameObject("GLBContainer");
        
        try
        {
            // Start the download
            StartCoroutine(DownloadAndProcessGLB(container));
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading GLB: {e.Message}");
        }
    }

    private System.Collections.IEnumerator DownloadAndProcessGLB(GameObject container)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(glbUrl))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to download GLB: {www.error}");
                yield break;
            }

            // For now, just log that we downloaded the file
            Debug.Log($"Downloaded GLB file: {www.downloadedBytes} bytes");

            // Find all hotspots in the scene
            var hotspots = FindObjectsOfType<Transform>()
                .Where(t => t.name.EndsWith("-hotspot"))
                .ToList();
                
            Debug.Log($"Found {hotspots.Count} hotspots in the scene");
            
            foreach (var hotspot in hotspots)
            {
                Debug.Log($"Processing hotspot: {hotspot.name}");
                
                if (testPrefab != null)
                {
                    GameObject instance = Instantiate(testPrefab, hotspot.position, hotspot.rotation, hotspot);
                    Debug.Log($"Attached test prefab to {hotspot.name}");
                }
                else
                {
                    Debug.LogError("Test prefab not assigned to GLBHotspotManager!");
                }
            }
        }
    }
} 