using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Nextension.NEditor
{
    [CustomPropertyDrawer(typeof(NFoldableAttribute))]
    public class NFoldableDrawer : PropertyDrawer
    {
        private bool _isFoudout;

        private static HashSet<string> _supportTypes = new HashSet<string>()
        {
            "UnityEvent",
            "UnityEvent`1",
        };

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (_isFoudout)
            {
                return CustomPropertyDrawerCache.forceGetPropertyHeight(property, label);
            }
            return EditorGUIUtility.singleLineHeight;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!_supportTypes.Contains(property.type))
            {
                _isFoudout = true;
                CustomPropertyDrawerCache.forceDraw(position, property, label);
                return;
            }
            EditorGUI.BeginChangeCheck();
            position.xMin += EditorGUI.indentLevel * 15;
            if (_isFoudout)
            {
                var headerRect = new Rect(position.x, position.y, -4, EditorGUIUtility.singleLineHeight);
                _isFoudout = EditorGUI.BeginFoldoutHeaderGroup(headerRect, true, "");
                EditorGUI.EndFoldoutHeaderGroup();
                if (_isFoudout)
                {
                    CustomPropertyDrawerCache.forceDraw(position, property, label);
                }
            }
            else
            {
                _isFoudout = EditorGUI.BeginFoldoutHeaderGroup(position, false, label);
                EditorGUI.EndFoldoutHeaderGroup();
            }

            EditorGUI.EndChangeCheck();
        }
    }
}