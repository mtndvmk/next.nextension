using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Nextension
{
    public sealed class ResourceScriptableTable : ScriptableObject, INPreprocessBuild
    {
        private void OnEnable()
        {
            if (_table == null)
            {
                _table = this;
            }
            hideFlags = HideFlags.NotEditable;
        }
        [ContextMenu("Clear table")]
        private void clear()
        {
            preloadScriptables.Clear();
            scriptableReferences.Clear();
        }

        private const string TABLE_FILENAME = "ResourceScriptableTable";
        [SerializeField] private List<ResourceScriptable> preloadScriptables = new List<ResourceScriptable>();
        [SerializeField] private List<ResourceScriptableReference> scriptableReferences = new List<ResourceScriptableReference>();

        private Dictionary<Type, ResourceScriptable> loadedScriptables;

        #region Editor
#if UNITY_EDITOR
        [MenuItem("Nextension/ResourceScriptableTable/Create Table")]
        private static void createTable()
        {
            NEditorUtils.createScriptableOnResource<ResourceScriptableTable>(TABLE_FILENAME);
        }

        [ContextMenu("Find all ResourceScriptable")]
        [InitializeOnLoadMethod]
        private static void fetchAllResourceScriptable()
        {
            if (Application.isPlaying || !Table)
            {
                return;
            }
            var table = Table;
            table.findAllResourceScriptable();
            removeNullItem();
            table.updateTable();
        }

        [MenuItem("Nextension/ResourceScriptableTable/Ping Table")]
        internal static void pingTable()
        {
            if (Table)
            {
                EditorGUIUtility.PingObject(Table);
            }
        }
        internal static async void add(ResourceScriptable resourceScriptable)
        {
            if (!checkValid(resourceScriptable))
            {
                Debug.LogWarning(resourceScriptable.name + ": Please inherit directly from [ResourceScriptable<T>]", resourceScriptable);
                return;
            }
            await new NWaitUntil_Editor(() => Table);
            var table = Table;
            if (!table.innerContain(resourceScriptable))
            {
                table.innerAdd(resourceScriptable);
                removeNullItem();
            }
        }
        internal static async void removeNullItem()
        {
            await new NWaitUntil_Editor(() => Table);
            Table.preloadScriptables.RemoveAll(i => i == null);
            Table.scriptableReferences.RemoveAll(i => i == null || i.ResourceScriptable == null);
            NEditorUtils.saveAsset(Table);
        }

        private static void checkTable()
        {
            if (_table)
            {
                if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(_table)))
                {
                    _table = null;
                }
            }
        }
        private bool innerContain(ResourceScriptable resourceScriptable)
        {
            if (preloadScriptables.Contains(resourceScriptable))
            {
                return true;
            }
            if (scriptableReferences.Exists(item => item.ResourceScriptable == resourceScriptable))
            {
                return true;
            }
            return false;
        }
        private void innerAdd(ResourceScriptable resourceScriptable)
        {
            if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(resourceScriptable)))
            {
                return;
            }
            if (isPreload(resourceScriptable))
            {
                preloadScriptables.Add(resourceScriptable);
            }
            else
            {
                var rref = new ResourceScriptableReference(resourceScriptable);
                scriptableReferences.Add(rref);
            }
        }
        private bool isPreload(ResourceScriptable resourceScriptable)
        {
            if (resourceScriptable.GetType().GetCustomAttribute(typeof(PreloadResourceScriptableAttribute)) != null)
            {
                return true;
            }

            var path = AssetDatabase.GetAssetPath(resourceScriptable);
            if (string.IsNullOrEmpty(path))
            {
                throw new Exception("Path is null or empty: " + resourceScriptable);
            }
            if (path.Contains("/Resources/"))
            {
                return false;
            }
            return true;
        }
        private void updateTable()
        {
            List<ResourceScriptable> temp = new List<ResourceScriptable>();
            for (int i = preloadScriptables.Count - 1; i >=0; i--)
            {
                var rs = preloadScriptables[i];
                if (!rs)
                {
                    preloadScriptables.RemoveAt(i); continue;
                }
                if (!isPreload(rs))
                {
                    preloadScriptables.RemoveAt(i);
                    temp.Add(rs);
                }
            }

            for (int i = scriptableReferences.Count - 1; i >= 0; i--)
            {
                var rref = scriptableReferences[i];
                if (rref == null || !rref.ResourceScriptable)
                {
                    scriptableReferences.RemoveAt(i); continue;
                }
                if (isPreload(rref.ResourceScriptable))
                {
                    scriptableReferences.RemoveAt(i);
                    temp.Add(rref.ResourceScriptable);
                }
                else
                {
                    rref.updateRef();
                }
            }

            foreach (var r in temp)
            {
                innerAdd(r);
            }

            NEditorUtils.saveAsset(Table);
        }

        private void findAllResourceScriptable()
        {
            removeNullItem();

            var guids = AssetDatabase.FindAssets("t:ResourceScriptable");
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var item = AssetDatabase.LoadAssetAtPath(path, typeof(ResourceScriptable)) as ResourceScriptable;
                if (item && checkValid(item))
                {
                    add(item);
                }
            }
        }

        internal class CustomAssetPostprocessor : AssetPostprocessor
        {
#if UNITY_2021_2_OR_NEWER
            private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
#else
            private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
#endif
            {
                if (deletedAssets.Length + movedAssets.Length == 0) 
                {
                    return;
                }

                if (_table == null)
                {
                    return;
                }

                bool isContinue = false;

                foreach (string str in deletedAssets)
                {
                    if (AssetDatabase.LoadAssetAtPath<ResourceScriptable>(str))
                    {
                        isContinue = true;
                        break;
                    }
                }

                if (!isContinue)
                {
                    for (int i = 0; i < movedAssets.Length; i++)
                    {
                        var str = movedAssets[i];
                        if (AssetDatabase.LoadAssetAtPath<ResourceScriptable>(str))
                        {
                            isContinue = true;
                            break;
                        }
                    }
                }

                if (!isContinue)
                {
                    return;
                }

                Debug.Log("Fetching ResourceScriptable...");
                fetchAllResourceScriptable();
            }
        }
