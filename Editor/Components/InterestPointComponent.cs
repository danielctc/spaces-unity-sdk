using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Reflection;
using System;

#if UNITY_EDITOR
namespace Spaces.Core.Editor
{
    public class InterestPointComponent
    {
        private const string PREFAB_PATH = "Assets/SpacesSDK/Runtime/Assets/Prefabs/Components/InterestPoint.prefab";
        private const string INTEREST_POINT_TYPE_NAME = "Spaces.React.Runtime.InterestPoint";
        
        // Use priority 0 to place at the top of the menu
        [MenuItem(SpacesMenuItems.MENU_ROOT + "/Interest Point", false, 0)]
        static void CreateInterestPoint()
        {
            // Find the prefab
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH);
            if (prefab == null)
            {
                Debug.LogError($"Could not find InterestPoint prefab at path: {PREFAB_PATH}");
                return;
            }
            
            // Determine the next InterestPoint number
            int nextPointNumber = GetNextInterestPointNumber();
            
            // Instantiate the prefab
            GameObject pointInstance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            
            // Set the pointId without changing the GameObject name
            Component interestPoint = pointInstance.GetComponent(INTEREST_POINT_TYPE_NAME);
            if (interestPoint != null)
            {
                // Use reflection to set the pointId field (not property)
                FieldInfo pointIdField = interestPoint.GetType().GetField("pointId");
                if (pointIdField != null)
                {
                    pointIdField.SetValue(interestPoint, $"InterestPoint_{nextPointNumber}");
                    Debug.Log($"Created InterestPoint with ID: InterestPoint_{nextPointNumber}");
                }
                else
                {
                    Debug.LogError("Could not find pointId field on InterestPoint component");
                }
            }
            else
            {
                Debug.LogError("InterestPoint component not found on prefab");
            }
            
            // Select the newly created InterestPoint
            Selection.activeGameObject = pointInstance;
            
            // Register undo
            Undo.RegisterCreatedObjectUndo(pointInstance, "Create InterestPoint");
        }
        
        /// <summary>
        /// Finds the next available InterestPoint number by checking existing InterestPoints in the scene
        /// </summary>
        private static int GetNextInterestPointNumber()
        {
            // Find the InterestPoint type using reflection
            Type interestPointType = FindType(INTEREST_POINT_TYPE_NAME);
            if (interestPointType == null)
            {
                Debug.LogError($"Could not find type {INTEREST_POINT_TYPE_NAME}");
                return 1;
            }
            
            // Find all InterestPoint components in the scene using the non-deprecated method
            var existingPoints = UnityEngine.Object.FindObjectsByType(interestPointType, FindObjectsSortMode.None);
            
            if (existingPoints.Length == 0)
                return 1;
            
            // Extract numbers from InterestPoint IDs and find the highest
            int highestNumber = 0;
            foreach (var point in existingPoints)
            {
                // Use reflection to get the pointId field (not property)
                FieldInfo pointIdField = interestPointType.GetField("pointId");
                if (pointIdField == null)
                    continue;
                
                string pointId = (string)pointIdField.GetValue(point);
                
                if (string.IsNullOrEmpty(pointId) || !pointId.StartsWith("InterestPoint_"))
                    continue;
                    
                string numberPart = pointId.Substring("InterestPoint_".Length);
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
        [MenuItem(SpacesMenuItems.MENU_ROOT + "/Interest Point", true)]
        static bool ValidateCreateInterestPoint()
        {
            // Check if the prefab exists
            return AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH) != null;
        }
    }
}
#endif 