using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Nextension.NEditor
{
    [CustomPropertyDrawer(typeof(NGroupAttribute))]
    public class NGroupDrawer : PropertyDrawer
    {
        private static object _currentObject;

        private static Dictionary<string, List<FieldInfo>> _groupTable = new Dictionary<string, List<FieldInfo>>();
        private static Dictionary<string, string> _fieldToGroupIds = new Dictionary<string, string>();
        private static HashSet<string> _foldoutGroups = new HashSet<string>();

        private void loadGroupTable(object currentObject)
        {
            if (_currentObject != currentObject)
            {
                _currentObject = currentObject;
                _groupTable.Clear();
                _fieldToGroupIds.Clear();
                var objType = currentObject.GetType();
                var validFieldInfos = new List<FieldInfo>();
                string currentGroupId = string.Empty;
                string currentGroupUid = string.Empty;
                var allFields = objType.GetRuntimeFields();
                foreach (var fieldInfo in allFields)
                {
                    if (fieldInfo.IsNotSerialized) continue;

                    var showIfAttribute = (NShowIfAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(NShowIfAttribute));
                    if (showIfAttribute != null && !showIfAttribute.check(currentObject))
                    {
                        continue;
                    }

                    var groupAttribute = (NGroupAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(NGroupAttribute));
                    if (groupAttribute != null)
                    {
                        var groupId = groupAttribute.groupId;
                        if (currentGroupId != groupId)
                        {
                            currentGroupId = groupId;

                            currentGroupUid = $"{groupId}.{fieldInfo.Name}.{currentObject}";

                            validFieldInfos = new List<FieldInfo>
                                {
                                    fieldInfo
                                };
                            _groupTable.Add(currentGroupUid, validFieldInfos);
                        }
                        else
                        {
                            validFieldInfos.Add(fieldInfo);
                        }
                        _fieldToGroupIds.Add(fieldInfo.Name, currentGroupUid);
                    }
                    else
                    {
                        currentGroupId = string.Empty;
                    }
                }
            }
        }

        private void checkAndResetTable(SerializedProperty property)
        {
            if (!_fieldToGroupIds.ContainsKey(property.name))
            {
                _currentObject = null;
            }
        }

        private string getFirstShownField(string groupId)
        {
            var objType = _currentObject.GetType();
            objType.GetFields(NUtils.getAllBindingFlags());
            var groupFields = _groupTable[groupId];
            for (int i = 0; i < groupFields.Count; i++)
            {
                var fieldInfo = groupFields[i];
                var showIfAttribute = (NShowIfAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(NShowIfAttribute));
                if (showIfAttribute == null || showIfAttribute.check(_currentObject)) return fieldInfo.Name;
            }
            return null;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var objectContainer = NEditorHelper.getContainerObject(property);

            checkAndResetTable(property);
            loadGroupTable(objectContainer);

            var groupId = _fieldToGroupIds[property.name];
            var groupFields = _groupTable[groupId];

            if (groupFields.Count == 0)
            {
                return CustomPropertyDrawerCache.forceGetPropertyHeight(property, label);
            }

            var firstFieldName = getFirstShownField(groupId);
            if (!_foldoutGroups.Contains(groupId))
            {
                if (firstFieldName != property.name)
                {
                    return 0;
                }
                return EditorGUIUtility.singleLineHeight;
            }
            if (firstFieldName != property.name)
            {
                return CustomPropertyDrawerCache.forceGetPropertyHeight(property, label);
            }
            return CustomPropertyDrawerCache.forceGetPropertyHeight(property, label) + EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            position.height = EditorGUIUtility.singleLineHeight;
            var objectContainer = NEditorHelper.getContainerObject(property);
            loadGroupTable(objectContainer);

            var groupId = _fieldToGroupIds[property.name];

            var groupFields = _groupTable[groupId];
            if (groupFields.Count == 0)
            {
                CustomPropertyDrawerCache.forceDraw(position, property, label);
            }
            else
            {
                var firstFieldName = getFirstShownField(groupId);
                if (firstFieldName != property.name)
                {
                    if (_foldoutGroups.Contains(groupId))
                    {
                        EditorGUI.indentLevel++;
                        CustomPropertyDrawerCache.forceDraw(position, property, label);
                        EditorGUI.indentLevel--;
                    }
                }
                else
                {
                    var name = groupId[..groupId.IndexOf('.')];
                    var isFoldout = _foldoutGroups.Contains(groupId);
                    isFoldout = EditorGUI.BeginFoldoutHeaderGroup(position, isFoldout, name);
                    EditorGUI.EndFoldoutHeaderGroup();
                    if (!isFoldout)
                    {
                        _foldoutGroups.Remove(groupId);
                    }
                    else
                    {
                        _foldoutGroups.Add(groupId);
                    }
                    if (_foldoutGroups.Contains(groupId))
                    {
                        EditorGUI.indentLevel++;
                        position.y += EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
                        CustomPropertyDrawerCache.forceDraw(position, property, label);
                        EditorGUI.indentLevel--;
                    }
                }
            }
            EditorGUI.EndChangeCheck();
        }
    }
}