#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Spaces.Core.Editor
{
    public class SetupComplete : EditorWindow
    {
        private Texture2D headerSectionTexture;
        
        public static void ShowWindow()
        {
            SetupComplete window = GetWindow<SetupComplete>(true, "Setup Complete", true);
            window.minSize = new Vector2(400, 300);
            window.maxSize = new Vector2(400, 300);
        }

        private void OnEnable()
        {
            headerSectionTexture = Resources.Load("Identity") as Texture2D;
        }

        void OnGUI()
        {
            // Draw the header image
            if (headerSectionTexture != null)
            {
                GUILayout.Label(new GUIContent(headerSectionTexture));
            }
            
            GUILayout.Space(20);

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };

            GUIStyle textStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };

            GUILayout.Label("Setup Complete!", titleStyle);
            GUILayout.Space(20);
            
            GUILayout.Label("All required packages have been installed successfully.", textStyle);
            GUILayout.Label("You're now ready to start using Spaces SDK.", textStyle);
            
            GUILayout.Space(30);
            
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Close", GUILayout.Width(100), GUILayout.Height(30)))
            {
                Close();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }
}
#endif 