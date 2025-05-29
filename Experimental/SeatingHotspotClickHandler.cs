using UnityEngine;
using UnityEngine.EventSystems;

public class SeatingHotspotClickHandler : MonoBehaviour, IPointerClickHandler
{
    private SeatingHotspotManager hotspotManager;

    public void Initialize(SeatingHotspotManager manager)
    {
        hotspotManager = manager;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("[SeatingHotspotClickHandler] GLB model clicked");
        if (hotspotManager != null)
        {
            // Forward the click to the hotspot manager
            hotspotManager.OnPointerClick(eventData);
        }
    }
} 