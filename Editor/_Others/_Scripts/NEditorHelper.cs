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

                (fieldInfo, valueOfProperty) = getFieldInfoAndValueFromPropertyPaths(valueOfProperty, elements, elements.Length);
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
                    (fieldInfo, containerObject) = getFieldInfoAndValueFromPropertyPaths(containerObject, elements, elements.Length - 1);
                    return containerObject;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return default;
            }
        }

        private static (FieldInfo, object) getFieldInfoAndValueFromPropertyPaths(object obj, string[] elements, int loopCount)
        {
            FieldInfo fieldInfo = null;
            var allBindingFlag = NUtils.getAllBindingFlags();
            for (int i = 0; i < loopCount; ++i)
            {
                var element = elements[i];
                var indexOfBracket = element.IndexOf("[");

                if (indexOfBracket >= 0)
                {
                    var elementName = element[..indexOfBracket];
                    var index = Convert.ToInt32(element[indexOfBracket..].Replace("[", "").Replace("]", ""));

                    fieldInfo = obj.GetType().getField(elementName, allBindingFlag);
                    obj = (fieldInfo.GetValue(obj) as IList)[index];
                }
                else
                {
                    fieldInfo = obj.GetType().getField(element, allBindingFlag);
                    obj = fieldInfo.GetValue(obj);
                }
            }
            return (fieldInfo, obj);
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
