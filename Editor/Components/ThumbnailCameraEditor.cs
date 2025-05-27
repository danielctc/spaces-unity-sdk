using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(ThumbnailCamera))]
public class ThumbnailCameraEditor : Editor
{
    private Camera previewCamera;
    private RenderTexture previewTexture;
    private const int PREVIEW_WIDTH = 256;
    private const int PREVIEW_HEIGHT = 144; // 16:9 aspect ratio

    private SerializedProperty encodeAsJPEG;
    private SerializedProperty faceCameraDirection;
    private SerializedProperty captureWidth;

    private ThumbnailCamera[] allThumbnailCameras;

    private void OnEnable()
    {
        encodeAsJPEG = serializedObject.FindProperty("encodeAsJPEG");
        faceCameraDirection = serializedObject.FindProperty("faceCameraDirection");
        captureWidth = serializedObject.FindProperty("captureWidth");

        var thumbnailCamera = (ThumbnailCamera)target;
        previewCamera = thumbnailCamera.GetComponent<Camera>();

        allThumbnailCameras = FindObjectsOfType<ThumbnailCamera>();

        if (previewCamera != null)
        {
            previewTexture = new RenderTexture(PREVIEW_WIDTH, PREVIEW_HEIGHT, 24);
            previewTexture.antiAliasing = 1;
            previewCamera.targetTexture = previewTexture;
            previewCamera.enabled = true;

            SceneView.duringSceneGui += OnSceneGUI;
        }
    }

    private void OnDisable()
    {
        if (previewCamera != null)
        {
            previewCamera.targetTexture = null;
            previewCamera.enabled = false;
        }

        if (previewTexture != null)
        {
            previewTexture.Release();
            DestroyImmediate(previewTexture);
        }

        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (previewCamera == null || previewTexture == null)
            return;

        // Force a render
        previewCamera.Render();

        // Calculate position: bottom right with margin
        float margin = 16f;
        float x = sceneView.position.width - PREVIEW_WIDTH - margin;
        float y = sceneView.position.height - PREVIEW_HEIGHT - margin - 20; // 20 for title bar

        Handles.BeginGUI();

        // Draw background box
        Rect bgRect = new Rect(x - 2, y - 22, PREVIEW_WIDTH + 4, PREVIEW_HEIGHT + 24);
        EditorGUI.DrawRect(bgRect, new Color(0.18f, 0.18f, 0.18f, 0.95f));

        // Draw title bar
        Rect titleRect = new Rect(x, y - 20, PREVIEW_WIDTH, 18);
        GUI.Label(titleRect, previewCamera.name, EditorStyles.boldLabel);

        // Draw border
        GUI.Box(new Rect(x - 1, y - 1, PREVIEW_WIDTH + 2, PREVIEW_HEIGHT + 2), GUIContent.none);

        // Draw the preview
        GUI.DrawTexture(new Rect(x, y, PREVIEW_WIDTH, PREVIEW_HEIGHT), previewTexture, ScaleMode.ScaleToFit, false);

        Handles.EndGUI();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Contextual help box
        var thumbnailCamera = (ThumbnailCamera)target;
        if (!thumbnailCamera.gameObject.activeInHierarchy)
        {
            EditorGUILayout.HelpBox("This camera is inactive because the GameObject is inactive.", MessageType.Info);
        }
        else if (allThumbnailCameras.Count(c => c.enabled) > 1)
        {
            EditorGUILayout.HelpBox("Multiple ThumbnailCameras are enabled in the scene. Only one should be active.", MessageType.Warning);
        }
        else
        {
            EditorGUILayout.HelpBox("This ThumbnailCamera is active.", MessageType.Info);
        }

        EditorGUILayout.PropertyField(encodeAsJPEG);
        EditorGUILayout.PropertyField(faceCameraDirection);
        EditorGUILayout.PropertyField(captureWidth);

        // Clamp captureWidth
        if (captureWidth.intValue < 256)
            captureWidth.intValue = 256;

        serializedObject.ApplyModifiedProperties();
    }
} 