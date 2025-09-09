using System;
using UnityEditor;
using UnityEngine;

namespace Nextension.NEditor
{
    [CustomPropertyDrawer(typeof(NShowIfAttribute), true)]
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
            if (_isShow)
            {
                EditorGUI.BeginChangeCheck();
                if (!CustomPropertyDrawerCache.draw(position, property, label))
                {
                    EditorGUI.PropertyField(position, property, label);
                }
                EditorGUI.EndChangeCheck();
            }
        }
        private bool isShow(SerializedProperty property)
        {
            try
            {
                var containerObj = NEditorHelper.getContainerObject(property);
                if (containerObj != null)
                {
                    if (attribute is NShowIfAttribute nShowIf)
                    {
                        return nShowIf.check(containerObj);
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