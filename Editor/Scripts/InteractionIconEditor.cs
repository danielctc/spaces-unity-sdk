using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(InteractionIconDetails))]
public class InteractionIconEditor : Editor
{
    private SerializedProperty currentIconTypeProp;
    private SerializedProperty iconSpritesProp;
    private SerializedProperty customIconProp;
    private SerializedProperty iconRendererProp;
    private GUIStyle _headerStyle;
    private GUIStyle _titleStyle;
    private GUIStyle _subTitleStyle;
    private string _prettyName;
    private string _tooltip;
    private bool _initialized;

    private void OnEnable()
    {
        currentIconTypeProp = serializedObject.FindProperty("currentIconType");
        iconSpritesProp = serializedObject.FindProperty("iconSprites");
        customIconProp = serializedObject.FindProperty("customIcon");
        iconRendererProp = serializedObject.FindProperty("iconRenderer");

        if (target is InteractionIconDetails component)
        {
            _prettyName = component.prettyName;
            _tooltip = component.tooltip;
        }
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
            EditorGUILayout.LabelField(_prettyName, _titleStyle);
            EditorGUILayout.LabelField(_tooltip, _subTitleStyle);
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10);

        EditorGUILayout.PropertyField(iconRendererProp, new GUIContent("Icon Renderer"));
        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Icon Selection", EditorStyles.boldLabel);

        // Draw the enum dropdown
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(currentIconTypeProp, new GUIContent("Icon Type"));
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            ((InteractionIconDetails)target).SetIconType((InteractionIconDetails.IconType)currentIconTypeProp.enumValueIndex);
        }

        // Show either the icon sprites array or custom icon field based on selection
        if (currentIconTypeProp.enumValueIndex == (int)InteractionIconDetails.IconType.Custom)
        {
            EditorGUILayout.PropertyField(customIconProp, new GUIContent("Custom Icon"));
        }
        else if (currentIconTypeProp.enumValueIndex != (int)InteractionIconDetails.IconType.None)
        {
            EditorGUILayout.PropertyField(iconSpritesProp, new GUIContent("Icon Sprites"), true);
        }

        serializedObject.ApplyModifiedProperties();
    }
} 