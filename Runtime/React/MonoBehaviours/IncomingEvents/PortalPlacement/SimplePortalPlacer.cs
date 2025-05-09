using UnityEngine;
using Spaces.React.Runtime;

namespace Spaces.React.Runtime
{
    [AddComponentMenu("Spaces/React/SimplePortalPlacer")]
    public class SimplePortalPlacer : MonoBehaviour
    {
        [SerializeField] private GameObject portalPrefabToPlace; // The portal prefab
        [SerializeField] private Transform placementParent;

        private void OnEnable()
        {
            // Subscribe to the new event for placing portal prefabs
            ReactIncomingEvent.OnPlacePortalPrefab += HandlePlacePortalPrefab;
        }

        private void OnDisable()
        {
            // Unsubscribe from the event
            ReactIncomingEvent.OnPlacePortalPrefab -= HandlePlacePortalPrefab;
        }

        private void HandlePlacePortalPrefab(PortalPrefabPlacementData data)
        {
            if (portalPrefabToPlace == null)
            {
                Debug.LogError("Portal Prefab not assigned to SimplePortalPlacer!");
                return;
            }

            if (string.IsNullOrEmpty(data.portalId))
            {
                Debug.LogError("Portal ID is missing in PortalPrefabPlacementData!");
                return;
            }

            Debug.Log($"[SimplePortalPlacer] Placing portal with ID: {data.portalId}");

            // Instantiate the portal prefab
            GameObject instance = Instantiate(portalPrefabToPlace, data.position, Quaternion.Euler(data.rotation), placementParent);
            instance.transform.localScale = data.scale;

            // Get the PortalManager component from the instantiated prefab
            PortalManager portalManager = instance.GetComponent<PortalManager>();
            if (portalManager != null)
            {
                // Set the portalId using the new method
                portalManager.SetPortalId(data.portalId);
                Debug.Log($"[SimplePortalPlacer] Set portal ID to: {data.portalId}");
                
                // Ensure the portal is properly set up
                if (portalManager.targetRenderer == null)
                {
                    Debug.LogWarning($"[SimplePortalPlacer] Portal {data.portalId} has no target renderer assigned!");
                }
            }
            else
            {
                Debug.LogError($"[SimplePortalPlacer] Instantiated portal prefab '{instance.name}' does not have a PortalManager component!");
            }
            
            // Optionally, use data.prefabName if you have logic to select different prefabs by name

            Debug.Log($"[SimplePortalPlacer] Placed portal '{data.portalId}' at position: {data.position}, rotation: {data.rotation}, scale: {data.scale}");
        }
    }
} 