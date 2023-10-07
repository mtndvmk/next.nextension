using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace Nextension.NEditor
{
    public class AssetImportedChecker
    {
        internal class CustomAssetPostprocessor : AssetPostprocessor
        {
#if UNITY_2021_2_OR_NEWER
            private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
#else
            private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
#endif
            {
                if (importedAssets.Length + deletedAssets.Length + movedAssets.Length > 0)
                {
                    var types = NUtils.getCustomTypes();

                    List<(MethodInfo, int)> importedMethods = new();
                    List<(MethodInfo, int)> deletedMethods = new();
                    List<(MethodInfo, int)> movedMethods = new();

                    BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
                    foreach (var type in types)
                    {
                        if (type.GetInterface(typeof(IAssetImportedCallback).Name) != null)
                        {
                            if (importedAssets.Length > 0)
                            {
                                var method = type.GetMethod(nameof(IAssetImportedCallback.onAssetImported), bindingFlags);
                                if (method != null && method.GetParameters().Length == 1 && method.GetParameters()[0].ParameterType == typeof(string))
                                {
                                    var order = (int)(type.GetProperty(nameof(IAssetImportedCallback.Priority), bindingFlags)?.GetValue(null) ?? 0);
                                    importedMethods.Add((method, order));
                                }
                            }
                            if (deletedAssets.Length > 0)
                            {
                                var method = type.GetMethod(nameof(IAssetImportedCallback.onAssetDeleted), bindingFlags);
                                if (method != null && method.GetParameters().Length == 1 && method.GetParameters()[0].ParameterType == typeof(string))
                                {
                                    var order = (int)(type.GetProperty(nameof(IAssetImportedCallback.Priority), bindingFlags)?.GetValue(null) ?? 0);
                                    deletedMethods.Add((method, order));
                                }
                            }
                            if (movedAssets.Length > 0)
                            {
                                var method = type.GetMethod(nameof(IAssetImportedCallback.onAssetMoved), bindingFlags);
                                if (method != null && method.GetParameters().Length == 1 && method.GetParameters()[0].ParameterType == typeof(string))
                                {
                                    var order = (int)(type.GetProperty(nameof(IAssetImportedCallback.Priority), bindingFlags)?.GetValue(null) ?? 0);
                                    movedMethods.Add((method, order));
                                }
                            }
                        }
                    }

                    importedMethods.Sort((a, b) => b.Item2.CompareTo(a.Item2));
                    deletedMethods.Sort((a, b) => b.Item2.CompareTo(a.Item2));
                    movedMethods.Sort((a, b) => b.Item2.CompareTo(a.Item2));

                    if (importedAssets.Length > 0)
                    {
                        foreach (var p in importedAssets)
                        {
                            foreach (var m in importedMethods)
                            {
                                m.Item1.Invoke(null, new object[] { p });
                            }
                        }
                    }
                    if (deletedAssets.Length > 0)
                    {
                        foreach (var p in deletedAssets)
                        {
                            foreach (var m in deletedMethods)
                            {
                                m.Item1.Invoke(null, new object[] { p });
                            }
                        }
                    }
                    if (movedAssets.Length > 0)
                    {
                        foreach (var p in movedAssets)
                        {
                            foreach (var m in movedMethods)
                            {
                                m.Item1.Invoke(null, new object[] { p });
                            }
                        }
                    }
                }
            }
        }
    }
}
