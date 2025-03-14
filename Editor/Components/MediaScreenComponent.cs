using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Reflection;
using System;

#if UNITY_EDITOR
namespace Spaces.Core.Editor
{
    public class MediaScreenComponent
    {
        private const string PREFAB_PATH = "Assets/SpacesSDK/Runtime/Assets/Prefabs/Components/MediaScreen.prefab";
        private const string MEDIA_SCREEN_TYPE_NAME = "Spaces.React.Runtime.MediaScreen";
        
        // Use priority 0 to place at the top of the menu
        [MenuItem(SpacesMenuItems.MENU_ROOT + "/Media Screen", false, 0)]
        static void CreateMediaScreen()
        {
            // Find the prefab
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH);
            if (prefab == null)
            {
                Debug.LogError($"Could not find MediaScreen prefab at path: {PREFAB_PATH}");
                return;
            }
            
            // Determine the next MediaScreen number
            int nextScreenNumber = GetNextMediaScreenNumber();
            
            // Instantiate the prefab
            GameObject screenInstance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            
            // Set the screenId without changing the GameObject name
            Component mediaScreen = screenInstance.GetComponent(MEDIA_SCREEN_TYPE_NAME);
            if (mediaScreen != null)
            {
                // Use reflection to set the screenId field (not property)
                FieldInfo screenIdField = mediaScreen.GetType().GetField("screenId");
                if (screenIdField != null)
                {
                    screenIdField.SetValue(mediaScreen, $"MediaScreen_{nextScreenNumber}");
                    Debug.Log($"Created MediaScreen with ID: MediaScreen_{nextScreenNumber}");
                }
                else
                {
                    Debug.LogError("Could not find screenId field on MediaScreen component");
                }
            }
            else
            {
                Debug.LogError("MediaScreen component not found on prefab");
            }
            
            // Select the newly created MediaScreen
            Selection.activeGameObject = screenInstance;
            
            // Register undo
            Undo.RegisterCreatedObjectUndo(screenInstance, "Create MediaScreen");
        }
        
        /// <summary>
        /// Finds the next available MediaScreen number by checking existing MediaScreens in the scene
        /// </summary>
        private static int GetNextMediaScreenNumber()
        {
            // Find the MediaScreen type using reflection
            Type mediaScreenType = FindType(MEDIA_SCREEN_TYPE_NAME);
            if (mediaScreenType == null)
            {
                Debug.LogError($"Could not find type {MEDIA_SCREEN_TYPE_NAME}");
                return 1;
            }
            
            // Find all MediaScreen components in the scene using the non-deprecated method
            var existingScreens = UnityEngine.Object.FindObjectsByType(mediaScreenType, FindObjectsSortMode.None);
            
            if (existingScreens.Length == 0)
                return 1;
            
            // Extract numbers from MediaScreen IDs and find the highest
            int highestNumber = 0;
            foreach (var screen in existingScreens)
            {
                // Use reflection to get the screenId field (not property)
                FieldInfo screenIdField = mediaScreenType.GetField("screenId");
                if (screenIdField == null)
                    continue;
                
                string screenId = (string)screenIdField.GetValue(screen);
                
                if (string.IsNullOrEmpty(screenId) || !screenId.StartsWith("MediaScreen_"))
                    continue;
                    
                string numberPart = screenId.Substring("MediaScreen_".Length);
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
        [MenuItem(SpacesMenuItems.MENU_ROOT + "/Media Screen", true)]
        static bool ValidateCreateMediaScreen()
        {
            // Check if the prefab exists
            return AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH) != null;
        }
    }
}
#endif 