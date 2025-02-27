#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace Spaces.Core.Editor
{
    [InitializeOnLoad]
    public class SetupWizard : EditorWindow
    {
        static SetupWizard()
        {
#if !CMPSETUP_COMPLETE
            EditorApplication.delayCall += OnInitialize;
#endif
        }

        static void OnInitialize()
        {
            // Open the wizard at startup if the preference is set.
            if (EditorPrefs.GetBool(ShowAtStartupKey, true))
            {
                ShowWindow();
            }
        }

        private Texture2D headerSectionTexture;
        private ListRequest request;
        private Vector2 scrollPosition;
        private const string ShowAtStartupKey = "SpacesSDK_ShowSetupWizardAtStartup";
        private int selectedTab = 0; // 0 = Setup, 1 = Add-Ons

        bool showTMPHelpBox = false;
        bool showInputSystemHelpBox = false;
        bool showVoiceHelpBox = false;
        bool showFusionHelpBox = false;
        bool iscompleteshowing = false;

        [MenuItem("Spaces SDK/Setup Wizard")]
        public static void ShowWindow()
        {
            // The window's title will always be "Setup Wizard"
            GetWindow(typeof(SetupWizard), false, "Setup Wizard");
        }

        private void OnEnable()
        {
            InitTextures();
            request = Client.List(); // List packages currently installed
            EditorApplication.update += Progress;
            Repaint();
        }

        private void OnDisable()
        {
            EditorApplication.update -= Progress;
        }

        private void Progress()
        {
            if (request.IsCompleted)
            {
                EditorApplication.update -= Progress;
            }
        }

        void InitTextures()
        {
            headerSectionTexture = Resources.Load("Identity") as Texture2D;
        }

        void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // Draw the header image
            GUILayout.Label(new GUIContent(headerSectionTexture));
            Repaint();
            GUILayout.Space(20);

            // Define some common GUIStyles
            GUIStyle Title = new GUIStyle(GUI.skin.label)
            {
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                wordWrap = true
            };

            GUIStyle checkStyle = new GUIStyle(GUI.skin.label)
            {
                normal = { textColor = Color.green },
                hover = { textColor = Color.green },
                fontSize = 13,
                wordWrap = true
            };

            GUILayout.Label("Welcome to Spaces SDK", Title);
            GUILayout.Space(20);

            // --- Internal Tab Toolbar ---
            // (Since the window's title bar is controlled by Unity, we simulate tabs with a toolbar inside the window.)
            string[] tabs = { "Setup", "Add-Ons" };
            selectedTab = GUILayout.Toolbar(selectedTab, tabs);
            GUILayout.Space(10);

            // Display content based on the selected tab.
            if (selectedTab == 0)
            {
                DrawSetupTab();
            }
            else if (selectedTab == 1)
            {
                DrawAddOnsTab();
            }

            EditorGUILayout.EndScrollView();

            // After drawing, check package installation status and complete setup if all are installed.
            if (request.IsCompleted)
            {
                bool packagesinstalled = AreAllPackagesInstalled();
                if (packagesinstalled)
                {
                    AddDefineSymbols("CMPSETUP_COMPLETE");
                    if (!iscompleteshowing)
                    {
                        // This would be your setup complete window or message.
                        SetupComplete.ShowWindow();
                        iscompleteshowing = true;
                    }
                }
            }
        }

        /// <summary>
        /// Draws the original Setup Wizard content.
        /// </summary>
        private void DrawSetupTab()
        {
            EditorGUILayout.Space();
            GUILayout.Label("Preferences", EditorStyles.boldLabel);

            bool showAtStartup = EditorPrefs.GetBool(ShowAtStartupKey, true);
            bool newShowAtStartup = EditorGUILayout.Toggle("Show this Panel at Startup", showAtStartup);
            if (newShowAtStartup != showAtStartup)
            {
                EditorPrefs.SetBool(ShowAtStartupKey, newShowAtStartup);
            }

            GUILayout.Space(20);
            GUILayout.Label("Build Target Setup", EditorStyles.boldLabel);

            GUIStyle checkStyle = new GUIStyle(GUI.skin.label)
            {
                normal = { textColor = Color.green },
                hover = { textColor = Color.green },
                fontSize = 13,
                wordWrap = true
            };

            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
            {
                if (GUILayout.Button("Switch to WebGL Build Target"))
                {
                    if (EditorUtility.DisplayDialog("Change Build Target",
                        "Are you sure you want to switch to WebGL? This might take some time.", "Yes", "No"))
                    {
                        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
                        Debug.Log("Build target switched to WebGL.");
                    }
                }
            }
            else
            {
                GUILayout.Label("WebGL Build Target is already selected ✔️", checkStyle);
            }

            EditorGUILayout.Space();
            GUILayout.Label("Colour Space Setup", EditorStyles.boldLabel);
            if (PlayerSettings.colorSpace != ColorSpace.Linear)
            {
                if (GUILayout.Button("Switch to Linear Color Space"))
                {
                    PlayerSettings.colorSpace = ColorSpace.Linear;
                }
            }
            else
            {
                GUILayout.Label("Done ✔️", checkStyle);
            }

            EditorGUILayout.Space();

            // --- Scene Setup ---
            EditorGUILayout.Space();
            GUILayout.Label("Single Player [Required]", EditorStyles.boldLabel);

            string gameCreatorPath = "Assets/SpacesSDK/Installs/GameCreator2.17.51.unitypackage";
            if (Directory.Exists("Assets/Plugins/GameCreator"))
            {
                GUILayout.Label("Game Creator 2.17.51 Installed ✔️", checkStyle);
            }
            else if (File.Exists(gameCreatorPath))
            {
                if (GUILayout.Button("Install Game Creator 2"))
                {
                    AssetDatabase.ImportPackage(gameCreatorPath, true);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Game Creator 2 package not found at: " + gameCreatorPath, MessageType.Error);
            }

            EditorGUILayout.Space();

            // --- ReadyPlayerMe ---
            string rpmPath = "Assets/SpacesSDK/Installs/ReadyPlayerMe7.3.1.unitypackage";
            if (Directory.Exists("Assets/Ready Player Me"))
            {
                GUILayout.Label("ReadyPlayerMe 7.3.1 Installed ✔️", checkStyle);
            }
            else if (File.Exists(rpmPath))
            {
                if (GUILayout.Button("Install ReadyPlayerMe"))
                {
                    // Install dependencies first
                    Client.Add("com.unity.cloud.gltfast@6.0.1");
                    Client.Add("com.unity.nuget.newtonsoft-json@3.2.1");
                    
                    // Import the main package after dependencies are installed
                    EditorApplication.delayCall += () => {
                        AssetDatabase.ImportPackage(rpmPath, true);
                    };
                }
            }
            else
            {
                EditorGUILayout.HelpBox("ReadyPlayerMe package not found at: " + rpmPath, MessageType.Error);
            }

            EditorGUILayout.Space();

            GUILayout.Space(10);
            GUILayout.Label("Multiplayer", EditorStyles.boldLabel);

            if (request.IsCompleted)
            {
                if (request.Status == StatusCode.Success)
                {
                    // --- Fusion 2 Package ---
                    string fusionPath = "Assets/SpacesSDK/Installs/Fusion2.0.4.unitypackage";
                    if (Directory.Exists("Assets/Photon/Fusion"))
                    {
                        GUILayout.Label("Fusion 2.0.4 Installed ✔️", checkStyle);
                    }
                    else if (File.Exists(fusionPath))
                    {
                        if (GUILayout.Button("Install Fusion 2"))
                        {
                            AssetDatabase.ImportPackage(fusionPath, true);
                            showFusionHelpBox = true;
                        }
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("Fusion 2 package not found at: " + fusionPath, MessageType.Error);
                    }
                    
                    if (showFusionHelpBox)
                    {
                        EditorGUILayout.HelpBox(
                            "Make sure to input your 'Photon Fusion 2' App ID in Assets/Photon/Fusion/Resources/PhotonAppSettings.asset",
                            MessageType.Info);
                    }

                    // --- Photon Voice Package ---
                    string voicePath = "Assets/SpacesSDK/Installs/PhotonVoice2.57.0.unitypackage";
                    if (Directory.Exists("Assets/Photon/PhotonVoice"))
                    {
                        GUILayout.Label("Photon Voice 2.57.0 Installed ✔️", checkStyle);
                    }
                    else if (File.Exists(voicePath))
                    {
                        if (GUILayout.Button("Install Photon Voice 2"))
                        {
                            AssetDatabase.ImportPackage(voicePath, true);
                            showVoiceHelpBox = true;
                        }
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("Photon Voice 2 package not found at: " + voicePath, MessageType.Error);
                    }

                    if (showVoiceHelpBox)
                    {
                        EditorGUILayout.HelpBox(
                            "Make sure to input your 'Photon Voice' App ID in Assets/Photon/Fusion/Resources/PhotonAppSettings.asset",
                            MessageType.Info);
                    }

                    // --- Game Creator 2 Fusion Module ---
                    string gcFusionPath = "Assets/SpacesSDK/Installs/GC2_Fusion1.2.7.unitypackage";
                    if (Directory.Exists("Assets/Plugins/NinjutsuGames"))
                    {
                        GUILayout.Label("Game Creator 2 Fusion Module 1.2.7 Installed ✔️", checkStyle);
                    }
                    else if (File.Exists(gcFusionPath))
                    {
                        if (GUILayout.Button("Install Game Creator 2 Fusion Module"))
                        {
                            AssetDatabase.ImportPackage(gcFusionPath, true);
                        }
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("Game Creator 2 Fusion Module package not found at: " + gcFusionPath, MessageType.Error);
                    }

                    // --- Option to Remove PUN (if applicable) ---
                    GUI.enabled = IsPhotonVoiceInstalled() && HasPun;
                    if (IsPhotonVoiceInstalled() && GUILayout.Button("Remove PUN from Photon Voice"))
                    {
                        RemovePun();
                    }
                    GUI.enabled = true;
                    if (IsPhotonVoiceInstalled() && !HasPun)
                    {
                        GUILayout.Label("Pun Removed ✔️", checkStyle);
                    }

                    // --- Input System Package ---
#if ENABLE_INPUT_SYSTEM
                    GUI.enabled = false;
#endif
                    if (GUILayout.Button("Install InputSystem & Enable it"))
                    {
                        bool isInstalled = IsPackageInstalled("com.unity.inputsystem");
                        using (new EditorGUI.DisabledScope(isInstalled))
                        {
                            Client.Add("com.unity.inputsystem");
                        }
                        showInputSystemHelpBox = true;
                    }
                    GUI.enabled = true;
                    if (IsPackageInstalled("com.unity.inputsystem"))
                    {
#if ENABLE_INPUT_SYSTEM
                        GUILayout.Label("Installed and Enabled ✔️", checkStyle);
                        showInputSystemHelpBox = false;
#else
                        showInputSystemHelpBox = true;
                        GUILayout.Label("Installed, but not enabled");
#endif
                    }
                    if (showInputSystemHelpBox)
                    {
                        EditorGUILayout.HelpBox(
                            "To enable the input system: Go to 'Edit' -> 'Project Settings' -> 'Player'. Expand the 'Other Settings' section. Locate the 'Active Input Handling' option. Set it to 'Both'. This should restart your editor",
                            MessageType.Info);
                    }
#if ENABLE_INPUT_SYSTEM
                    showInputSystemHelpBox = false;
#endif

                    // --- TextMeshPro ---
                    bool isTMPinstall = IsTextMeshProInstalled();
                    if (isTMPinstall)
                        GUI.enabled = false;
                    if (GUILayout.Button("Install TextMeshPro"))
                    {
                        showTMPHelpBox = true;
                    }
                    if (showTMPHelpBox)
                    {
                        EditorGUILayout.HelpBox(
                            "Please go to 'Window -> TextMeshPro -> Import TMP Essential Resources' to install it",
                            MessageType.Info);
                    }
                    GUI.enabled = true;
                    if (isTMPinstall)
                    {
                        GUILayout.Label("Installed ✔️", checkStyle);
                        showTMPHelpBox = false;
                    }

                    // --- Additional Packages ---
                    AddPackageButton("com.unity.postprocessing", "Post Processing");
                }
                else
                {
                    EditorGUILayout.LabelField("Failed to list packages.");
                }
            }
            else
            {
                EditorGUILayout.LabelField("Loading package list...");
            }
        }

        /// <summary>
        /// Draws the Add‑Ons tab content.
        /// </summary>
        private void DrawAddOnsTab()
        {
            EditorGUILayout.Space();
            GUILayout.Label("Add‑Ons Management", EditorStyles.boldLabel);

            if (GUILayout.Button("Install Additional Tool 1"))
            {
                Debug.Log("Installing Additional Tool 1...");
            }
            if (GUILayout.Button("Install Additional Tool 2"))
            {
                Debug.Log("Installing Additional Tool 2...");
            }
            if (GUILayout.Button("Manage Installed Add‑Ons"))
            {
                Debug.Log("Opening Add‑Ons Management...");
            }

            EditorGUILayout.Space();
            GUILayout.Label("Installed Add‑Ons", EditorStyles.boldLabel);
            GUILayout.Label("• Tool 1 ✔️", EditorStyles.label);
            GUILayout.Label("• Tool 2 ✔️", EditorStyles.label);
        }

        private void AddPackageButton(string packageId, string displayName)
        {
            bool isInstalled = IsPackageInstalled(packageId);
            using (new EditorGUI.DisabledScope(isInstalled))
            {
                if (GUILayout.Button($"Install {displayName}"))
                {
                    Client.Add(packageId);
                }
            }
            if (isInstalled)
            {
                GUIStyle checkStyle = new GUIStyle(GUI.skin.label)
                {
                    normal = { textColor = Color.green },
                    hover = { textColor = Color.green },
                    fontSize = 13,
                    wordWrap = true
                };
                GUILayout.Label("Installed ✔️", checkStyle);
            }
        }

        private void AddDefineSymbols(string defineSymbol)
        {
            string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            List<string> allDefines = definesString.Split(';').ToList();
            if (!allDefines.Contains(defineSymbol))
            {
                allDefines.Add(defineSymbol);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup,
                    string.Join(";", allDefines.ToArray()));
            }
        }

        bool IsFusionInstalled()
        {
#if FUSION_WEAVER
            return true;
#else
            return false;
#endif
        }

        private bool IsPhotonVoiceInstalled()
        {
            var result = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "PhotonVoice.Fusion");
            return result != null;
        }

        private bool IsFusionPhysicsAddOnInstall()
        {
            var result = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "Fusion.Addons.Physics");
            return result != null;
        }

        public static bool HasPun
        {
            get
            {
                return Type.GetType("Photon.Pun.PhotonNetwork, Assembly-CSharp") != null ||
                       Type.GetType("Photon.Pun.PhotonNetwork, Assembly-CSharp-firstpass") != null ||
                       Type.GetType("Photon.Pun.PhotonNetwork, PhotonUnityNetworking") != null;
            }
        }

        bool IsTextMeshProInstalled()
        {
            return Directory.Exists("Assets/TextMesh Pro");
        }

        private bool IsPackageInstalled(string packageId)
        {
            foreach (var package in request.Result)
            {
                if (package.packageId.StartsWith(packageId))
                {
                    return true;
                }
            }
            return false;
        }

        private static void RemovePun()
        {
            DeleteDirectory("Assets/Photon/PhotonVoice/Demos/DemoVoiceProximityChat");
            DeleteDirectory("Assets/Photon/PhotonVoice/Demos/DemoVoicePun");
            DeleteDirectory("Assets/Photon/PhotonVoice/Code/PUN");
            DeleteDirectory("Assets/Photon/PhotonUnityNetworking");
            CleanUpPunDefineSymbols();
            if (EditorUtility.DisplayDialog("SPACES SDK", "Please Restart the editor for proper installation", "Restart"))
            {
                EditorApplication.OpenProject(Directory.GetCurrentDirectory());
            }
        }

        public static void DeleteDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                if (!FileUtil.DeleteFileOrDirectory(path))
                {
                    Debug.LogWarningFormat("Directory \"{0}\" not deleted.", path);
                }
                DeleteFile(string.Concat(path, ".meta"));
            }
            else
            {
                Debug.LogWarningFormat("Directory \"{0}\" does not exist.", path);
            }
        }

        public static void DeleteFile(string path)
        {
            if (File.Exists(path))
            {
                if (!FileUtil.DeleteFileOrDirectory(path))
                {
                    Debug.LogWarningFormat("File \"{0}\" not deleted.", path);
                }
            }
            else
            {
                Debug.LogWarningFormat("File \"{0}\" does not exist.", path);
            }
        }

        public static void CleanUpPunDefineSymbols()
        {
            foreach (BuildTarget target in Enum.GetValues(typeof(BuildTarget)))
            {
                BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup(target);
                if (group == BuildTargetGroup.Unknown)
                {
                    continue;
                }

                var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(group)
                    .Split(';')
                    .Select(d => d.Trim())
                    .ToList();

                List<string> newDefineSymbols = new List<string>();
                foreach (var symbol in defineSymbols)
                {
                    if ("PHOTON_UNITY_NETWORKING".Equals(symbol) || symbol.StartsWith("PUN_2_"))
                    {
                        continue;
                    }
                    newDefineSymbols.Add(symbol);
                }

                try
                {
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", newDefineSymbols.ToArray()));
                }
                catch (Exception e)
                {
                    Debug.LogErrorFormat("Could not set clean up PUN2's define symbols for build target: {0} group: {1}, {2}", target, group, e);
                }
            }
        }

        bool AreAllPackagesInstalled()
        {
            var packageIds = new string[]
            {
                "com.unity.inputsystem",
#if !UNITY_2023_2_OR_NEWER
                "com.unity.textmeshpro",
#endif
                "com.unity.postprocessing"
            };

            foreach (string packageId in packageIds)
            {
                if (!IsPackageInstalled(packageId))
                {
                    return false;
                }
            }

            if (!IsFusionInstalled())
                return false;
            if (!IsPhotonVoiceInstalled())
                return false;
            if (HasPun)
                return false;
            return true;
        }
    }
}
#endif 