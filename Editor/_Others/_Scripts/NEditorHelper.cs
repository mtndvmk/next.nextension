using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Nextension.NEditor
{
    public static class NEditorHelper
    {
        public static object getValue(SerializedProperty property)
        {
            var fieldInfo = getFieldInfo(property, out var value);
            if (fieldInfo == null)
            {
                return null;
            }
            return value;
        }
        public static T getValue<T>(SerializedProperty property)
        {
            var fieldInfo = getFieldInfo(property, out var value);
            if (fieldInfo == null)
            {
                return default;
            }
            return (T)value;
        }
        public static FieldInfo getFieldInfo(SerializedProperty property)
        {
            return getFieldInfo(property, out var _);
        }
        public static T getAttribute<T>(SerializedProperty property) where T : Attribute
        {
            var fieldInfo = getFieldInfo(property);
            return fieldInfo.GetCustomAttribute<T>();
        }

        public static FieldInfo getFieldInfo(SerializedProperty property, out object valueOfProperty)
        {
            try
            {
                var path = property.propertyPath.Replace(".Array.data[", "[");
                var elements = path.Split('.');

                valueOfProperty = property.serializedObject.targetObject;
                FieldInfo fieldInfo = null;

                for (int i = 0; i < elements.Length; ++i)
                {
                    var element = elements[i];
                    if (element.Contains("["))
                    {
                        var elementName = element.Substring(0, element.IndexOf("["));
                        var index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));

                        var qType = valueOfProperty.GetType();
                        fieldInfo = qType.getField(elementName, NUtils.getAllBindingFlags());
                        valueOfProperty = (fieldInfo.GetValue(valueOfProperty) as IList)[index];
                    }
                    else
                    {
                        fieldInfo = valueOfProperty.GetType().getField(element, NUtils.getAllBindingFlags());
                        valueOfProperty = fieldInfo.GetValue(valueOfProperty);
                    }
                }

                return fieldInfo;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                valueOfProperty = default;
                return default;
            }
        }
        public static object getContainerObject(SerializedProperty property)
        {
            try
            {
                var path = property.propertyPath.Replace(".Array.data[", "[");
                var elements = path.Split('.');
                FieldInfo fieldInfo = null;
                object containerObject = property.serializedObject.targetObject;

                if (elements.Length == 1)
                {
                    return containerObject;
                }
                else
                {

                    for (int i = 0; i < elements.Length - 1; ++i)
                    {
                        var containerType = containerObject.GetType();
                        var element = elements[i];
                        var indexOfBracket = element.IndexOf("[");

                        if (indexOfBracket >= 0)
                        {
                            var elementName = element[..indexOfBracket];
                            var index = Convert.ToInt32(element[indexOfBracket..].Replace("[", "").Replace("]", ""));

                            fieldInfo = containerType.getField(elementName, NUtils.getAllBindingFlags());
                            containerObject = (fieldInfo.GetValue(containerObject) as IList)[index];
                        }
                        else
                        {
                            fieldInfo = containerType.getField(element, NUtils.getAllBindingFlags());
                            containerObject = fieldInfo.GetValue(containerObject);
                        }
                    }

                    return containerObject;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return default;
            }
        }
        public static void clearLog()
        {
            var assembly = Assembly.GetAssembly(typeof(Editor));
            var type = assembly.GetType("UnityEditor.LogEntries");
            var method = type.GetMethod("Clear");
            method.Invoke(new object(), null);
        }
    }
}
