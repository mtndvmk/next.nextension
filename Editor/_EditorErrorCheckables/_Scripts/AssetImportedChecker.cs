using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace Nextension.NEditor
{
    internal class AssetImportedChecker
    {
        public class CustomAssetPostprocessor : AssetPostprocessor
        {
            private static MethodInfo[] _importedMethods;
            private static MethodInfo[] _deletedMethods;
            private static MethodInfo[] _movedMethods;
            private static MethodInfo[] _postprocessMethods;

#if UNITY_2021_2_OR_NEWER
            private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
#else
            private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
#endif
            {
                if (importedAssets.Length + deletedAssets.Length + movedAssets.Length > 0)
                {
                    var types = NUtils.getCustomTypes();

                    if (_importedMethods == null)
                    {
                        var importedMethods = new List<(MethodInfo, int)>();
                        var deletedMethods = new List<(MethodInfo, int)>();
                        var movedMethods = new List<(MethodInfo, int)>();
                        var postprocessMethods = new List<(MethodInfo, int)>();

                        BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
                        foreach (var type in types)
                        {
                            if (type.GetInterface(typeof(IOnAssetImported).Name) != null)
                            {
                                if (importedAssets.Length > 0)
                                {
                                    var method = type.GetMethod(nameof(IOnAssetImported.onAssetImported), bindingFlags);
                                    if (method != null && method.GetParameters().Length == 1 && method.GetParameters()[0].ParameterType == typeof(string))
                                    {
                                        var order = (int)(type.GetProperty(nameof(IOnAssetImported.Priority), bindingFlags)?.GetValue(null) ?? 0);
                                        importedMethods.Add((method, order));
                                    }
                                }
                                if (deletedAssets.Length > 0)
                                {
                                    var method = type.GetMethod(nameof(IOnAssetImported.onAssetDeleted), bindingFlags);
                                    if (method != null && method.GetParameters().Length == 1 && method.GetParameters()[0].ParameterType == typeof(string))
                                    {
                                        var order = (int)(type.GetProperty(nameof(IOnAssetImported.Priority), bindingFlags)?.GetValue(null) ?? 0);
                                        deletedMethods.Add((method, order));
                                    }
                                }
                                if (movedAssets.Length > 0)
                                {
                                    var method = type.GetMethod(nameof(IOnAssetImported.onAssetMoved), bindingFlags);
                                    if (method != null && method.GetParameters().Length == 1 && method.GetParameters()[0].ParameterType == typeof(string))
                                    {
                                        var order = (int)(type.GetProperty(nameof(IOnAssetImported.Priority), bindingFlags)?.GetValue(null) ?? 0);
                                        movedMethods.Add((method, order));
                                    }
                                }
                                {
                                    var method = type.GetMethod(nameof(IOnAssetImported.onPostprocessAllAssets), bindingFlags);
                                    if (method != null && method.GetParameters().Length == 4
                                        && method.GetParameters()[0].ParameterType == typeof(string[])
                                        && method.GetParameters()[1].ParameterType == typeof(string[])
                                        && method.GetParameters()[2].ParameterType == typeof(string[])
                                        && method.GetParameters()[3].ParameterType == typeof(string[]))
                                    {
                                        var order = (int)(type.GetProperty(nameof(IOnAssetImported.Priority), bindingFlags)?.GetValue(null) ?? 0);
                                        postprocessMethods.Add((method, order));
                                    }
                                }
                            }
                        }

                        importedMethods.Sort((a, b) => b.Item2.CompareTo(a.Item2));
                        deletedMethods.Sort((a, b) => b.Item2.CompareTo(a.Item2));
                        movedMethods.Sort((a, b) => b.Item2.CompareTo(a.Item2));
                        postprocessMethods.Sort((a, b) => b.Item2.CompareTo(a.Item2));

                        _importedMethods = new MethodInfo[importedMethods.Count];
                        for (int i = 0; i < importedMethods.Count; i++)
                        {
                            _importedMethods[i] = importedMethods[i].Item1;
                        }
                        _deletedMethods = new MethodInfo[deletedMethods.Count];
                        for (int i = 0; i < deletedMethods.Count; i++)
                        {
                            _deletedMethods[i] = deletedMethods[i].Item1;
                        }
                        _movedMethods = new MethodInfo[movedMethods.Count];
                        for (int i = 0; i < movedMethods.Count; i++)
                        {
                            _movedMethods[i] = movedMethods[i].Item1;
                        }
                        _postprocessMethods = new MethodInfo[postprocessMethods.Count];
                        for (int i = 0; i < postprocessMethods.Count; i++)
                        {
                            _postprocessMethods[i] = postprocessMethods[i].Item1;
                        }
                    }

                    object[] arr = null;
                    if (importedAssets.Length > 0)
                    {
                        arr ??= new object[1];
                        foreach (var p in importedAssets)
                        {
                            arr[0] = p;
                            foreach (var m in _importedMethods)
                            {
                                m.Invoke(null, arr);
                            }
                        }
                    }
                    if (deletedAssets.Length > 0)
                    {
                        arr ??= new object[1];
                        foreach (var p in deletedAssets)
                        {
                            arr[0] = p;
                            foreach (var m in _deletedMethods)
                            {
                                m.Invoke(null, arr);
                            }
                        }
                    }
                    if (movedAssets.Length > 0)
                    {
                        arr ??= new object[1];
                        foreach (var p in movedAssets)
                        {
                            arr[0] = p;
                            foreach (var m in _movedMethods)
                            {
                                m.Invoke(null, arr);
                            }
                        }
                    }
                    {
                        var input = new object[] { importedAssets, deletedAssets, movedAssets, movedFromAssetPaths };
                        foreach (var p in _postprocessMethods)
                        {
                            p.Invoke(null, input);
                        }
                    }
                }
            }
        }
    }
}
