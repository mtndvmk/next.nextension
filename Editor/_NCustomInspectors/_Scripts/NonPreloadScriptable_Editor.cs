using System;
using UnityEditor;
using UnityEngine;

namespace Nextension.NEditor
{
    [CustomPropertyDrawer(typeof(NonPreloadScriptable))]
    public class NonPreloadScriptable_Editor : PropertyDrawer
    {
        private ScriptableObject _scriptableObject;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (_scriptableObject == null)
            {
                _scriptableObject = NEditorHelper.getValue<NonPreloadScriptable>(property).getScriptableObject();
            }
            try
            {
                EditorGUI.PropertyField(position, SerializedPropertyUtil.getSerializedProperty(_scriptableObject));
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                base.OnGUI(position, property, label);
            }
        }
        ~NonPreloadScriptable_Editor()
        {
            if (_scriptableObject != null)
            {
                SerializedPropertyUtil.release(_scriptableObject);
                _scriptableObject = null;
            }
        }
    }
}
