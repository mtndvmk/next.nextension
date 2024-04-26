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
        public static T getAttribute<T>(SerializedProperty property) where T : System.Attribute
        {
            var fieldInfo = getFieldInfo(property);
            return fieldInfo.GetCustomAttribute<T>();
        }

        public static FieldInfo getFieldInfo(SerializedProperty property, out object qObject)
        {
            try
            {
                var path = property.propertyPath.Replace(".Array.data[", "[");
                var elements = path.Split('.');

                qObject = property.serializedObject.targetObject;
                FieldInfo fieldInfo = null;

                for (int i = 0; i < elements.Length; ++i)
                {
                    var element = elements[i];
                    if (element.Contains("["))
                    {
                        var elementName = element.Substring(0, element.IndexOf("["));
                        var index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));

                        var qType = qObject.GetType();
                        fieldInfo = qType.getField(elementName, NUtils.getAllBindingFlags());
                        qObject = (fieldInfo.GetValue(qObject) as IList)[index];
                    }
                    else
                    {
                        fieldInfo = qObject.GetType().getField(element, NUtils.getAllBindingFlags());
                        qObject = fieldInfo.GetValue(qObject);
                    }
                }

                return fieldInfo;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                qObject = default;
                return default;
            }
        }
        public static object getParentValue(SerializedProperty property)
        {
            try
            {
                var path = property.propertyPath.Replace(".Array.data[", "[");
                var elements = path.Split('.');
                FieldInfo fieldInfo = null;
                object qObject = property.serializedObject.targetObject;

                if (elements.Length == 1)
                {
                    return qObject;
                }
                else
                {

                    for (int i = 0; i < elements.Length - 1; ++i)
                    {
                        var element = elements[i];
                        if (element.Contains("["))
                        {
                            var elementName = element.Substring(0, element.IndexOf("["));
                            var index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));

                            var qType = qObject.GetType();
                            fieldInfo = qType.getField(elementName, NUtils.getAllBindingFlags());
                            qObject = (fieldInfo.GetValue(qObject) as IList)[index];
                        }
                        else
                        {
                            fieldInfo = qObject.GetType().getField(element, NUtils.getAllBindingFlags());
                            qObject = fieldInfo.GetValue(qObject);
                        }
                    }

                    return qObject;
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
