using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Nextension.NEditor
{
    public static class CustomPropertyDrawerCache
    {
        private static Dictionary<Type, Cache> _caches = new();

        private static Cache getCache(Type type)
        {
            if (_caches.TryGetValue(type, out var cache)) return cache;
            var drawType = getDrawType(type);
            if (drawType == null)
            {
                _caches.Add(type, null);
                return null;
            }
            else
            {
                cache = new Cache(drawType);
                _caches.Add(type, cache);
                return cache;
            }
        }
        private static Type getDrawType(Type propertyType)
        {
            var types = NUtils.getCustomTypes();
            foreach (var type in types)
            {
                var customPropertyDrawerAttr = type.GetCustomAttribute<CustomPropertyDrawer>();
                if (customPropertyDrawerAttr != null)
                {
                    var customPropertyDrawerAttrType = customPropertyDrawerAttr.GetType();
                    var targetTypeOfDrawer = (Type)customPropertyDrawerAttrType.GetField("m_Type", NUtils.getAllBindingFlags()).GetValue(customPropertyDrawerAttr);
                    var useForChildren = (bool)customPropertyDrawerAttrType.GetField("m_UseForChildren", NUtils.getAllBindingFlags()).GetValue(customPropertyDrawerAttr);

                    bool isValid;
                    if (useForChildren)
                    {
                        isValid = NUtils.isInherited(propertyType, targetTypeOfDrawer);
                    }
                    else
                    {
                        isValid = propertyType == targetTypeOfDrawer;
                    }
                    if (isValid)
                    {
                        return type;
                    }
                }
            }
            return null;
        }

        public static bool draw(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property == null || property.boxedValue == null) return false;
            var boxedType = property.boxedValue.GetType();
            var cache = getCache(boxedType);
            if (cache != null)
            {
                cache.drawerFuncCache(position, property, label);
                return true;
            }
            return false;
        }

        public static void forceDraw(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!draw(position, property, label))
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }

        public static float? getPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property == null || property.boxedValue == null) return default; 
            var boxedType = property.boxedValue.GetType();
            var cache = getCache(boxedType);
            if (cache != null)
            {
                return cache.getHeightFuncCache(property, label);
            }
            return default;
        }
        public static float forceGetPropertyHeight(SerializedProperty property, GUIContent label, bool includeChildren = true)
        {
            var height = getPropertyHeight(property, label);
            if (height.HasValue) return height.Value;
            else return EditorGUI.GetPropertyHeight(property, label, includeChildren);
        }

        private class Cache
        {
            public Action<Rect, SerializedProperty, GUIContent> drawerFuncCache;
            public Func<SerializedProperty, GUIContent, float> getHeightFuncCache;
            public Cache(Type type)
            {
                var method = type.GetMethod("OnGUI", NUtils.getAllBindingFlags());
                var instance = type.createInstance();
                drawerFuncCache = (Action<Rect, SerializedProperty, GUIContent>)method.CreateDelegate(typeof(Action<Rect, SerializedProperty, GUIContent>), instance);

                var method2 = type.GetMethod("GetPropertyHeight", NUtils.getAllBindingFlags());
                getHeightFuncCache = (Func<SerializedProperty, GUIContent, float>)method2.CreateDelegate(typeof(Func<SerializedProperty, GUIContent, float>), instance);
            }
        }
    }
}
