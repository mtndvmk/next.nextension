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
            if (NAssetUtils.IsCompiling || property == null || property.serializedObject == null)
            {
                return;
            }

#if UNITY_2022_3_OR_NEWER
            EditorGUI.indentLevel--;
#endif
            Rect contentPosition = position;
            var headerSize = position;
            headerSize.height = EditorGUIUtility.singleLineHeight;
            property.isExpanded = EditorGUI.Foldout(headerSize, property.isExpanded, label, true);
#if UNITY_2022_3_OR_NEWER
            EditorGUI.indentLevel++;
#endif
            var arrValue = (NEditorHelper.getValue(property) as IEnumArrayValue);
            if (arrValue != null)
            {
                EditorGUI.indentLevel++;
                try
                {
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

                        bool hasChanged = arrValue.refreshEditorCache();

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
                            NAssetUtils.saveAsset(property.serializedObject.targetObject);
                            if (NEditorHelper.getValue(property) is IEnumArrayValue arrValue2)
                            {
                                arrValue = arrValue2;
                            }
                            else
                            {
                                Debug.LogWarning("Error when get array value");
                                return;
                            }
                        }

                        contentPosition.y += EditorGUIUtility.singleLineHeight;

                        var length = arrValue.Length;
                        for (int i = 0; i < length; ++i)
                        {
                            var enumType = arrValue.getEnumAtIndexAsObject(i);
                            var enumName = getEnumDisplayName(enumType);
                            var vPro = values.GetArrayElementAtIndex(i);
                            var h = EditorGUI.GetPropertyHeight(vPro);
                            contentPosition.height = h;
                            var valuePosition = contentPosition;
                            EditorGUI.PropertyField(valuePosition, vPro, new GUIContent(enumName), true);
                            contentPosition.y += h;
                        }
                    }
                    else
                    {
                        arrValue.refreshEditorCache();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                finally
                {
                    EditorGUI.indentLevel--;
                }
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            try
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
            catch (Exception e)
            {
                Debug.LogException(e);
                return 0;
            }
        }


        private static string getEnumDisplayName(object enumType)
        {
            return enumType.ToString().Replace("_", " ") + $" ({(int)Convert.ChangeType(enumType, TypeCode.Int32)})";
        }
    }
}