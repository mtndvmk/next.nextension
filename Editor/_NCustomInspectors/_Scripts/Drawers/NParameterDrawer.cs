using UnityEditor;
using UnityEngine;

namespace Nextension.NEditor
{
    [CustomPropertyDrawer(typeof(NParameter))]
    public class NParameterDrawer : PropertyDrawer
    {
        private const int ROW_COUNT = 3;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var headerRect = position;
            var h = GUI.skin.label.CalcSize(label).y;
            headerRect.height = h;

            property.isExpanded = EditorGUI.Foldout(headerRect, property.isExpanded, label, true);

            if (!property.isExpanded)
            {
                return;
            }

            Rect rowPosition = position;
            rowPosition.height = h;
            rowPosition.y += h + NEditorConst.ROW_SPACING;
            rowPosition.x += 15;
            rowPosition.width -= 15;

            var typeLable = new GUIContent("Type");
            var valueLabel = new GUIContent("Value");

            var typePropertyRect = rowPosition;
            EditorGUI.PropertyField(typePropertyRect, property.FindPropertyRelative("type"), typeLable);

            var valuePropertyRect = typePropertyRect;
            valuePropertyRect.y += h + NEditorConst.ROW_SPACING;

            var type = (NParameter.ParameterType)property.FindPropertyRelative("type").intValue;
            SerializedProperty serializedProperty = null;
            switch (type)
            {
                case NParameter.ParameterType.Bool:
                    serializedProperty = property.FindPropertyRelative("boolValue");
                    break;
                case NParameter.ParameterType.Number:
                    serializedProperty = property.FindPropertyRelative("numberValue");
                    break;
                case NParameter.ParameterType.String:
                    serializedProperty = property.FindPropertyRelative("stringValue");
                    break;
                case NParameter.ParameterType.Component:
                    serializedProperty = property.FindPropertyRelative("componentValue");
                    break;
            }
            EditorGUI.PropertyField(valuePropertyRect, serializedProperty, valueLabel);
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
            {
                return base.GetPropertyHeight(property, label);
            }
            return base.GetPropertyHeight(property, label) * ROW_COUNT + (ROW_COUNT - 1) * NEditorConst.ROW_SPACING;
        }
    }
}
