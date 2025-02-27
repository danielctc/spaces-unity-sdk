#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Spaces.Core.Editor
{
    public class InstalledTab : AssetMenuExtensions.ISpacesSDKTab
    {
        public string TabName => "Installed";

        public void DrawTabGUI()
        {
            GUILayout.Label("Installed Packages", EditorStyles.boldLabel);
            GUILayout.Space(20); // Increased space for better section separation

            var packages = new Dictionary<string, string>
            {
                { "Spaces SDK Core", "Packages/com.spacesmetaverse.core" },
                { "Spaces SDK React", "Packages/com.spacesmetaverse.react" },
                { "Spaces SDK Fusion", "Packages/com.spacesmetaverse.fusion" }
            };

            foreach (var package in packages)
            {
                CheckAndDisplayPackageStatus(package.Key, package.Value);
                GUILayout.Space(10); // Space between each package section for better readability
            }
        }

        private void CheckAndDisplayPackageStatus(string packageName, string packagePath)
        {
            if (AssetDatabase.IsValidFolder(packagePath))
            {
                GUILayout.Label($"{packageName}: Installed", EditorStyles.label);
            }
            else
            {
                GUILayout.Label($"{packageName}: Not Installed", EditorStyles.boldLabel);
            }
        }

        public void OnPluginLoaded(AssetMenuExtensions.ConfigurationWindow window)
        {
            // Add any code that should be executed when the plugin is loaded
        }
    }
}
#endif
