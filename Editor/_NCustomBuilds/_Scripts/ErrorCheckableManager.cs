using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Nextension.NEditor
{
    public class ErrorCheckableManager : IPreprocessBuildWithReport
    {
        public int callbackOrder => int.MinValue;

        static Exception _exception;
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
        [InitializeOnLoadMethod]
        static async void checkOnEditorLoop()
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
            //NEditorHelper.clearLog();
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