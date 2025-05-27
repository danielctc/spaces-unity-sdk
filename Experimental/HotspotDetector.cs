using UnityEngine;
using System.Linq;

public class HotspotDetector : MonoBehaviour
{
    [SerializeField] private GameObject testPrefab; // Assign this in the inspector
    
    private void Start()
    {
        DetectHotspots();
    }

    public void DetectHotspots()
    {
        // Find all objects in the scene that end with "-hotspot"
        var hotspots = FindObjectsOfType<Transform>()
            .Where(t => t.name.EndsWith("-hotspot"))
            .ToList();

        foreach (var hotspot in hotspots)
        {
            Debug.Log($"Found hotspot: {hotspot.name} at position {hotspot.position}");
            
            // Check if this hotspot already has a test prefab
            if (hotspot.childCount > 0 && hotspot.GetChild(0).name == testPrefab.name + "(Clone)")
            {
                Debug.Log($"Hotspot {hotspot.name} already has a test prefab attached");
                continue;
            }
            
            // Instantiate the test prefab at the hotspot's position
            if (testPrefab != null)
            {
                GameObject instance = Instantiate(testPrefab, hotspot.position, hotspot.rotation, hotspot);
                Debug.Log($"Attached test prefab to {hotspot.name}");
            }
            else
            {
                Debug.LogError("Test prefab not assigned to HotspotDetector!");
            }
        }
    }
} 