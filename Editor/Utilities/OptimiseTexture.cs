using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;

namespace Spaces.Core.Editor
{
    /// <summary>
    /// Editor window for optimizing textures in Unity projects.
    /// Provides functionality to resize, convert formats, and optimize textures for better performance.
    /// </summary>
    public class OptimiseTexture : EditorWindow
    {
        /// <summary>
        /// Supported texture formats for optimization.
        /// Using flags to allow multiple format selection.
        /// </summary>
        [System.Flags]
        public enum TextureFormat
        {
            None = 0,
            PNG = 1 << 0,    // Lossless format, supports transparency
            JPG = 1 << 1,    // Lossy format, no transparency
            TGA = 1 << 2,    // High quality format, supports transparency
            EXR = 1 << 3,    // High dynamic range format
            HDR = 1 << 4,    // High dynamic range format
            All = PNG | JPG | TGA | EXR | HDR
        }

        // UI State Variables
        private Vector2 scrollPosition;                    // Scroll position for the window
        private string texturesFolder = "Assets";         // Default folder to search for textures
        private bool showSelectedTextures = true;         // Toggle for showing selected textures list
        private List<Texture2D> selectedTextures = new List<Texture2D>();  // List of textures to process

        // Optimization Settings
        private bool resizeTextures = false;              // Whether to resize textures
        private bool saveOriginals = false;               // Whether to keep original textures
        private bool resizeToMultipleOfFour = true;       // Whether to resize to multiple of 4 (better compression)
        private TextureFormat targetFormat = TextureFormat.All;  // Target format(s) for conversion
        private int maxTextureSize = 2048;                // Maximum texture size when resizing

        /// <summary>
        /// Adds menu item to open the Texture Optimizer window
        /// </summary>
        [MenuItem("Spaces SDK/Utilities/Optimise Textures")]
        public static void ShowWindow()
        {
            GetWindow<OptimiseTexture>("Optimise Textures");
        }

        /// <summary>
        /// Main GUI drawing function for the editor window
        /// </summary>
        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawHeader();
            DrawTextureSelection();
            DrawOptimizationSettings();
            DrawSelectedTexturesList();

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// Draws the header section with title and description
        /// </summary>
        private void DrawHeader()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Optimise Textures", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Optimise your textures for better performance and reduced file size.", EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space(10);
        }

        /// <summary>
        /// Draws the texture selection section with folder selection and texture loading options
        /// </summary>
        private void DrawTextureSelection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Texture Selection", EditorStyles.boldLabel);

            // Folder selection field with browse button
            EditorGUILayout.BeginHorizontal();
            texturesFolder = EditorGUILayout.TextField("Textures Folder", texturesFolder);
            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                string path = EditorUtility.OpenFolderPanel("Select Folder", "Assets", "");
                if (!string.IsNullOrEmpty(path))
                {
                    texturesFolder = path.Replace(Application.dataPath, "Assets");
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // Texture selection buttons
            if (GUILayout.Button("Select Textures from Folder"))
            {
                selectedTextures = GetTexturesFromFolder(texturesFolder);
            }

            if (GUILayout.Button("Select Textures from Selection"))
            {
                selectedTextures = GetTexturesFromSelection();
            }

            if (GUILayout.Button("Clear Selection"))
            {
                selectedTextures.Clear();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(10);
        }

        /// <summary>
        /// Draws the optimization settings section with various options for texture processing
        /// </summary>
        private void DrawOptimizationSettings()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Optimisation Settings", EditorStyles.boldLabel);

            // Resize settings
            resizeTextures = EditorGUILayout.Toggle("Resize Textures", resizeTextures);
            if (resizeTextures)
            {
                maxTextureSize = EditorGUILayout.IntField("Max Texture Size", maxTextureSize);
            }

            // Additional optimization options
            saveOriginals = EditorGUILayout.Toggle("Save Original Textures", saveOriginals);
            resizeToMultipleOfFour = EditorGUILayout.Toggle("Resize to Multiple of 4", resizeToMultipleOfFour);
            targetFormat = (TextureFormat)EditorGUILayout.EnumFlagsField("Target Format", targetFormat);

            EditorGUILayout.Space(10);

            // Optimization button (enabled only when textures are selected)
            GUI.enabled = selectedTextures.Count > 0;
            if (GUILayout.Button("Optimise Selected Textures"))
            {
                if (EditorUtility.DisplayDialog("Confirm Optimisation",
                    "This will optimise the selected textures. Make sure you have a backup of your project.",
                    "Proceed", "Cancel"))
                {
                    OptimiseTextures();
                }
            }
            GUI.enabled = true;

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(10);
        }

        /// <summary>
        /// Draws the list of selected textures with their properties
        /// </summary>
        private void DrawSelectedTexturesList()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"Selected Textures ({selectedTextures.Count})", EditorStyles.boldLabel);

            if (selectedTextures.Count > 0)
            {
                showSelectedTextures = EditorGUILayout.Foldout(showSelectedTextures, "Show Textures");
                if (showSelectedTextures)
                {
                    foreach (Texture2D texture in selectedTextures)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.ObjectField(texture, typeof(Texture2D), false);
                        EditorGUILayout.LabelField($"{texture.width}x{texture.height}");
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
            else
            {
                EditorGUILayout.LabelField("No textures selected");
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Gets all textures from a specified folder
        /// </summary>
        /// <param name="folder">The folder path to search for textures</param>
        /// <returns>List of found textures</returns>
        private List<Texture2D> GetTexturesFromFolder(string folder)
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });
            List<Texture2D> textures = new List<Texture2D>();
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (texture != null)
                {
                    textures.Add(texture);
                }
            }
            
            return textures;
        }

        /// <summary>
        /// Gets textures from the current Unity selection
        /// </summary>
        /// <returns>List of selected textures</returns>
        private List<Texture2D> GetTexturesFromSelection()
        {
            List<Texture2D> textures = new List<Texture2D>();
            foreach (UnityEngine.Object obj in Selection.objects)
            {
                if (obj is Texture2D texture)
                {
                    textures.Add(texture);
                }
            }
            return textures;
        }

        /// <summary>
        /// Main function to process and optimize selected textures
        /// </summary>
        private void OptimiseTextures()
        {
            // TODO: Implement texture optimization logic
            Debug.Log("Texture optimisation not yet implemented");
        }
    }
}
