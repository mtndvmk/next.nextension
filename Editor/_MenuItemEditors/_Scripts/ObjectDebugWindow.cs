using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Nextension.NEditor
{
    public class ComponentDebugWindow : EditorWindow
    {
        private Object target;
        private List<FieldInfo> _fieldInfos;
        private List<MemberInfo> _memberInfos;

        private int _version;

        private int _selectedMethodIndex = 0;
        private string[] _methodNames;

        private string _searchMethodString;
        private string[] _filteredMethodNames = Array.Empty<string>();
        private int[] _filteredMethodIndices;

        private int _executedIndex = -1;
        private string _errorText;
        private object _result;
        private Vector2 _scrollPos;

        private MethodInfo _currentMethodInfo;
        private object[] _paramValues;

        public static void show(Object target)
        {
            var window = ComponentDebugWindow.GetWindow(typeof(ComponentDebugWindow)) as ComponentDebugWindow;
            window.titleContent = new GUIContent(target.name + " (Debug)");
            window.target = target;
            window._scrollPos = Vector2.zero;
            window._version = 0;
            window.__initFieldInfos();
        }

        private void OnGUI()
        {
            if (target == null || _fieldInfos == null || _memberInfos == null)
            {
                Close();
                return;
            }

            NEditorGUILayout.drawReadOnlyField("Target", target);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            foreach (var fieldInfo in _fieldInfos)
            {
                if (fieldInfo.GetCustomAttribute<HideInInspector>() != null)
                {
                    continue;
                }
                var value = fieldInfo.GetValue(target);
                var name = fieldInfo.Name;
                if (name.StartsWith("<") && name.EndsWith(">k__BackingField"))
                {
                    name = name[1..name.IndexOf(">")];
                }
                NEditorGUILayout.drawReadOnlyField(name, value);
            }

            if (_methodNames.Length > 0)
            {
                var tempSearchMethodString = EditorGUILayout.TextField("Search method", _searchMethodString);
                if (_version == 0 || tempSearchMethodString != _searchMethodString)
                {
                    _version++;
                    _searchMethodString = tempSearchMethodString;
                    if (_searchMethodString.isNullOrEmpty())
                    {
                        _filteredMethodNames = _methodNames;
                        _filteredMethodIndices = null;
                    }
                    else
                    {
                        var lowerCase = _searchMethodString.ToLower();
                        var nameList = new List<string>();
                        var indexList = new List<int>();
                        for (int i = 0; i < _methodNames.Length; i++)
                        {
                            var methodName = _methodNames[i];
                            if (methodName.ToLower().Contains(lowerCase))
                            {
                                nameList.Add(methodName);
                                indexList.Add(i);
                            }
                        }
                        _filteredMethodNames = nameList.ToArray();
                        _filteredMethodIndices = indexList.ToArray();
                    }
                }

                var runButtonSize = 70f;
                var methodRect = EditorGUILayout.GetControlRect();
                methodRect.xMax -= runButtonSize;

                if (_filteredMethodNames.Length > 0)
                {
                    _selectedMethodIndex = EditorGUI.Popup(methodRect, "Method: ", _selectedMethodIndex, _filteredMethodNames);
                    if (_selectedMethodIndex >= 0)
                    {
                        var buttonR = methodRect;
                        buttonR.xMin = methodRect.xMax;
                        buttonR.width = runButtonSize;
                        var targetMethodIndex = _filteredMethodIndices == null ? _selectedMethodIndex : _filteredMethodIndices[_selectedMethodIndex];
                        if (GUI.Button(buttonR, "Execute"))
                        {
                            _executedIndex = targetMethodIndex;
                            if (_executedIndex < _memberInfos.Count)
                            {
                                try
                                {
                                    var memberInfo = _memberInfos[_executedIndex];
                                    if (memberInfo.MemberType == MemberTypes.Property)
                                    {
                                        var propertyInfo = memberInfo as PropertyInfo;
                                        _result = propertyInfo.GetValue(target);
                                    }
                                    else
                                    {
                                        var methodInfo = memberInfo as MethodInfo;
                                        _result = methodInfo.Invoke(target, _paramValues);
                                    }
                                    _errorText = "";
                                }
                                catch (Exception e)
                                {
                                    _errorText = e.ToString();
                                    Debug.LogException(e);
                                }
                            }
                        }

                        if (targetMethodIndex >= 0 && targetMethodIndex < _memberInfos.Count)
                        {
                            var memberInfo = _memberInfos[targetMethodIndex];
                            if (memberInfo.MemberType == MemberTypes.Method)
                            {
                                var methodInfo = memberInfo as MethodInfo;

                                var parameters = methodInfo.GetParameters();

                                if (_currentMethodInfo != methodInfo)
                                {
                                    _currentMethodInfo = methodInfo;
                                    _paramValues = new object[parameters.Length];
                                    for (int i = 0; i < _paramValues.Length; i++)
                                    {
                                        _paramValues[i] = parameters[i].ParameterType.getDefault();
                                    }
                                }

                                for (int i = 0; i < parameters.Length; i++)
                                {
                                    var p = parameters[i];
                                    _paramValues[i] = NEditorGUILayout.drawFieldAndGetValue(p.Name, _paramValues[i], p.ParameterType);
                                }
                            }
                        }
                    }
                }
            }

            if (_executedIndex >= 0)
            {
                EditorGUILayout.Space();
                var methodName = _methodNames[_executedIndex];
                if (!_errorText.isNullOrEmpty())
                {
                    EditorGUILayout.HelpBox(methodName, MessageType.Error);
                    EditorGUILayout.Space();
                    var content = new GUIContent(_errorText);
                    var guiSkin = new GUIStyle(GUI.skin.label)
                    {
                        wordWrap = true,
                    };
                    var height = guiSkin.CalcHeight(content, position.width) + EditorGUIUtility.singleLineHeight;
                    EditorGUILayout.SelectableLabel(_errorText, guiSkin, GUILayout.Height(height));
                }
                else
                {
                    EditorGUILayout.HelpBox(methodName, MessageType.Info);
                    if (_result != null)
                    {
                        EditorGUILayout.Space();
                        NEditorGUILayout.drawReadOnlyField("Result: ", _result);
                    }
                }
            }

            EditorGUILayout.Space(100);
            EditorGUILayout.EndScrollView();
        }
        private void __initFieldInfos()
        {
            var memberInfos = new List<MemberInfo>();
            if (target is MonoBehaviour)
            {
                NUtils.getMembers(target.GetType(), ref memberInfos, BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance |
                BindingFlags.Static |
                BindingFlags.DeclaredOnly, typeof(MonoBehaviour), false);
            }
            else
            {
                NUtils.getMembers(target.GetType(), ref memberInfos, BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance |
                BindingFlags.Static |
                BindingFlags.DeclaredOnly, target.GetType(), true);
            }
            _fieldInfos = new List<FieldInfo>();
            _memberInfos = new List<MemberInfo>();
            for (int i = memberInfos.Count - 1; i >= 0; i--)
            {
                var memberInfo = memberInfos[i];
                if (memberInfo.MemberType == MemberTypes.Field)
                {
                    var fieldInfo = memberInfo as FieldInfo;
                    _fieldInfos.Add(fieldInfo);
                }
                else if (memberInfo.MemberType == MemberTypes.Method)
                {
                    var methodInfo = memberInfo as MethodInfo;

                    var methodName = memberInfo.Name;
                    if (methodName.StartsWith("get_")) continue;
                    if (methodName.StartsWith("set_")) continue;

                    bool isValid = true;

                    foreach (var p in methodInfo.GetParameters())
                    {
                        if (!NEditorGUILayout.canDrawFieldAndGetValue(p.ParameterType))
                        {
                            isValid = false;
                            break;
                        }
                    }
                    if (isValid)
                    {
                        _memberInfos.Add(methodInfo);
                    }
                }
                //else if (memberInfo.MemberType == MemberTypes.Property)
                //{
                //    var propertyInfo = memberInfo as PropertyInfo;
                //    if (propertyInfo.CanRead)
                //    {
                //        _memberInfos.Add(propertyInfo);
                //    }
                //}
            }

            _memberInfos.Reverse();
            _methodNames = new string[_memberInfos.Count];
            for (int i = _memberInfos.Count - 1; i >= 0; i--)
            {
                var memberInfo = _memberInfos[i];
                if (memberInfo.MemberType == MemberTypes.Property)
                {
                    var propertyInfo = memberInfo as PropertyInfo;
                    _methodNames[i] = $"{propertyInfo.Name} -> ({propertyInfo.PropertyType})";
                    continue;
                }
                else
                {
                    var methodInfo = _memberInfos[i] as MethodInfo;
                    var methodName = methodInfo.Name + "(";

                    var parameters = methodInfo.GetParameters();

                    foreach (var p in parameters)
                    {
                        methodName += $" {p.ParameterType},";
                    }

                    if (parameters.Length > 0)
                    {
                        methodName = methodName.Remove(methodName.Length - 1);
                    }

                    methodName += ")";

                    _methodNames[i] = $"{methodName} -> ({methodInfo.ReturnType})";
                }
            }
        }
    }
}
