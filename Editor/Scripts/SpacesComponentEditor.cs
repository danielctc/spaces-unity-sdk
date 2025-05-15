using UnityEngine;
using UnityEditor;
using Spaces.Core.Runtime;

namespace SpacesSDK.Editor
{
    public abstract class SpacesComponentEditorBase : UnityEditor.Editor
    {
        private bool _initialized;
        private static readonly string[] _excludedProperties = new string[] { "m_Script" };
        private GUIStyle _headerStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _subTitleStyle;
        private string _prettyName;
        private string _tooltip;

        protected abstract void GetComponentInfo(UnityEngine.Object target, out string prettyName, out string tooltip);

        private void InitializeIfNecessary(UnityEngine.Object target)
        {
            if (_initialized) return;

            GetComponentInfo(target, out _prettyName, out _tooltip);

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
            var editorTarget = target as UnityEngine.Object;
            InitializeIfNecessary(editorTarget);
            serializedObject.Update();

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

            // Properties
            DrawPropertiesExcluding(serializedObject, _excludedProperties);

            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(InteractionIconDetails), true)]
    public class InteractionIconDetailsEditor : SpacesComponentEditorBase
    {
        protected override void GetComponentInfo(UnityEngine.Object target, out string prettyName, out string tooltip)
        {
            if (target is InteractionIconDetails component)
            {
                prettyName = component.prettyName;
                tooltip = component.tooltip;
            }
            else
            {
                prettyName = target.GetType().Name;
                tooltip = "";
            }
        }
    }

    [CustomEditor(typeof(InterestPointDetails), true)]
    public class InterestPointDetailsEditor : SpacesComponentEditorBase
    {
        protected override void GetComponentInfo(UnityEngine.Object target, out string prettyName, out string tooltip)
        {
            if (target is InterestPointDetails component)
            {
                prettyName = component.prettyName;
                tooltip = component.tooltip;
            }
            else
            {
                prettyName = target.GetType().Name;
                tooltip = "";
            }
        }
    }
} 