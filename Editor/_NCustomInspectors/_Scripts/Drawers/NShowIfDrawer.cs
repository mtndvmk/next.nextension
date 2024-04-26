using System;
using UnityEditor;
using UnityEngine;

namespace Nextension.NEditor
{
    [CustomPropertyDrawer(typeof(NShowIfAttribute))]
    public class NShowIfDrawer : PropertyDrawer
    {
        private bool _isShow;
       
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            _isShow = isShow(property);
            if (_isShow)
            {
                var height = CustomPropertyDrawerCache.getPropertyHeight(property, label);
                if (height.HasValue) return height.Value;
                else return EditorGUI.GetPropertyHeight(property, label, true);
            }
            else
            {
                return 0;
            }
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            if (_isShow)
            {
                if (!CustomPropertyDrawerCache.draw(position, property, label))
                {
                    EditorGUI.PropertyField(position, property, label);
                }
            }
        }
        private bool isShow(SerializedProperty property)
        {
            try
            {
                var parentValue = NEditorHelper.getParentValue(property);
                if (parentValue != null)
                {
                    var parentType = parentValue.GetType();
                    var predicateName = ((NShowIfAttribute)attribute).predicateName;
                    var methodInfo = parentType.GetMethod(predicateName, NUtils.getAllBindingFlags());
                    if (methodInfo != null)
                    {
                        bool result;
                        if (methodInfo.IsStatic)
                        {
                            result = (bool)methodInfo.Invoke(null, null);
                        }
                        else
                        {
                            result = (bool)methodInfo.Invoke(parentValue, null);
                        }
                        return result;
                    }
                    else
                    {
                        Debug.LogError($"Can't found method {predicateName} in {parentType}");
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            return true;
        }
    }
}