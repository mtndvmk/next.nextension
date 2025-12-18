using UnityEditor;
using UnityEngine;

namespace Nextension.NEditor
{
    [CustomPropertyDrawer(typeof(NIndentAttribute))]
    public class NIndentDrawer : PropertyDrawer
    {       
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return CustomPropertyDrawerCache.forceGetPropertyHeight(property, label);
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            var indentAttribute = attribute as NIndentAttribute;
            EditorGUI.indentLevel += indentAttribute.indentLevel;
            label.text = $"{indentAttribute.bullet} {label}";
            CustomPropertyDrawerCache.forceDraw(position, property, label);
            EditorGUI.indentLevel -= indentAttribute.indentLevel;
            EditorGUI.EndChangeCheck();
        }
    }
}