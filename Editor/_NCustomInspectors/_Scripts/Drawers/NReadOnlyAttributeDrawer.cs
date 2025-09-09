using UnityEditor;
using UnityEngine;

namespace Nextension.NEditor
{
    [CustomPropertyDrawer(typeof(NReadOnlyAttribute))]
    public class NReadOnlyAttributeDrawer : PropertyDrawer
    {
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