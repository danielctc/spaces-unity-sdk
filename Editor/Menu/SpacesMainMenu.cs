#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Spaces.Core.Editor
{
    public class AssetMenuExtensions : MonoBehaviour
    {
        [MenuItem("Spaces SDK/Settings")]
        static void Configure()
        {
            ConfigurationWindow.Init();
        }

        public interface ISpacesSDKTab
        {
            string TabName { get; }
            void DrawTabGUI();
            void OnPluginLoaded(ConfigurationWindow window);
        }

        public class ConfigurationWindow : EditorWindow
        {
            private Vector2 scrollPosition = Vector2.zero;
            private List<ISpacesSDKTab> tabs = new List<ISpacesSDKTab>();
            private int selectedTabIndex = 0;
            private static ConfigurationWindow instance;

            private const int MIN_WIDTH = 800;
            private const int MIN_HEIGHT = 600;
            private const float FIXED_PANEL_WIDTH = 250f;

            private GUIStyle lineStyle;

            public delegate void OnPluginLoadedDelegate(ISpacesSDKTab tab);
            public static event OnPluginLoadedDelegate OnPluginLoaded;

            public static void Init()
            {
                instance = (ConfigurationWindow)GetWindow(typeof(ConfigurationWindow), true, "Configure Spaces SDK");
                instance.minSize = new Vector2(MIN_WIDTH, MIN_HEIGHT);
                instance.Show();

                if (OnPluginLoaded != null)
                {
                    foreach (var tab in instance.tabs)
                    {
                        OnPluginLoaded(tab);
                    }
                }
            }

            private void OnEnable()
            {
                lineStyle = new GUIStyle();
                lineStyle.normal.background = EditorGUIUtility.whiteTexture;
                lineStyle.margin = new RectOffset(0, 0, 4, 4);

                LoadTabs();

                OnPluginLoaded += AddTab;
            }

            private void OnDisable()
            {
                OnPluginLoaded -= AddTab;
            }

            private void LoadTabs()
            {
                tabs.Add(new CurrentTab());
                tabs.Add(new SinglePlayerTab());

                string[] guids = AssetDatabase.FindAssets("t:Script");
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                    if (script != null)
                    {
                        Type type = script.GetClass();
                        if (type != null && typeof(ISpacesSDKTab).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                        {
                            try
                            {
                                if (!IsTabAlreadyAdded(type))
                                {
                                    ISpacesSDKTab tabInstance = (ISpacesSDKTab)Activator.CreateInstance(type);
                                    tabs.Add(tabInstance);
                                }
                            }
                            catch (Exception e)
                            {
                                Debug.LogError("Failed to load tab: " + e.Message);
                            }
                        }
                    }
                }
            }

            private bool IsTabAlreadyAdded(Type type)
            {
                foreach (var tab in tabs)
                {
                    if (tab.GetType() == type)
                    {
                        return true;
                    }
                }
                return false;
            }

            void OnGUI()
            {
                EditorGUILayout.BeginHorizontal();
                DrawTabsPanel();
                DrawVerticalLine();
                DrawSelectedTabContent();
                EditorGUILayout.EndHorizontal();
            }

            void DrawTabsPanel()
            {
                EditorGUILayout.BeginVertical(GUILayout.Width(FIXED_PANEL_WIDTH));
                GUILayout.Label("Spaces Settings", EditorStyles.boldLabel);
                DrawHorizontalLine();

                foreach (var tab in tabs)
                {
                    if (GUILayout.Button(tab.TabName, EditorStyles.toolbarButton))
                    {
                        selectedTabIndex = tabs.IndexOf(tab);
                    }
                    DrawHorizontalLine();
                }
                EditorGUILayout.EndVertical();
            }

            void DrawSelectedTabContent()
            {
                if (tabs.Count > 0 && selectedTabIndex >= 0 && selectedTabIndex < tabs.Count)
                {
                    EditorGUILayout.BeginVertical();
                    scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));
                    tabs[selectedTabIndex].DrawTabGUI();
                    EditorGUILayout.EndScrollView();
                    EditorGUILayout.EndVertical();
                }
            }

            void DrawHorizontalLine()
            {
                var rect = GUILayoutUtility.GetRect(GUIContent.none, lineStyle, GUILayout.ExpandWidth(true), GUILayout.Height(1f));
                EditorGUI.DrawRect(rect, Color.grey);
            }

            void DrawVerticalLine()
            {
                EditorGUILayout.BeginVertical(lineStyle, GUILayout.Width(1), GUILayout.ExpandHeight(true));
                GUILayout.Space(MIN_HEIGHT);
                EditorGUILayout.EndVertical();
            }

            public static void AddTab(ISpacesSDKTab tab)
            {
                if (instance != null && !instance.tabs.Contains(tab))
                {
                    instance.tabs.Add(tab);
                }
            }

            public static void RemoveTab(ISpacesSDKTab tab)
            {
                if (instance != null && instance.tabs.Contains(tab))
                {
                    instance.tabs.Remove(tab);
                }
            }

            private class CurrentTab : ISpacesSDKTab
            {
                public string TabName => "Current";

                public void DrawTabGUI()
                {
                    GUILayout.Label("Current Build Settings", EditorStyles.boldLabel);

                    GUILayout.Space(10);

                    string unityVersion = Application.unityVersion;
                    GUIStyle versionStyle = new GUIStyle(EditorStyles.boldLabel);
                    if (unityVersion == "6000.0.30f1")
                    {
                        versionStyle.normal.textColor = Color.green;
                    }
                    else
                    {
                        versionStyle.normal.textColor = Color.red;
                        GUILayout.Label("Version 6.0.30f1 required", EditorStyles.boldLabel);
                    }
                    GUILayout.Label("Unity Version: " + unityVersion, versionStyle);

                    GUILayout.Space(10);

                    string pipeline = "Built In";
                    if (GraphicsSettings.currentRenderPipeline != null)
                    {
                        if (GraphicsSettings.currentRenderPipeline.GetType().Name.Contains("Universal"))
                        {
                            pipeline = "URP";
                        }
                        else if (GraphicsSettings.currentRenderPipeline.GetType().Name.Contains("HD"))
                        {
                            pipeline = "HDRP";
                        }
                    }
                    GUILayout.Label("Current Render Pipeline: " + pipeline);

                    BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
                    Color previousColor = GUI.color;
                    GUI.color = (buildTarget == BuildTarget.WebGL) ? Color.green : Color.red;
                    GUILayout.Label("Active Build Platform: " + buildTarget.ToString());
                    GUI.color = previousColor;

                    GUILayout.Space(10);
                }

                public void OnPluginLoaded(ConfigurationWindow window) { }
            }
        }
    }
}
#endif