#endif
        #endregion

        private static ResourceScriptableTable _table;
        private static ResourceScriptableTable Table
        {
            get
            {
#if UNITY_EDITOR
                checkTable();
#endif
                if (_table == null)
                {
                    _table = Resources.Load<ResourceScriptableTable>(TABLE_FILENAME);
                    if (_table == null)
                    {
#if UNITY_EDITOR
                        createTable();
#endif
                    }
                    else
                    {
                        _table.loadedScriptables = new Dictionary<Type, ResourceScriptable>();
                        foreach (var item in _table.preloadScriptables)
                        {
                            _table.loadedScriptables.Add(item.GetType(), item);
                        }
                    }
                }
                return _table;
            }
        }

        internal static ResourceScriptable get(Type type)
        {
            var table = Table;
            if (table == null)
            {
                throw new NullReferenceException("Table is null");
            }

            if (!checkValid(type))
            {
                throw new NotSupportedException($"Not support ResourceScriptable: {type.FullName}");
            }

            if (table.loadedScriptables.ContainsKey(type))
            {
                return table.loadedScriptables[type];
            }

            var fullNameType = type.FullName;
            foreach (var item in table.scriptableReferences)
            {
                if (item != null && fullNameType.Equals(item.FullNameType))
                {
                    var scriptable = Resources.Load(item.Path, type) as ResourceScriptable;
                    table.loadedScriptables.Add(type, scriptable);
                    return scriptable;
                }
            }

            var autoCreateAttribute = type.GetCustomAttribute(typeof(AutoCreateOnResourceAttribute)) as AutoCreateOnResourceAttribute;
            if (autoCreateAttribute != null)
            {
#if UNITY_EDITOR
                var fileName = autoCreateAttribute.getFileName(type);
                var resourceScriptable = NEditorUtils.createScriptableOnResource(type, fileName) as ResourceScriptable;
                if (resourceScriptable)
                {
                    add(resourceScriptable);       
                    table.loadedScriptables.Add(type, resourceScriptable);
                    return resourceScriptable;
                }
#else
                var resourceScriptable = ScriptableObject.CreateInstance(type) as ResourceScriptable;
                table.loadedScriptables.Add(type, resourceScriptable);
                return resourceScriptable;
#endif
            }
            throw new Exception($"Create or add {type} to table");
        }

        private static bool checkValid(Type resourceScriptableType)
        {
            var baseType = resourceScriptableType.BaseType;
            return baseType.Name.Equals("ResourceScriptable`1");
        }
        private static bool checkValid(ResourceScriptable resourceScriptable)
        {
            return checkValid(resourceScriptable.GetType());
        }

        public static void onPreprocessBuild()
        {
#if UNITY_EDITOR
            fetchAllResourceScriptable();
#endif
        }
    }
}
