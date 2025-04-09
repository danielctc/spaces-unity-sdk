using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Reflection;
using System;

#if UNITY_EDITOR
namespace Spaces.Core.Editor
{
    public class SeatingHotspotComponent
    {
        private const string PREFAB_PATH = "Assets/SpacesSDK/Runtime/Assets/Prefabs/Components/Seating.prefab";
        private const string SEATING_TYPE_NAME = "Spaces.React.Runtime.SeatingHotspot";
        
        // Use priority 0 to place at the top of the menu
        [MenuItem(SpacesMenuItems.MENU_ROOT + "/Seating Hotspot", false, 0)]
        static void CreateSeatingHotspot()
        {
            // Find the prefab
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH);
            if (prefab == null)
            {
                Debug.LogError($"Could not find Seating prefab at path: {PREFAB_PATH}");
                return;
            }
            
            // Determine the next Seating number
            int nextSeatingNumber = GetNextSeatingNumber();
            
            // Instantiate the prefab
            GameObject seatingInstance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            
            // Set the seatingId
            Component seatingHotspot = seatingInstance.GetComponent(SEATING_TYPE_NAME);
            if (seatingHotspot != null)
            {
                // Use reflection to set the seatingId field
                FieldInfo seatingIdField = seatingHotspot.GetType().GetField("seatingId");
                if (seatingIdField != null)
                {
                    seatingIdField.SetValue(seatingHotspot, $"Seating_{nextSeatingNumber}");
                    Debug.Log($"Created Seating Hotspot with ID: Seating_{nextSeatingNumber}");
                }
                else
                {
                    Debug.LogError("Could not find seatingId field on SeatingHotspot component");
                }
            }
            else
            {
                Debug.LogError("SeatingHotspot component not found on prefab");
            }
            
            // Select the newly created Seating Hotspot
            Selection.activeGameObject = seatingInstance;
            
            // Register undo
            Undo.RegisterCreatedObjectUndo(seatingInstance, "Create Seating Hotspot");
        }
        
        /// <summary>
        /// Finds the next available Seating number by checking existing Seating Hotspots in the scene
        /// </summary>
        private static int GetNextSeatingNumber()
        {
            // Find the SeatingHotspot type using reflection
            Type seatingType = FindType(SEATING_TYPE_NAME);
            if (seatingType == null)
            {
                Debug.LogError($"Could not find type {SEATING_TYPE_NAME}");
                return 1;
            }
            
            // Find all SeatingHotspot components in the scene
            var existingSeats = UnityEngine.Object.FindObjectsByType(seatingType, FindObjectsSortMode.None);
            
            if (existingSeats.Length == 0)
                return 1;
            
            // Extract numbers from Seating IDs and find the highest
            int highestNumber = 0;
            foreach (var seat in existingSeats)
            {
                // Use reflection to get the seatingId field
                FieldInfo seatingIdField = seatingType.GetField("seatingId");
                if (seatingIdField == null)
                    continue;
                
                string seatingId = (string)seatingIdField.GetValue(seat);
                
                if (string.IsNullOrEmpty(seatingId) || !seatingId.StartsWith("Seating_"))
                    continue;
                    
                string numberPart = seatingId.Substring("Seating_".Length);
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
        [MenuItem(SpacesMenuItems.MENU_ROOT + "/Seating Hotspot", true)]
        static bool ValidateCreateSeatingHotspot()
        {
            // Check if the prefab exists
            return AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH) != null;
        }
    }
}
#endif 