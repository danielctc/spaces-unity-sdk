using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Reflection;
using System;

#if UNITY_EDITOR
namespace Spaces.Core.Editor
{
    public class LiveProjectorComponent
    {
        private const string PREFAB_PATH = "Assets/SpacesSDK/Runtime/Assets/Prefabs/Components/LiveProjector.prefab";
        private const string LIVE_PROJECTOR_TYPE_NAME = "Spaces.React.Runtime.LiveProjector";
        
        // Use priority 0 to place at the top of the menu
        [MenuItem(SpacesMenuItems.MENU_ROOT + "/Live Projector", false, 0)]
        static void CreateLiveProjector()
        {
            // Find the prefab
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH);
            if (prefab == null)
            {
                Debug.LogError($"Could not find LiveProjector prefab at path: {PREFAB_PATH}");
                return;
            }
            
            // Determine the next LiveProjector number
            int nextProjectorNumber = GetNextLiveProjectorNumber();
            
            // Instantiate the prefab
            GameObject projectorInstance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            
            // Set the projectorId without changing the GameObject name
            Component liveProjector = projectorInstance.GetComponent(LIVE_PROJECTOR_TYPE_NAME);
            if (liveProjector != null)
            {
                // Use reflection to set the projectorId field (not property)
                FieldInfo projectorIdField = liveProjector.GetType().GetField("projectorId");
                if (projectorIdField != null)
                {
                    projectorIdField.SetValue(liveProjector, $"LiveProjector_{nextProjectorNumber}");
                    Debug.Log($"Created LiveProjector with ID: LiveProjector_{nextProjectorNumber}");
                }
                else
                {
                    Debug.LogError("Could not find projectorId field on LiveProjector component");
                }
            }
            else
            {
                Debug.LogError("LiveProjector component not found on prefab");
            }
            
            // Select the newly created LiveProjector
            Selection.activeGameObject = projectorInstance;
            
            // Register undo
            Undo.RegisterCreatedObjectUndo(projectorInstance, "Create LiveProjector");
        }
        
        /// <summary>
        /// Finds the next available LiveProjector number by checking existing LiveProjectors in the scene
        /// </summary>
        private static int GetNextLiveProjectorNumber()
        {
            // Find the LiveProjector type using reflection
            Type liveProjectorType = FindType(LIVE_PROJECTOR_TYPE_NAME);
            if (liveProjectorType == null)
            {
                Debug.LogError($"Could not find type {LIVE_PROJECTOR_TYPE_NAME}");
                return 1;
            }
            
            // Find all LiveProjector components in the scene using the non-deprecated method
            var existingProjectors = UnityEngine.Object.FindObjectsByType(liveProjectorType, FindObjectsSortMode.None);
            
            if (existingProjectors.Length == 0)
                return 1;
            
            // Extract numbers from LiveProjector IDs and find the highest
            int highestNumber = 0;
            foreach (var projector in existingProjectors)
            {
                // Use reflection to get the projectorId field (not property)
                FieldInfo projectorIdField = liveProjectorType.GetField("projectorId");
                if (projectorIdField == null)
                    continue;
                
                string projectorId = (string)projectorIdField.GetValue(projector);
                
                if (string.IsNullOrEmpty(projectorId) || !projectorId.StartsWith("LiveProjector_"))
                    continue;
                    
                string numberPart = projectorId.Substring("LiveProjector_".Length);
                if (int.TryParse(numberPart, out int number))
                {
                    highestNumber = Mathf.Max(highestNumber, number);
                }
            }
            
            return highestNumber + 1;
        }
        
        /// <summary>
        /// Find a type by name in all loaded assemblies
        /// </summary>
        private static Type FindType(string typeName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(typeName);
                if (type != null)
                    return type;
            }
            return null;
        }
        
        // Validate the menu item
        [MenuItem(SpacesMenuItems.MENU_ROOT + "/Live Projector", true)]
        static bool ValidateCreateLiveProjector()
        {
            // Check if the prefab exists
            return AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH) != null;
        }
    }
}
#endif 