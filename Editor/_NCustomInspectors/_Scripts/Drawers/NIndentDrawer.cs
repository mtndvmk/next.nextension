using UnityEditor;
using UnityEngine;

namespace Nextension.NEditor
{
    [CustomPropertyDrawer(typeof(NIndentAttribute))]
    public class NIndentDrawer : PropertyDrawer
    {       
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = CustomPropertyDrawerCache.getPropertyHeight(property, label);
            if (height.HasValue) return height.Value;
            else return EditorGUI.GetPropertyHeight(property, label, true);
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            var indentAttribute = attribute as NIndentAttribute;
            EditorGUI.indentLevel += indentAttribute.indentLevel;
            label.text = $"{indentAttribute.bullet} {label}";
            if (!CustomPropertyDrawerCache.draw(position, property, label))
            {
                EditorGUI.PropertyField(position, property, label);
            }
            EditorGUI.indentLevel -= indentAttribute.indentLevel;
            EditorGUI.EndChangeCheck();
        }
    }
}