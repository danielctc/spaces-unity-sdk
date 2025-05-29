using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;

public class LocalGLBHandler : MonoBehaviour, IGLBHandler
{
    [SerializeField] private HotspotDetector hotspotDetector;
    
    public System.Collections.IEnumerator LoadGLB(string glbPath, Transform parentTransform = null)
    {
        #if UNITY_EDITOR
        // Create a container for the model
        GameObject container = new GameObject("LocalGLBContainer");
        if (parentTransform != null)
        {
            container.transform.SetParent(parentTransform);
        }
        container.transform.localPosition = Vector3.zero;
        container.transform.localRotation = Quaternion.identity;
        container.transform.localScale = Vector3.one;

        // Load the GLB file
        Object glbObject = AssetDatabase.LoadAssetAtPath<Object>(glbPath);
        if (glbObject != null)
        {
            // Create a new GameObject to hold the GLB
            GameObject glbHolder = new GameObject(Path.GetFileNameWithoutExtension(glbPath));
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

            Debug.Log($"LocalGLBHandler: Added GLB to hierarchy: {glbHolder.name}");
            
            // Wait a frame to ensure the GLB is fully loaded
            yield return null;
            
            OnGLBLoaded(glbInstance);
        }
        else
        {
            Debug.LogError($"LocalGLBHandler: Failed to load GLB file at path: {glbPath}");
            Destroy(container);
        }
        #else
        Debug.LogError("LocalGLBHandler: Local GLB loading is only supported in the Unity Editor");
        yield break;
        #endif
    }

    public void OnGLBLoaded(GameObject glbInstance)
    {
        if (hotspotDetector != null)
        {
            hotspotDetector.DetectHotspots();
        }
        else
        {
            Debug.LogWarning("LocalGLBHandler: No HotspotDetector assigned!");
        }
    }
} 