using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Nextension.NEditor
{
    public class NCustomBuildProcess : IPreprocessBuildWithReport
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
                    if (t.GetInterface(typeof(INPreprocessBuild).Name) != null)
                    {
                        var method = t.GetMethod(nameof(INPreprocessBuild.onPreprocessBuild), bindingFlags);
                        if (method != null)
                        {
                            var order = (int)(t.GetProperty(nameof(INPreprocessBuild.Priority), bindingFlags)?.GetValue(null) ?? 0);
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
        private static void onReloadScript()
        {
            try
            {
                List<(MethodInfo, int)> methods = new List<(MethodInfo, int)>();
                BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
                foreach (var t in NUtils.getCustomTypes())
                {
                    if (t.GetInterface(typeof(INPreprocessBuild).Name) != null)
                    {
                        var method = t.GetMethod(nameof(INPreprocessBuild.onReloadScript), bindingFlags);
                        if (method != null)
                        {
                            var order = (int)(t.GetProperty(nameof(INPreprocessBuild.Priority), bindingFlags)?.GetValue(null) ?? 0);
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
                Debug.LogException(e);
            }
        }
    }
}