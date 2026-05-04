using System;
using UnityEditor;
using UnityEngine;

namespace Nextension.NEditor
{
    [CustomPropertyDrawer(typeof(NReadOnlyAttribute))]
    public class NReadOnlyAttributeDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return CustomPropertyDrawerCache.forceGetPropertyHeight(property, label);
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (isReadOnly(property))
            {
                EditorGUI.BeginDisabledGroup(true);
                CustomPropertyDrawerCache.forceDraw(position, property, label);
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                CustomPropertyDrawerCache.forceDraw(position, property, label);
            }
        }

        private bool isReadOnly(SerializedProperty property)
        {
            try
            {
                var containerObj = NEditorHelper.getContainerObject(property);
                if (containerObj != null)
                {
                    if (attribute is NReadOnlyAttribute nReadOnly)
                    {
                        return nReadOnly.check(containerObj);
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