using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spaces.React.Runtime;

public class CatalogObjectReceiver : MonoBehaviour
{
    [SerializeField] private GameObject testPrefab; // For testing with a simple prefab
    [SerializeField] private Transform objectsParent; // Parent transform to organize instantiated objects
    
    private Dictionary<string, GameObject> placedObjects = new Dictionary<string, GameObject>();
    
    private void Awake()
    {
        // Create parent object if not assigned
        if (objectsParent == null)
        {
            GameObject parent = new GameObject("CatalogObjects");
            objectsParent = parent.transform;
        }
    }
    
    private void OnEnable()
    {
        // Subscribe to the catalog object event
        ReactIncomingEvent.OnPlaceCatalogObject += HandlePlaceCatalogObject;
    }
    
    private void OnDisable()
    {
        // Unsubscribe from the catalog object event
        ReactIncomingEvent.OnPlaceCatalogObject -= HandlePlaceCatalogObject;
    }
    
    // Handle the catalog object event
    private void HandlePlaceCatalogObject(CatalogItemData catalogItemData)
    {
        Debug.Log($"Unity: Received catalog object: {catalogItemData.name}");
        
        // Create a new game object for this catalog item
        GameObject newObject;
        
        if (testPrefab != null)
        {
            // Use test prefab for initial testing
            newObject = Instantiate(testPrefab, objectsParent);
        }
        else
        {
            // Create empty object (in a real implementation, you'd load the model)
            newObject = new GameObject(catalogItemData.name);
            newObject.transform.SetParent(objectsParent);
        }
        
        // Apply transform data from catalog item
        if (catalogItemData.position != null)
            newObject.transform.position = catalogItemData.position.ToVector3();
        
        if (catalogItemData.rotation != null)
            newObject.transform.eulerAngles = catalogItemData.rotation.ToVector3();
        
        if (catalogItemData.scale != null)
            newObject.transform.localScale = catalogItemData.scale.ToVector3();
        
        // Store reference to object
        if (!string.IsNullOrEmpty(catalogItemData.id))
        {
            // Remove existing object with same ID if it exists
            if (placedObjects.TryGetValue(catalogItemData.id, out GameObject existingObject))
            {
                Destroy(existingObject);
            }
            
            placedObjects[catalogItemData.id] = newObject;
        }
        
        // Add a component to store catalog data
        CatalogObjectComponent component = newObject.AddComponent<CatalogObjectComponent>();
        component.Initialize(catalogItemData);
    }
}

// Component to attach to catalog objects for reference
public class CatalogObjectComponent : MonoBehaviour
{
    public string objectId;
    public string objectName;
    public string modelUrl;
    
    public void Initialize(CatalogItemData catalogItemData)
    {
        objectId = catalogItemData.id;
        objectName = catalogItemData.name;
        modelUrl = catalogItemData.modelUrl;
    }
} 