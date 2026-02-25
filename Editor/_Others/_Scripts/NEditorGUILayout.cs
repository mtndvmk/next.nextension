using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Nextension.NEditor
{
    public static class NEditorGUILayout
    {
        private static HashSet<Type> _drawableTypes = new HashSet<Type>()
        {
            typeof(Vector2),
            typeof(Vector3),
            typeof(Vector4),
            typeof(float2),
            typeof(float3),
            typeof(float4),
            typeof(AnimationCurve),
            typeof(Gradient),
            typeof(Color),
            typeof(Quaternion),
            typeof(quaternion),
            typeof(string),
        };

        private static HashSet<Type> _toStringTypes = new HashSet<Type>()
        {
            typeof(System.Numerics.BigInteger),
            typeof(Guid),
            typeof(decimal),
            typeof(DateTime),
            typeof(DateTimeOffset),
        };

        private static Dictionary<Type, bool> _supportedTypeCache = new();

        private static HashSet<string> _foldouts = new();

        private static bool __checkDrawableType(Type type)
        {
            if (_drawableTypes.Contains(type))
            {
                return true;
            }

            if (_toStringTypes.Contains(type))
            {
                return true;
            }

            if (type.isInherited(typeof(UnityEngine.Object)))
            {
                return true;
            }

            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                if (isSupportedType(elementType))
                {
                    return true;
                }
            }

            if (type.isInherited(typeof(System.Collections.IEnumerable)))
            {
                Type listOfType;
                if (type.IsGenericType)
                {
                    listOfType = type.GetGenericArguments()[0];
                }
                else
                {
                    listOfType = typeof(object);
                }
                if (isSupportedType(listOfType))
                {
                    return true;
                }
            }

            var fieldInfos = NUtils.getFields(type, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var fieldInfo in fieldInfos)
            {
                if (isSupportedType(fieldInfo.FieldType))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool isSupportedType(Type type)
        {
            if (type == null)
            {
                return false;
            }
            if (type.IsEnum)
            {
                return true;
            }

            if (type.IsPrimitive)
            {
                return true;
            }

            if (_supportedTypeCache.TryGetValue(type, out var isSupported))
            {
                return isSupported;
            }

            var drawable = __checkDrawableType(type);
            _supportedTypeCache.Add(type, drawable);
            return drawable;
        }
        public static void drawReadOnlyField(string name, object value, string uniqueId = null)
        {
            if (value == null)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.LabelField(name, "null");
                EditorGUI.EndDisabledGroup();
                return;
            }

            var type = value.GetType();

            if (!isSupportedType(type))
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.LabelField(name, "Not supported");
                EditorGUI.EndDisabledGroup();
                return;
            }

            uniqueId ??= type.FullName + ".." + name;

            if (type == typeof(int) || type == typeof(byte) || type == typeof(sbyte) || type == typeof(short) || type == typeof(ushort))
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.IntField(name, Convert.ToInt32(value));
                EditorGUI.EndDisabledGroup();
            }
            else if (type == typeof(float))
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.FloatField(name, (float)value);
                EditorGUI.EndDisabledGroup();
            }
            else if (type == typeof(bool))
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.Toggle(name, (bool)value);
                EditorGUI.EndDisabledGroup();
            }
            else if (type == typeof(long) || type == typeof(uint))
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.LongField(name, Convert.ToInt64(value));
                EditorGUI.EndDisabledGroup();
            }
            else if (type == typeof(double))
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.DoubleField(name, (long)value);
                EditorGUI.EndDisabledGroup();
            }
            else if (type == typeof(Vector2))
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.Vector2Field(name, (Vector2)value);
                EditorGUI.EndDisabledGroup();
            }
            else if (type == typeof(Vector3))
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.Vector3Field(name, (Vector3)value);
                EditorGUI.EndDisabledGroup();
            }
            else if (type == typeof(Vector4))
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.Vector4Field(name, (Vector4)value);
                EditorGUI.EndDisabledGroup();
            }
            else if (type == typeof(Color))
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ColorField(name, (Color)value);
                EditorGUI.EndDisabledGroup();
            }
            else if (type == typeof(Quaternion))
            {
                EditorGUI.BeginDisabledGroup(true);
                var quat = (Quaternion)value;
                EditorGUILayout.Vector4Field(name, new Vector4(quat.x, quat.y, quat.z, quat.w));
                EditorGUI.EndDisabledGroup();
            }
            else if (type == typeof(float2))
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.Vector2Field(name, (float2)value);
                EditorGUI.EndDisabledGroup();
            }
            else if (type == typeof(float3))
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.Vector3Field(name, (float3)value);
                EditorGUI.EndDisabledGroup();
            }
            else if (type == typeof(float4))
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.Vector4Field(name, (float4)value);
                EditorGUI.EndDisabledGroup();
            }
            else if (type == typeof(quaternion))
            {
                EditorGUI.BeginDisabledGroup(true);
                var quat = (quaternion)value;
                EditorGUILayout.Vector4Field(name, new Vector4(quat.value.x, quat.value.y, quat.value.z, quat.value.w));
                EditorGUI.EndDisabledGroup();
            }
            else if (type.IsEnum)
            {
                EditorGUI.BeginDisabledGroup(true);
                if (type.GetCustomAttribute<FlagsAttribute>() != null)
                {
                    EditorGUILayout.EnumFlagsField(name, (Enum)value);
                }
                else
                {
                    EditorGUILayout.EnumPopup(name, (Enum)value);
                }
                EditorGUI.EndDisabledGroup();
            }
            else if (type == typeof(string))
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextField(name, value.ToString());
                EditorGUI.EndDisabledGroup();
            }
            else if (_toStringTypes.Contains(type) || type.IsPrimitive)
            {
                EditorGUILayout.LabelField(name, $"{value}\t({type})");
            }
            else if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ObjectField(name, (UnityEngine.Object)value, type, true);
                EditorGUI.EndDisabledGroup();
            }
            else if (type.IsArray)
            {
                __drawArray(name, type, value, uniqueId);
            }
            else if (type.isInherited(typeof(System.Collections.IList)))
            {
                __drawList(name, type, value, uniqueId);
            }
            else if (type.isInherited(typeof(System.Collections.IDictionary)))
            {
                __drawDictionary(name, type, value, uniqueId);
            }
            else if (type.isInherited(typeof(System.Collections.IEnumerable)))
            {
                __drawIEnumerable(name, type, value, uniqueId);
            }
            else if (type.isInherited(typeof(Gradient)))
            {
                var grad = new Gradient();
                var currentGrad = (Gradient)value;
                grad.SetKeys(currentGrad.colorKeys, currentGrad.alphaKeys);
                EditorGUILayout.GradientField(new GUIContent(name), grad);
            }
            else if (type == typeof(AnimationCurve))
            {
                var curve = new AnimationCurve();
                curve.CopyFrom((AnimationCurve)value);
                EditorGUILayout.CurveField(new GUIContent(name), curve);
            }
            else
            {
                __drawCustomType(name, type, value, uniqueId);
            }
        }

        public static bool canDrawFieldAndGetValue(Type type)
        {
            if (type == typeof(int) || type == typeof(byte) || type == typeof(sbyte) || type == typeof(short) || type == typeof(ushort))
            {
                return true;
            }
            else if (type == typeof(float))
            {
                return true;
            }
            else if (type == typeof(bool))
            {
                return true;
            }
            else if (type == typeof(long) || type == typeof(uint))
            {
                return true;
            }
            else if (type == typeof(double))
            {
                return true;
            }
            else if (type == typeof(Vector2))
            {
                return true;
            }
            else if (type == typeof(Vector3))
            {
                return true;
            }
            else if (type == typeof(Vector4))
            {
                return true;
            }
            else if (type == typeof(Color))
            {
                return true;
            }
            else if (type == typeof(Quaternion))
            {
                return true;
            }
            else if (type == typeof(float2))
            {
                return true;
            }
            else if (type == typeof(float3))
            {
                return true;
            }
            else if (type == typeof(float4))
            {
                return true;
            }
            else if (type == typeof(quaternion))
            {
                return true;
            }
            else if (type.IsEnum)
            {
                return true;
            }
            else if (type == typeof(string))
            {
                return true;
            }
            else if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                return true;
            }
            else if (type.isInherited(typeof(Gradient)))
            {
                return true;
            }
            else if (type == typeof(AnimationCurve))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static object drawFieldAndGetValue(string name, object value, Type type, string uniqueId = null)
        {
            if (type == typeof(int) || type == typeof(byte) || type == typeof(sbyte) || type == typeof(short) || type == typeof(ushort))
            {
                return EditorGUILayout.IntField(name, Convert.ToInt32(value));
            }
            else if (type == typeof(float))
            {
                return EditorGUILayout.FloatField(name, (float)value);
            }
            else if (type == typeof(bool))
            {
                return EditorGUILayout.Toggle(name, (bool)value);
            }
            else if (type == typeof(long) || type == typeof(uint))
            {
                return EditorGUILayout.LongField(name, Convert.ToInt64(value));
            }
            else if (type == typeof(double))
            {
                return EditorGUILayout.DoubleField(name, (long)value);
            }
            else if (type == typeof(Vector2))
            {
                return EditorGUILayout.Vector2Field(name, (Vector2)value);
            }
            else if (type == typeof(Vector3))
            {
                return EditorGUILayout.Vector3Field(name, (Vector3)value);
            }
            else if (type == typeof(Vector4))
            {
                return EditorGUILayout.Vector4Field(name, (Vector4)value);
            }
            else if (type == typeof(Color))
            {
                return EditorGUILayout.ColorField(name, (Color)value);
            }
            else if (type == typeof(Quaternion))
            {
                var quat = (Quaternion)value;
                return EditorGUILayout.Vector4Field(name, new Vector4(quat.x, quat.y, quat.z, quat.w)).toQuaternion();
            }
            else if (type == typeof(float2))
            {
                return (float2)EditorGUILayout.Vector2Field(name, (float2)value);
            }
            else if (type == typeof(float3))
            {
                return (float3)EditorGUILayout.Vector3Field(name, (float3)value);
            }
            else if (type == typeof(float4))
            {
                return (float4)EditorGUILayout.Vector4Field(name, (float4)value);
            }
            else if (type == typeof(quaternion))
            {
                var quat = (quaternion)value;
                return (quaternion)EditorGUILayout.Vector4Field(name, new Vector4(quat.value.x, quat.value.y, quat.value.z, quat.value.w)).toQuaternion();
            }
            else if (type.IsEnum)
            {
                if (type.GetCustomAttribute<FlagsAttribute>() != null)
                {
                    return EditorGUILayout.EnumFlagsField(name, (Enum)value);
                }
                else
                {
                    return EditorGUILayout.EnumPopup(name, (Enum)value);
                }
            }
            else if (type == typeof(string))
            {
                if (value == null) value = "";
                return EditorGUILayout.TextField(name, value?.ToString());
            }
            else if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                return EditorGUILayout.ObjectField(name, (UnityEngine.Object)value, type, true);
            }
            else if (type.isInherited(typeof(Gradient)))
            {
                var grad = new Gradient();
                var currentGrad = (Gradient)value;
                grad.SetKeys(currentGrad.colorKeys, currentGrad.alphaKeys);
                return EditorGUILayout.GradientField(new GUIContent(name), grad);
            }
            else if (type == typeof(AnimationCurve))
            {
                var curve = new AnimationCurve();
                curve.CopyFrom((AnimationCurve)value);
                return EditorGUILayout.CurveField(new GUIContent(name), curve);
            }
            else
            {
                throw new NotSupportedException($"Type {type} is not supported in drawFieldAndGetValue");
            }
        }

        private static bool __drawGroupHeader(string groupName, string uniqueId)
        {
            bool isFoldout = _foldouts.Contains(uniqueId);
            var content = new GUIContent(groupName);
            var style = EditorStyles.foldoutHeader;
            Rect rect = GUILayoutUtility.GetRect(content, style);
            rect.xMin += (EditorGUI.indentLevel - 1) * 15f;
            isFoldout = EditorGUI.BeginFoldoutHeaderGroup(rect, isFoldout, content, style);
            EditorGUI.EndFoldoutHeaderGroup();
            if (isFoldout)
            {
                _foldouts.Add(uniqueId);
            }
            else
            {
                _foldouts.Remove(uniqueId);
            }
            return isFoldout;
        }
        private static void __drawArray(string name, Type type, object value, string uniqueId)
        {
            var array = value as Array;

            EditorGUI.indentLevel++;

            var isFoldout = __drawGroupHeader(name + " {Length: " + array.Length + "}", uniqueId);
            if (isFoldout)
            {
                for (int i = 0; i < array.Length; ++i)
                {
                    var elementValue = array.GetValue(i);
                    var uniqueId2 = uniqueId + "[" + i + "]";
                    drawReadOnlyField("Element " + i, elementValue, uniqueId2);
                }
            }
            EditorGUI.indentLevel--;
        }
        private static void __drawList(string name, Type type, object value, string uniqueId)
        {
            var list = value as System.Collections.IList;
            EditorGUI.indentLevel++;
            var isFoldout = __drawGroupHeader(name + " {Count: " + list.Count + "}", uniqueId);
            if (isFoldout)
            {
                for (int i = 0; i < list.Count; ++i)
                {
                    var elementValue = list[i];
                    var uniqueId2 = uniqueId + "[" + i + "]";
                    drawReadOnlyField("Element " + i, elementValue, uniqueId2);
                }
            }
            EditorGUI.indentLevel--;
        }
        private static void __drawDictionary(string name, Type type, object value, string uniqueId)
        {
            var dict = value as System.Collections.IDictionary;
            EditorGUI.indentLevel++;
            var isFoldout = __drawGroupHeader(name + " {Count: " + dict.Count + "}", uniqueId);
            if (isFoldout)
            {
                if (dict.Count > 0)
                {
                    EditorGUI.indentLevel++;
                    foreach (var elementKey in dict.Keys)
                    {
                        var elementValue = dict[elementKey];
                        var uniqueId2 = uniqueId + "{Key:" + elementKey + "}";
                        isFoldout = __drawGroupHeader($"[{elementKey}]", uniqueId2);
                        if (isFoldout)
                        {
                            drawReadOnlyField("Key", elementKey, uniqueId2 + ".key");
                            drawReadOnlyField("Value", elementValue, uniqueId2 + ".value");
                        }
                    }
                    EditorGUI.indentLevel--;
                }
            }
            EditorGUI.indentLevel--;
        }
        private static void __drawIEnumerable(string name, Type type, object value, string uniqueId)
        {
            EditorGUI.indentLevel++;
            var enumerable = value as System.Collections.IEnumerable;

            var countProperty = type.GetRuntimeProperty("Count");
            if (countProperty != null)
            {
                var count = (int)countProperty.GetValue(value);
                name += " {Count: " + count + "}";
            }

            var isFoldout = __drawGroupHeader(name, uniqueId);
            if (isFoldout)
            {
                int index = 0;
                foreach (var elementValue in enumerable)
                {
                    var uniqueId2 = uniqueId + "[" + index + "]";
                    drawReadOnlyField("Element " + index, elementValue, uniqueId2);
                    index++;
                }
            }
            EditorGUI.indentLevel--;
        }
        private static void __drawCustomType(string name, Type type, object value, string uniqueId)
        {
            var fieldInfos = NUtils.getFields(type, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfos.Count > 0)
            {
                EditorGUI.indentLevel++;
                var isFoldout = __drawGroupHeader(name, uniqueId);
                if (isFoldout)
                {
                    foreach (var fieldInfo in fieldInfos)
                    {
                        if (fieldInfo.GetCustomAttribute<HideInInspector>() != null)
                        {
                            continue;
                        }
                        var uniqueId2 = uniqueId + "." + fieldInfo.Name;
                        var fieldValue = fieldInfo.GetValue(value);
                        drawReadOnlyField(fieldInfo.Name, fieldValue, uniqueId2);
                    }
                }
                EditorGUI.indentLevel--;
            }
        }
    }
}
