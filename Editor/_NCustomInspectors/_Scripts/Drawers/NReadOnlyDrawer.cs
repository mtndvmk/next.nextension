using UnityEditor;
using UnityEngine;

namespace Nextension.NEditor
{
    [CustomPropertyDrawer(typeof(NReadOnlyAttribute))]
    public class NReadOnlyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = CustomPropertyDrawerCache.getPropertyHeight(property, label);
            if (height.HasValue) return height.Value;
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginDisabledGroup(true);
            if (!CustomPropertyDrawerCache.draw(position, property, label))
            {
                EditorGUI.PropertyField(position, property, label);
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}