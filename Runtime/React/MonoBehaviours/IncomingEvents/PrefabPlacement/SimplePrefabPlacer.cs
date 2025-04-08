using UnityEngine;
using System.Collections.Generic;
using Spaces.React.Runtime;

namespace Spaces.React.Runtime
{
    [AddComponentMenu("Spaces/React/SimplePrefabPlacer")]
    public class SimplePrefabPlacer : MonoBehaviour
    {
        [SerializeField] private GameObject prefabToPlace;
        [SerializeField] private Transform placementParent;

        private void OnEnable()
        {
            if (ReactIncomingEvent.Instance != null)
            {
                ReactIncomingEvent.OnPlacePrefab += HandlePlacePrefab;
            }
        }

        private void OnDisable()
        {
            if (ReactIncomingEvent.Instance != null)
            {
                ReactIncomingEvent.OnPlacePrefab -= HandlePlacePrefab;
            }
        }

        private void HandlePlacePrefab(PrefabPlacementData data)
        {
            if (prefabToPlace == null)
            {
                Debug.LogError("Prefab not assigned to SimplePrefabPlacer!");
                return;
            }

            GameObject instance = Instantiate(prefabToPlace, data.position, Quaternion.Euler(data.rotation), placementParent);
            instance.transform.localScale = data.scale;
            Debug.Log($"Placed prefab at position: {data.position}, rotation: {data.rotation}, scale: {data.scale}");
        }
    }
} 