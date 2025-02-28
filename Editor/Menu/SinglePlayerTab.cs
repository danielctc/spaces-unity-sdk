#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System.IO;
using Spaces.Core.Editor;
using UnityEngine.UI;
using TMPro;

namespace Spaces.Core.Editor
{
    public class SinglePlayerTab : AssetMenuExtensions.ISpacesSDKTab
    {
        public string TabName => "Single Player";

        // Define paths for assets
        private const string ASSETS_PATH = "Assets/SpacesSDK/Editor/Assets/SingleDefaultScene";
        private const string PACKAGE_PATH = "Packages/com.spacesmetaverse.sdk/Editor/Assets/SingleDefaultScene";

        private string newProjectName = "";
        private string sceneName = "";

        public void DrawTabGUI()
        {
            EditorGUILayout.BeginVertical();
            GUILayout.Label("New Single Player Project", EditorStyles.boldLabel);

            GUILayout.Label("New Project", EditorStyles.label);
            newProjectName = EditorGUILayout.TextField(newProjectName);

            GUILayout.Label("Scene Name", EditorStyles.label);
            sceneName = EditorGUILayout.TextField(sceneName);

            if (GUILayout.Button("Create Space"))
            {
                CreateSpace();
            }

            GUILayout.Space(10); // Add some space between buttons

            EditorGUILayout.EndVertical();
        }

        public void OnPluginLoaded(AssetMenuExtensions.ConfigurationWindow window)
        {
            // Add any code that should be executed when the plugin is loaded
        }

        /// <summary>
        /// Finds the correct path for a file by checking first in Assets and then in Packages.
        /// </summary>
        /// <param name="fileName">The file name to find</param>
        /// <returns>The full path if found, or null if not found</returns>
        private string FindAssetPath(string fileName)
        {
            // First check in Assets
            string assetsPath = Path.Combine(ASSETS_PATH, fileName);
            if (File.Exists(assetsPath))
            {
                return assetsPath;
            }

            // Then check in Packages
            string packagePath = Path.Combine(PACKAGE_PATH, fileName);
            if (File.Exists(packagePath))
            {
                return packagePath;
            }

            // Not found in either location
            return null;
        }

        private void CreateSpace()
        {
            if (string.IsNullOrEmpty(newProjectName) || string.IsNullOrEmpty(sceneName))
            {
                EditorUtility.DisplayDialog("Error", "Please enter both project name and scene name.", "OK");
                return;
            }

            // Ensure Projects folder exists.
            string projectsPath = Path.Combine("Assets", "Projects");
            if (!AssetDatabase.IsValidFolder(projectsPath))
            {
                AssetDatabase.CreateFolder("Assets", "Projects");
            }

            // Ensure the new project folder exists.
            string projectPath = Path.Combine(projectsPath, newProjectName);
            if (!AssetDatabase.IsValidFolder(projectPath))
            {
                AssetDatabase.CreateFolder(projectsPath, newProjectName);
            }

            // Define the new scene path.
            string newScenePath = Path.Combine(projectPath, sceneName + ".unity");

            if (File.Exists(newScenePath))
            {
                EditorUtility.DisplayDialog("Error", "A scene with that name already exists in the project.", "OK");
                return;
            }

            // Find the default scene using the helper method
            string defaultScenePath = FindAssetPath("DefaultScene.unity");
            
            if (string.IsNullOrEmpty(defaultScenePath))
            {
                EditorUtility.DisplayDialog("Error", $"Default scene not found in either {ASSETS_PATH} or {PACKAGE_PATH}", "OK");
                return;
            }

            // Copy the default scene to the new location.
            AssetDatabase.CopyAsset(defaultScenePath, newScenePath);
            AssetDatabase.Refresh();

            // Open the new scene.
            var scene = EditorSceneManager.OpenScene(newScenePath);

            // Find or create the Fusion_Triggers parent
            GameObject fusionTriggers = GameObject.Find("Fusion_Triggers");
            if (fusionTriggers == null)
            {
                fusionTriggers = new GameObject("Fusion_Triggers");
            }

            // Check if FusionConfig canvas already exists
            GameObject canvasGO = GameObject.Find("FusionConfig");
            if (canvasGO == null)
            {
                // Create Canvas for UI elements
                canvasGO = new GameObject("FusionConfig");
                canvasGO.transform.SetParent(fusionTriggers.transform);
                canvasGO.AddComponent<RectTransform>();
                var canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGO.AddComponent<CanvasScaler>();
                canvasGO.AddComponent<GraphicRaycaster>();
            }

            // Check if input field already exists
            GameObject inputFieldGO = GameObject.Find("InputField_SessionName");
            if (inputFieldGO == null)
            {
                // Create input field as a child of the canvas
                inputFieldGO = new GameObject("InputField_SessionName", typeof(RectTransform));
                inputFieldGO.transform.SetParent(canvasGO.transform);
                var inputField = inputFieldGO.AddComponent<TMP_InputField>();
                
                // Add required components for the input field
                var rectTransform = inputFieldGO.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(160, 30);
                
                // Set the initial text value
                inputField.text = sceneName;
            }
            else
            {
                // Update existing input field's text
                var inputField = inputFieldGO.GetComponent<TMP_InputField>();
                if (inputField != null)
                {
                    inputField.text = sceneName;
                }
            }

            EditorSceneManager.SaveScene(scene);
            EditorUtility.DisplayDialog("Success", "New project and scene created and opened successfully.", "OK");
        }
    }
}
#endif
