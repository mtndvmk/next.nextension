using UnityEditor;
using UnityEngine;

namespace Nextension.NEditor
{
    [CustomPropertyDrawer(typeof(NDisplayNameAttribute))]
    public class NDisplayNameDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return CustomPropertyDrawerCache.forceGetPropertyHeight(property, label);
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var nameAttribute = attribute as NDisplayNameAttribute;
            var originColor = GUI.contentColor;
            GUI.contentColor = nameAttribute.color;
            if (!nameAttribute.displayName.isNullOrEmpty())
            {
                label.text = nameAttribute.displayName;
            }
            EditorGUI.BeginChangeCheck();
            CustomPropertyDrawerCache.forceDraw(position, property, label);
            EditorGUI.EndChangeCheck();
            GUI.contentColor = originColor;
        }
    }
}