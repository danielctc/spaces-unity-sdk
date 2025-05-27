using UnityEngine;
using System.Linq;

public class HotspotVisualizer : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        // Find all hotspots in the scene
        var hotspots = FindObjectsOfType<Transform>()
            .Where(t => t.name.EndsWith("-hotspot"))
            .ToList();
            
        foreach (var hotspot in hotspots)
        {
            // Draw a red sphere at each hotspot
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(hotspot.position, 0.1f);
            
            // Draw a line showing the forward direction
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(hotspot.position, hotspot.forward * 0.2f);
        }
    }
} 