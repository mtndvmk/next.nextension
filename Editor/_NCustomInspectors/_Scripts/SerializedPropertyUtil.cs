using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Nextension.NEditor
{
    public static class SerializedPropertyUtil
    {
        private static Dictionary<int, SerializedScriptableObject> _serializedScriptableObjects = new();
        public static SerializedProperty getSerializedProperty(ScriptableObject scriptable)
        {
            if (scriptable == null)
            {
                return null;
            }
            var key = scriptable.GetHashCode();
            if (!_serializedScriptableObjects.TryGetValue(key, out var serializedScriptableObject))
            {
                serializedScriptableObject = SerializedScriptableObject.get(scriptable);
                _serializedScriptableObjects.Add(key, serializedScriptableObject);
            }
            return serializedScriptableObject.getSerializedProperty();
        }
        public static void release(ScriptableObject scriptable)
        {
            if (scriptable)
            {
                var key = scriptable.GetHashCode();
                if (_serializedScriptableObjects.TryGetValue(key, out var serializedScriptableObject))
                {
                    _serializedScriptableObjects.Remove(key);
                    Object.DestroyImmediate(serializedScriptableObject);
                }
            }
        }
        public class SerializedScriptableObject : ScriptableObject
        {
            public static SerializedScriptableObject get(ScriptableObject scriptableObject)
            {
                var serializedScriptableObject = ScriptableObject.CreateInstance<SerializedScriptableObject>();
                serializedScriptableObject.scriptableObject = scriptableObject;
                serializedScriptableObject.serializedObject = new SerializedObject(serializedScriptableObject);
                serializedScriptableObject.scriptableProperty = serializedScriptableObject.serializedObject.FindProperty(nameof(SerializedScriptableObject.scriptableObject));
                return serializedScriptableObject;
            }

            [SerializeField] private ScriptableObject scriptableObject;
            [NonSerialized] private SerializedObject serializedObject;
            [NonSerialized] private SerializedProperty scriptableProperty;

            public SerializedProperty getSerializedProperty()
            {
                return scriptableProperty;
            }
        }
    }
}
