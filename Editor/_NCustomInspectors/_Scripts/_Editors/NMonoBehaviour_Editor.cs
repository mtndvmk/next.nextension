using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Nextension.NEditor
{
    [CustomEditor(typeof(NMonoBehaviour), true), CanEditMultipleObjects]
    public class NMonoBehaviour_Editor : Editor
    {
        private List<MemberInfo> _memberInfos;
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginDisabledGroup(true);
            var property = serializedObject.FindProperty("m_Script");
            EditorGUILayout.PropertyField(property, true);
            EditorGUI.EndDisabledGroup();

            __initFieldInfos();
            foreach (var memberInfo in _memberInfos)
            {
                if (memberInfo.MemberType == MemberTypes.Field)
                {
                    property = serializedObject.FindProperty(memberInfo.Name);
                    if (property != null)
                    {
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(property, true);
                        EditorGUI.EndChangeCheck();
                    }
                    else
                    {
                        var fieldInfo = memberInfo as FieldInfo;
                        if (fieldInfo.GetCustomAttribute<HideInInspector>() != null)
                        {
                            continue;
                        }
                        var value = fieldInfo.GetValue(target);
                        var name = memberInfo.Name;
                        if (name.StartsWith("<") && name.EndsWith(">k__BackingField"))
                        {
                            name = name[1..name.IndexOf(">")];
                        }
                        NEditorGUILayout.drawReadOnlyField(name, value);
                    }
                }
                //else if (memberInfo.MemberType == MemberTypes.Property)
                //{
                //    var propertyInfo = memberInfo as PropertyInfo;
                //    var value = propertyInfo.GetValue(target);
                //    NEditorGUILayout.drawField(memberInfo.Name, propertyInfo.PropertyType, , value);
                //}
                //else if (memberInfo.MemberType == MemberTypes.Method)
                //{
                //    var methodInfo = memberInfo as MethodInfo;
                //    var value = methodInfo.Invoke(target, null);
                //    NEditorGUILayout.DrawPropertyField(value);
                //}
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void __initFieldInfos()
        {
            if (_memberInfos == null)
            {
                var memberInfos = new List<MemberInfo>();
                NUtils.getMembers(target.GetType(), ref memberInfos, BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Instance |
                    BindingFlags.Static |
                    BindingFlags.DeclaredOnly, typeof(NMonoBehaviour), false);
                _memberInfos = new List<MemberInfo>();
                for (int i = 0; i < memberInfos.Count; i++)
                {
                    var memberInfo = memberInfos[i];
                    if (memberInfo.MemberType == MemberTypes.Field)
                    {
                        var fieldInfo = memberInfo as FieldInfo;
                        if (fieldInfo.IsNotSerialized)
                        {
                            continue;
                        }
                        _memberInfos.Add(memberInfo);
                    }
                    //else if (memberInfo.MemberType == MemberTypes.Property)
                    //{
                    //    if (memberInfo.GetCustomAttribute<NInspectorViewableAttribute>() == null)
                    //    {
                    //        continue;
                    //    }
                    //    var propertyInfo = memberInfo as PropertyInfo;
                    //    if (!propertyInfo.CanRead)
                    //    {
                    //        continue;
                    //    }
                    //    if (!NEditorGUILayout.isSupportedType(propertyInfo.PropertyType))
                    //    {
                    //        continue;
                    //    }
                    //    _memberInfos.Add(memberInfo);
                    //}
                    //else if (memberInfo.MemberType == MemberTypes.Method)
                    //{
                    //    var methodInfo = memberInfo as MethodInfo;
                    //    if (methodInfo.GetParameters().Length > 0)
                    //    {
                    //        continue;
                    //    }
                    //    if (!NEditorGUILayout.isSupportedType(methodInfo.ReturnType))
                    //    {
                    //        continue;
                    //    }
                    //    if (methodInfo.GetCustomAttribute<NInspectorViewableAttribute>() == null)
                    //    {
                    //        continue;
                    //    }
                    //    _memberInfos.Add(memberInfo);
                    //}
                }
            }
        }
    }
}
