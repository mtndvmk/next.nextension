using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Nextension.NEditor
{
    public static class NEditorHelper
    {
        public static BindingFlags getAllBindingFlags()
        {
            return (BindingFlags)(-1);
        }

        public static object getValue(SerializedProperty property)
        {
            var fieldInfo = getFieldInfo(property, out var value);
            if (fieldInfo == null)
            {
                return null;
            }
            return value;
        }
        public static FieldInfo getFieldInfo(SerializedProperty property)
        {
            return getFieldInfo(property, out var _);
        }
        public static T getAttribute<T>(SerializedProperty property) where T : System.Attribute
        {
            var fieldInfo = getFieldInfo(property);
            return fieldInfo.GetCustomAttribute<T>();
        }

        public static FieldInfo getFieldInfo(SerializedProperty property, out object qObject)
        {
            var path = property.propertyPath.Replace(".Array.data[", "[");
            var elements = path.Split('.');

            qObject = property.serializedObject.targetObject;
            FieldInfo fieldInfo = null;

            try
            {
                for (int i = 0; i < elements.Length; ++i)
                {
                    var element = elements[i];
                    if (element.Contains("["))
                    {
                        var elementName = element.Substring(0, element.IndexOf("["));
                        var index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));

                        var qType = qObject.GetType();
                        fieldInfo = qType.getField(elementName, getAllBindingFlags());
                        qObject = (fieldInfo.GetValue(qObject) as IList)[index];
                    }
                    else
                    {
                        fieldInfo = qObject.GetType().getField(element, getAllBindingFlags());
                        qObject = fieldInfo.GetValue(qObject);
                    }
                }

                return fieldInfo;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return default;
            }
        }
    }
}
