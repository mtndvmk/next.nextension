using UnityEditor;
using UnityEngine;

namespace Nextension.NEditor
{
    [CustomPropertyDrawer(typeof(NMinMax01))]
    public class NMinMax01Editor : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float minnn = 0;
            float maxxx = 1;
            var minPro = property.FindPropertyRelative("min");
            var maxPro = property.FindPropertyRelative("max");

            position.y += EditorGUIUtility.standardVerticalSpacing;
            var labelRect = position;
            labelRect.height = EditorGUIUtility.singleLineHeight;
            labelRect.width = GUI.skin.label.CalcSize(label).x * 2;
            EditorGUI.LabelField(labelRect, label);
            var rect = position;
            rect.width = position.width - labelRect.width;
            rect.x += labelRect.width;
            rect.height = EditorGUIUtility.singleLineHeight;
            position.height = EditorGUIUtility.singleLineHeight;
            var minRect = rect;
            minRect.width /= 2;
            var maxRect = minRect;
            maxRect.x += maxRect.width;

            var minLabel = new GUIContent("Min");
            var minLabelRect = minRect;
            minLabelRect.width = GUI.skin.label.CalcSize(minLabel).x * 2;
            minRect.width -= minLabelRect.width;
            minRect.x += minLabelRect.width;

            EditorGUI.LabelField(minLabelRect, minLabel);
            EditorGUI.PropertyField(minRect, minPro, GUIContent.none);

            var maxLabel = new GUIContent("Max");
            var maxLabelRect = maxRect;
            maxLabelRect.width = GUI.skin.label.CalcSize(maxLabel).x * 2;
            maxRect.width -= maxLabelRect.width;
            maxRect.x += maxLabelRect.width;
            EditorGUI.LabelField(maxLabelRect, maxLabel);
            EditorGUI.PropertyField(maxRect, maxPro, GUIContent.none);

            var minValue = minPro.floatValue;
            var maxValue = maxPro.floatValue;
            if (minValue > maxValue)
            {
                maxValue = minValue;
            }

            position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.indentLevel++;
            EditorGUI.MinMaxSlider(position, ref minValue, ref maxValue, minnn, maxxx);
            maxPro.floatValue = maxValue;
            minPro.floatValue = minValue;
            EditorGUI.indentLevel--;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing * 2;
        }
    }
}
