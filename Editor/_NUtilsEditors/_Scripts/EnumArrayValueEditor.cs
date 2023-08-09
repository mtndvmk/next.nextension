using System;
using UnityEditor;
using UnityEngine;

namespace Nextension.NEditor
{
    [CustomPropertyDrawer(typeof(IEnumArrayValue), true)]
    public class EnumArrayValueDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect contentPosition = position;
            var headerSize = position;
            headerSize.height = EditorGUIUtility.singleLineHeight;
            property.isExpanded = EditorGUI.Foldout(headerSize, property.isExpanded, label, true);
            EditorGUI.indentLevel++;
            var arrValue = (NEditorHelper.getValue(property) as IEnumArrayValue);
            if (property.isExpanded)
            {

                if (arrValue.getTypeOfValue().IsArray)
                {
                    Debug.LogError("Don't support type: " + arrValue.getTypeOfValue());
                    return;
                }

                contentPosition.height = EditorGUIUtility.singleLineHeight;
                contentPosition.y += NEditorConst.ROW_SPACING;

                var values = property.FindPropertyRelative("enumValues");

                if (values == null)
                {
                    Debug.LogError("Don't support type: " + arrValue.getTypeOfValue());
                    return;
                }

                var enumNameStyle = new GUIStyle(GUI.skin.label);
                enumNameStyle.fontStyle = FontStyle.Bold;

                object cache = null;

                if (values.arraySize != arrValue.EnumCount)
                {
                    cache = arrValue.getEditorCache();
                }

                bool hasChanged = false;
                while (values.arraySize > arrValue.EnumCount)
                {
                    values.DeleteArrayElementAtIndex(values.arraySize - 1);
                    hasChanged = true;
                }
                while (values.arraySize < arrValue.EnumCount)
                {
                    values.InsertArrayElementAtIndex(values.arraySize - 1);
                    hasChanged = true;
                }

                if (hasChanged)
                {
                    property.serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(property.serializedObject.targetObject);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    arrValue = (NEditorHelper.getValue(property) as IEnumArrayValue);
                    arrValue.applyEditorCache(cache);
                }

                contentPosition.y += EditorGUIUtility.singleLineHeight;

                var length = arrValue.Length;
                for (int i = 0; i < length; ++i)
                {
                    var enumType = arrValue.getEnumAtIndex(i);
                    var enumName = getEnumDisplayName(enumType);
                    var vPro = values.GetArrayElementAtIndex(i);
                    var h = EditorGUI.GetPropertyHeight(vPro);
                    contentPosition.height = h;
                    var valuePosition = contentPosition;
                    EditorGUI.PropertyField(valuePosition, vPro, new GUIContent(enumName), true);
                    contentPosition.y += h;
                }
            }
            arrValue.refreshEditorCache();
            EditorGUI.indentLevel--;
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
            {
                return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            var arrValue = (NEditorHelper.getValue(property) as IEnumArrayValue);
            var values = property.FindPropertyRelative("enumValues");

            if (values == null)
            {
                Debug.LogError("Don't support type: " + arrValue.getTypeOfValue());
                return EditorGUIUtility.singleLineHeight;
            }

            var height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            var length = arrValue.Length;
            for (int i = 0; i < length; ++i)
            {
                height += EditorGUI.GetPropertyHeight(values.GetArrayElementAtIndex(i));
            }

            return height;
        }


        private static string getEnumDisplayName(object enumType)
        {
            return enumType.ToString().Replace("_", " ") + $" ({(int)Convert.ChangeType(enumType, TypeCode.Int32)})";
        }
    }
}