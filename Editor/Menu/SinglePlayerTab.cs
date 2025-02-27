#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System.IO;

namespace Spaces.Core.Editor
{
    public class SinglePlayerTab : AssetMenuExtensions.ISpacesSDKTab
    {
        public string TabName => "Single Player";

        private string newProjectName = "";
        private string sceneName = "";

        public void DrawTabGUI()
        {
            EditorGUILayout.BeginVertical();
            GUILayout.Label("Single Player settings", EditorStyles.boldLabel);

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

        private void CreateSpace()
        {
            if (string.IsNullOrEmpty(newProjectName) || string.IsNullOrEmpty(sceneName))
            {
                EditorUtility.DisplayDialog("Error", "Please enter both project name and scene name.", "OK");
                return;
            }

            string projectsPath = Path.Combine("Assets", "Projects");
            if (!AssetDatabase.IsValidFolder(projectsPath))
            {
                AssetDatabase.CreateFolder("Assets", "Projects");
            }

            string projectPath = Path.Combine(projectsPath, newProjectName);
            if (!AssetDatabase.IsValidFolder(projectPath))
            {
                AssetDatabase.CreateFolder(projectsPath, newProjectName);
            }

            string newScenePath = Path.Combine(projectPath, sceneName + ".unity");

            if (File.Exists(newScenePath))
            {
                EditorUtility.DisplayDialog("Error", "A scene with that name already exists in the project.", "OK");
                return;
            }

            // Path to the default scene in the package
            string defaultScenePath = "Packages/com.spacesmetaverse.core/Editor/Assets/DefaultScene/DefaultScene.unity";

            if (!File.Exists(defaultScenePath))
            {
                EditorUtility.DisplayDialog("Error", "Default scene not found in the package.", "OK");
                return;
            }

            // Copy the default scene to the new location
            AssetDatabase.CopyAsset(defaultScenePath, newScenePath);

            AssetDatabase.Refresh();

            // Open the new scene
            EditorSceneManager.OpenScene(newScenePath);

            EditorUtility.DisplayDialog("Success", "New project and scene created and opened successfully.", "OK");
        }

  
    }
}
#endif
