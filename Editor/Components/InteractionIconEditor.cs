using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;

namespace Spaces.Core.Editor
{
    [CustomEditor(typeof(InteractionIconDetails))]
    public class InteractionIconEditor : UnityEditor.Editor
    {
        private SerializedProperty currentIconTypeProp;
        private SerializedProperty iconSpritesProp;
        private SerializedProperty customIconProp;
        private SerializedProperty iconRendererProp;

        private void OnEnable()
        {
            currentIconTypeProp = serializedObject.FindProperty("currentIconType");
            iconSpritesProp = serializedObject.FindProperty("iconSprites");
            customIconProp = serializedObject.FindProperty("customIcon");
            iconRendererProp = serializedObject.FindProperty("iconRenderer");
        }

        public override void OnInspectorGUI()
        {
            if (serializedObject == null) return;

            serializedObject.Update();

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
} 