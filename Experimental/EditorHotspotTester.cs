#if UNITY_EDITOR
using UnityEngine;
using System.Reflection;

/// <summary>
/// Editor-only helper that lets you paste a GLB URL and a hotspot prefab in the
/// inspector, then loads the GLB and attaches the prefab to every transform
/// whose name ends with "-hotspot". Ideal for testing inside the Unity Editor
/// without the React pipeline or WebGL build.
/// </summary>
[AddComponentMenu("Testing/Editor Hotspot Tester")]
public class EditorHotspotTester : MonoBehaviour
{
    [Header("GLB Source")]
    [Tooltip("Absolute or http(s) URL of the .glb file to download and load in the editor.")]
    [SerializeField] private string glbUrl = string.Empty;

    [Header("Hotspot Prefab")] 
    [Tooltip("Prefab to instantiate at every *-hotspot transform inside the downloaded GLB.")]
    [SerializeField] private GameObject testPrefab;

    private GLBDownloader _downloader;

    private void Start()
    {
        if (string.IsNullOrEmpty(glbUrl))
        {
            Debug.LogError("[EditorHotspotTester] Please provide a GLB URL in the inspector.", this);
            return;
        }
        if (testPrefab == null)
        {
            Debug.LogError("[EditorHotspotTester] Please assign a hotspot prefab in the inspector.", this);
            return;
        }

        // Ensure a GLBDownloader component exists on the same GO
        _downloader = GetComponent<GLBDownloader>() ?? gameObject.AddComponent<GLBDownloader>();

        ForceEditorTestingMode(_downloader);

        // Make sure we have a HotspotDetector in the scene and set its prefab
        var detector = FindObjectOfType<HotspotDetector>();
        if (detector == null)
        {
            detector = new GameObject("HotspotDetector").AddComponent<HotspotDetector>();
        }
        // Assign testPrefab using reflection because the field is private
        var prefabField = typeof(HotspotDetector).GetField("testPrefab", BindingFlags.NonPublic | BindingFlags.Instance);
        prefabField?.SetValue(detector, testPrefab);

        // Ensure we are in local instantiate mode while testing in the Editor (avoid NetworkRunner requirement)
        var networkSpawnField = typeof(HotspotDetector).GetField("networkSpawn", BindingFlags.NonPublic | BindingFlags.Instance);
        networkSpawnField?.SetValue(detector, false);

        // After the GLB is loaded run hotspot detection once
        _downloader.OnModelLoaded += () => detector.DetectHotspots();

        // Kick off the download
        _downloader.Url = glbUrl;
    }

    /// <summary>
    /// GLBDownloader has a private "testingMode" bool; toggle it on via reflection so it
    /// uses the AssetDatabase loading path that works in the Editor.
    /// </summary>
    private static void ForceEditorTestingMode(GLBDownloader downloader)
    {
        var flag = BindingFlags.Instance | BindingFlags.NonPublic;
        var field = typeof(GLBDownloader).GetField("testingMode", flag);
        if (field != null)
        {
            field.SetValue(downloader, true);
        }
    }
}
#endif 