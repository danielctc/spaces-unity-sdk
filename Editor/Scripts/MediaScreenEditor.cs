using UnityEngine;
using UnityEditor;
using Spaces.React.Runtime;

[CustomEditor(typeof(MediaScreen))]
public class MediaScreenEditor : Editor
{
    private SerializedProperty targetRendererProp;
    private SerializedProperty screenIdProp;
    private GUIStyle _headerStyle;
    private GUIStyle _titleStyle;
    private GUIStyle _subTitleStyle;
    private bool _initialized;

    private void OnEnable()
    {
        targetRendererProp = serializedObject.FindProperty("targetRenderer");
        screenIdProp = serializedObject.FindProperty("screenId");
    }

    private void InitializeStyles()
    {
        if (_initialized) return;

        // Initialize styles
        _headerStyle = new GUIStyle()
        {
            padding = new RectOffset(10, 10, 10, 10),
            margin = new RectOffset(0, 0, 0, 0)
        };

        _titleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 16,
            margin = new RectOffset(0, 0, 4, 4),
            normal = { textColor = Color.white }
        };

        _subTitleStyle = new GUIStyle(EditorStyles.label)
        {
            fontSize = 12,
            wordWrap = true,
            richText = true,
            normal = { textColor = new Color(0.7f, 0.7f, 0.7f) }
        };

        _initialized = true;
    }

    public override void OnInspectorGUI()
    {
        if (serializedObject == null) return;

        serializedObject.Update();
        InitializeStyles();

        // Draw black background
        Rect headerRect = EditorGUILayout.GetControlRect(false, 0);
        headerRect.x -= 10;
        headerRect.width += 20;
        EditorGUI.DrawRect(headerRect, Color.black);

        // Header
        EditorGUILayout.BeginVertical(_headerStyle);
        {
            EditorGUILayout.LabelField("Media Screen", _titleStyle);
            EditorGUILayout.LabelField("Displays images and videos on a 3D surface with interactive capabilities", _subTitleStyle);
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10);

        // Properties
        EditorGUILayout.PropertyField(targetRendererProp, new GUIContent("Target Renderer", "The mesh renderer where the media will be displayed"));
        EditorGUILayout.PropertyField(screenIdProp, new GUIContent("Screen ID", "Unique identifier for this media screen (required for React)"));

        serializedObject.ApplyModifiedProperties();
    }
} 