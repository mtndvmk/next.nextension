using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Nextension.NEditor
{
    internal class EditorReportingManager
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
                        if (t.GetInterface(typeof(IOnCompiled).Name) != null)
                        {
                            var method = t.GetMethod(nameof(IOnCompiled.onPreprocessBuild), bindingFlags);
                            if (method != null)
                            {
                                if (method.GetParameters().Length > 0)
                                {
                                    continue;
                                }
                                var order = (int)(t.GetProperty(nameof(IOnCompiled.Priority), bindingFlags)?.GetValue(null) ?? 0);
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
                    if (t.GetInterface(typeof(IOnCompiled).Name) != null)
                    {
                        var method = t.GetMethod(nameof(IOnCompiled.onLoadOrRecompiled), bindingFlags);
                        if (method != null)
                        {
                            if (method.GetParameters().Length > 0)
                            {
                                continue;
                            }
                            var order = (int)(t.GetProperty(nameof(IOnCompiled.Priority), bindingFlags)?.GetValue(null) ?? 0);
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
    }
}