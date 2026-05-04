using UnityEditor;
using UnityEngine;

namespace Nextension.NEditor
{
    [CustomPropertyDrawer(typeof(NDictionary), true)]
    public class NDictionaryDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            var size = GUI.skin.box.CalcSize(label);
            var headerSize = position;
            var h = size.y;
            headerSize.height = h;

            property.isExpanded = EditorGUI.Foldout(headerSize, property.isExpanded, label, true);
            if (property.isExpanded)
            {
                var itemProperty = property.FindPropertyRelative("items");
                position.y += h;
                position.x += 15;
                position.width -= 15;
                EditorGUI.PropertyField(position, itemProperty, new GUIContent("Items"));

                var nDict = NEditorHelper.getValue(property) as NDictionary;
                if (nDict.isHasInvalidKeys())
                {
                    var buttonPosition = position;
                    buttonPosition.y += position.height - h - h;
                    buttonPosition.height = h;
                    var buttonLabel = new GUIContent("  Remove null/duplicate key item(s)");
                    if (GUI.Button(buttonPosition, buttonLabel))
                    {
                        nDict.removeInvalidItems();
                    }
                }
            }
            EditorGUI.EndChangeCheck();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
            {
                return GUI.skin.box.CalcSize(label).y;
            }

            var nDict = NEditorHelper.getValue(property) as NDictionary;

            float height = EditorGUIUtility.standardVerticalSpacing;

            height += EditorGUI.GetPropertyHeight(property);

            if (nDict.isHasInvalidKeys())
            {
                height += GUI.skin.box.CalcSize(label).y;
            }

            return height;
        }
    }
}
