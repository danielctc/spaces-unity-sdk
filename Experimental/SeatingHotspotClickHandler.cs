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
        if (hotspotManager == null) return;
        hotspotManager.OnPointerClick(eventData);
    }
} 