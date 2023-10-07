using System;
using UnityEditor;
using UnityEngine;

namespace Nextension.NEditor
{
    [CustomPropertyDrawer(typeof(NonPreloadScriptable))]
    public class NonPreloadScriptable_Editor : PropertyDrawer
    {
        public class SerializedScriptableObject : ScriptableObject
        {
            public ScriptableObject scriptable;
            [NonSerialized] public SerializedObject serializedObject;
            [NonSerialized] public SerializedProperty scriptableProperty;
        }
        private SerializedScriptableObject _serializedScriptableObject;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            loadProperty(property);
            if (_serializedScriptableObject != null)
            {
                EditorGUI.PropertyField(position, _serializedScriptableObject.scriptableProperty);
                _serializedScriptableObject.serializedObject.Dispose();
                _serializedScriptableObject.scriptableProperty.Dispose();
                NUtils.destroy(_serializedScriptableObject);
                _serializedScriptableObject = null;
            }
            else
            {
                base.OnGUI(position, property, label);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label);
        }

        private void loadProperty(SerializedProperty property)
        {
            var scriptableObject = (NEditorHelper.getValue(property) as NonPreloadScriptable)?.getScriptableObject();
            if (scriptableObject)
            {
                _serializedScriptableObject = ScriptableObject.CreateInstance<SerializedScriptableObject>();
                _serializedScriptableObject.scriptable = scriptableObject;
                _serializedScriptableObject.serializedObject = new SerializedObject(_serializedScriptableObject);
                _serializedScriptableObject.scriptableProperty = _serializedScriptableObject.serializedObject.FindProperty(nameof(SerializedScriptableObject.scriptable));
            }
        }
    }
}
