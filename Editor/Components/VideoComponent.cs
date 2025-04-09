/*
using UnityEngine;
using UnityEditor;
using System.Linq;

#if UNITY_EDITOR
namespace Spaces.Core.Editor

{
    public class VideoComponent
    {
        private const string PREFAB_PATH = "Assets/SpacesSDK/Runtime/Assets/Prefabs/Components/DefaultTV.prefab";
        
        // Use priority 0 to place at the top of the menu
        [MenuItem(SpacesMenuItems.MENU_ROOT + "/Video", false, 0)]
        static void CreateTV()
        {
            // Find the prefab
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH);
            if (prefab == null)
            {
                Debug.LogError($"Could not find TV prefab at path: {PREFAB_PATH}");
                return;
            }
            
            // Determine the next TV number
            int nextTVNumber = GetNextTVNumber();
            
            // Instantiate the prefab
            GameObject tvInstance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            tvInstance.name = $"TV{nextTVNumber}";
            
            // Select the newly created TV
            Selection.activeGameObject = tvInstance;
            
            // Register undo
            Undo.RegisterCreatedObjectUndo(tvInstance, "Create TV");
            
            Debug.Log($"Created {tvInstance.name} in the scene");
        }
        
        /// <summary>
        /// Finds the next available TV number by checking existing TVs in the scene
        /// </summary>
        private static int GetNextTVNumber()
        {
            // Find all GameObjects that start with "TV" followed by a number
            var existingTVs = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                .Where(go => go.name.StartsWith("TV") && char.IsDigit(go.name.FirstOrDefault(c => char.IsDigit(c))))
                .ToList();
            
            if (existingTVs.Count == 0)
                return 1;
            
            // Extract numbers from TV names and find the highest
            int highestNumber = 0;
            foreach (var tv in existingTVs)
            {
                string numberPart = new string(tv.name.Where(char.IsDigit).ToArray());
                if (int.TryParse(numberPart, out int number))
                {
                    highestNumber = Mathf.Max(highestNumber, number);
                }
            }
            
            return highestNumber + 1;
        }
        
        // Validate the menu item
        [MenuItem(SpacesMenuItems.MENU_ROOT + "/Video", true)]
        static bool ValidateCreateTV()
        {
            // Check if the prefab exists
            return AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH) != null;
        }
    }
}
#endif 
*/ 