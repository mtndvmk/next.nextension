using Nextension;
using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Nextension.NEditor
{
    public class NCustomBuildProcess : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            try
            {
                foreach (var t in NUtils.getCustomTypes())
                {
                    if (t.GetInterface(typeof(INPreprocessBuild).Name) != null)
                    {
                        var method = t.GetMethod("onPreprocessBuild", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                        if (method != null)
                        {
                            method.Invoke(null, null);
                        }
                    }
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
                foreach (var t in NUtils.getCustomTypes())
                {
                    if (t.GetInterface(typeof(INPreprocessBuild).Name) != null)
                    {
                        var method = t.GetMethod("onReloadScript", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                        if (method != null)
                        {
                            method.Invoke(null, null);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
}