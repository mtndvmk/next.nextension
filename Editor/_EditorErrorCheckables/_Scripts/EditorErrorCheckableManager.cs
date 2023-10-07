using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Nextension.NEditor
{
    internal class EditorErrorCheckableManager
    {
        public class CustomOnPreprocessBuild : IPreprocessBuildWithReport
        {
            public int callbackOrder => int.MinValue;
            public void OnPreprocessBuild(BuildReport report)
            {
                try
                {
                    List<(MethodInfo, int)> methods = new List<(MethodInfo, int)>();
                    BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
                    foreach (var t in NUtils.getCustomTypes())
                    {
                        if (t.GetInterface(typeof(IErrorCheckable).Name) != null)
                        {
                            var method = t.GetMethod(nameof(IErrorCheckable.onPreprocessBuild), bindingFlags);
                            if (method != null)
                            {
                                if (method.GetParameters().Length > 0)
                                {
                                    continue;
                                }
                                var order = (int)(t.GetProperty(nameof(IErrorCheckable.Priority), bindingFlags)?.GetValue(null) ?? 0);
                                methods.Add((method, order));
                            }
                        }
                    }
                    methods.Sort((a, b) => b.Item2.CompareTo(a.Item2));
                    for (int i = 0; i < methods.Count; i++)
                    {
                        methods[i].Item1.Invoke(null, null);
                    }
                }
                catch (Exception e)
                {
                    throw new BuildFailedException(e);
                }
            }
        }
        public class OnLoadOrRecompiled
        {
            [InitializeOnLoadMethod]
            static void checkOnLoadOrRecompiled()
            {
                var types = NUtils.getCustomTypes();
                BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

                List<(MethodInfo, int)> methods = new();

                foreach (var t in types)
                {
                    if (t.GetInterface(typeof(IErrorCheckable).Name) != null)
                    {
                        var method = t.GetMethod(nameof(IErrorCheckable.onLoadOrRecompiled), bindingFlags);
                        if (method != null)
                        {
                            if (method.GetParameters().Length > 0)
                            {
                                continue;
                            }
                            var order = (int)(t.GetProperty(nameof(IErrorCheckable.Priority), bindingFlags)?.GetValue(null) ?? 0);
                            methods.Add((method, order));
                        }
                    }
                }

                methods.Sort((a, b) => b.Item2.CompareTo(a.Item2));
                for (int i = 0; i < methods.Count; i++)
                {
                    var method = methods[i].Item1;
                    try
                    {
                        methods[i].Item1.Invoke(null, null);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            }
        }
        public class OnEditorLoop
        {
            static Exception _exception;
            [InitializeOnLoadMethod]
            static void checkOnEditorLoop()
            {
                EditorApplication.playModeStateChanged -= editorApplication_playModeStateChanged;
                EditorApplication.playModeStateChanged += editorApplication_playModeStateChanged;
                var types = NUtils.getCustomTypes();
                BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

                List<(MethodInfo, int)> methods = new();

                foreach (var t in types)
                {
                    if (t.GetInterface(typeof(IErrorCheckable).Name) != null)
                    {
                        var method = t.GetMethod(nameof(IErrorCheckable.onEditorLoop), bindingFlags);
                        if (method != null)
                        {
                            if (method.GetParameters().Length > 0)
                            {
                                continue;
                            }
                            var order = (int)(t.GetProperty(nameof(IErrorCheckable.Priority), bindingFlags)?.GetValue(null) ?? 0);
                            methods.Add((method, order));
                        }
                    }
                }

                methods.Sort((a, b) => b.Item2.CompareTo(a.Item2));
                for (int i = 0; i < methods.Count; i++)
                {
                    var method = methods[i].Item1;
                    try
                    {
                        methods[i].Item1.Invoke(null, null);
                    }
                    catch (Exception ex)
                    {
                        _exception = ex;
                        Debug.LogException(_exception);
                        NAwaiterEditor.runDelay(1, checkOnEditorLoop);
                        return;
                    }
                }

                _exception = null;
            }
            private static void editorApplication_playModeStateChanged(PlayModeStateChange state)
            {
                if (state == PlayModeStateChange.EnteredPlayMode)
                {
                    if (_exception != null)
                    {
                        EditorApplication.ExitPlaymode();
                        Debug.LogError("Can't enter Playmode: " + _exception);
                    }
                }
            }
        }
    }
}