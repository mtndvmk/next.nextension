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
#if ENABLE_RESOURCE_SCRIPTABLE_REF
            scriptableReferences.Clear();
#endif
        }

        private const string TABLE_FILENAME = "ResourceScriptableTable";
        [SerializeField] private List<ResourceScriptable> preloadScriptables = new List<ResourceScriptable>();
#if ENABLE_RESOURCE_SCRIPTABLE_REF
        [SerializeField] private List<ResourceScriptableReference> scriptableReferences = new List<ResourceScriptableReference>();
#endif

        private Dictionary<Type, ResourceScriptable> loadedScriptables;

#region Editor
#if UNITY_EDITOR
        [ContextMenu("Reload and save")]
        private void reloadAndSave()
        {
            if (!fetchAllResourceScriptable())
            {
                saveTable();
            }
        }

        [MenuItem("Nextension/ResourceScriptableTable/Create Table")]
        private static void createTable()
        {
            NEditorUtils.createScriptableOnResource(typeof(ResourceScriptableTable), TABLE_FILENAME);
        }

        [InitializeOnLoadMethod]
        private static bool fetchAllResourceScriptable()
        {
            if (Application.isPlaying || !Table || EditorApplication.isCompiling)
            {
                return false;
            }
            createAutoCreateOnResourceAttribute();
            var table = Table;
            table.findAllResourceScriptable();
            removeNullItem();
            return table.updateTable();
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
            if (!table.innerContains(resourceScriptable))
            {
                table.innerAdd(resourceScriptable);
                removeNullItem();
                saveTable();
            }
        }
        internal static async void removeNullItem()
        {
            await new NWaitUntil_Editor(() => Table);
            int count = 0;
            count += Table.preloadScriptables.RemoveAll(i => i == null);
#if ENABLE_RESOURCE_SCRIPTABLE_REF
            count += Table.scriptableReferences.RemoveAll(i => i == null || i.ResourceScriptable == null);
#endif
            if (count > 0)
            {
                saveTable();
            }
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
        private static void saveTable()
        {
            if (_table)
            {
                NEditorUtils.saveAsset(_table);
                Debug.Log($"Saved {nameof(ResourceScriptableTable)}");
            }
        }

        private bool innerContains(ResourceScriptable resourceScriptable)
        {
            if (preloadScriptables.Contains(resourceScriptable))
            {
                return true;
            }
#if ENABLE_RESOURCE_SCRIPTABLE_REF
            if (scriptableReferences.Exists(item => item.ResourceScriptable == resourceScriptable))
            {
                return true;
            }
#endif
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
#if ENABLE_RESOURCE_SCRIPTABLE_REF
            else
            {
                var rref = new ResourceScriptableReference(resourceScriptable);
                scriptableReferences.addAndSort(rref);
            }
#endif
        }
        private bool isPreload(ResourceScriptable resourceScriptable)
        {
#if ENABLE_RESOURCE_SCRIPTABLE_REF
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
#endif
            return true;
        }
        private async void unloadResourceScriptable()
        {
            await new NWaitUntil_Editor(() => !NStartRunner.IsPlaying);
            foreach (var item in loadedScriptables)
            {
                if (!preloadScriptables.Contains(item.Value))
                {
                    Resources.UnloadAsset(item.Value);
                }
            }
        }
        private bool updateTable()
        {
            List<ResourceScriptable> temp = new List<ResourceScriptable>();
            bool hasChange = false;
            for (int i = preloadScriptables.Count - 1; i >=0; i--)
            {
                var rs = preloadScriptables[i];
                if (!rs)
                {
                    hasChange = true;
                    preloadScriptables.RemoveAt(i); continue;
                }
                if (!isPreload(rs))
                {
                    hasChange = true;
                    preloadScriptables.RemoveAt(i);
                    temp.Add(rs);
                }
            }

#if ENABLE_RESOURCE_SCRIPTABLE_REF
            for (int i = scriptableReferences.Count - 1; i >= 0; i--)
            {
                var rref = scriptableReferences[i];
                if (rref == null || !rref.ResourceScriptable)
                {
                    hasChange = true;
                    scriptableReferences.RemoveAt(i); continue;
                }
                if (isPreload(rref.ResourceScriptable))
                {
                    hasChange = true;
                    scriptableReferences.RemoveAt(i);
                    temp.Add(rref.ResourceScriptable);
                }
                else
                {
                    hasChange |= rref.updateRef();
                }
            }
#endif

            foreach (var r in temp)
            {
                innerAdd(r);
                hasChange = true;
            }

            if (hasChange)
            {
                saveTable();
            }

            return hasChange;
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

        private static void createAutoCreateOnResourceAttribute()
        {
            var types = NUtils.getCustomTypes();
            foreach (var type in types)
            {
                var autoCreateAttr = type.GetCustomAttribute<AutoCreateOnResourceAttribute>();
                if (autoCreateAttr != null && !autoCreateAttr.onlyCreateIfAccess)
                {
                    if (!AutoCreateOnResourceAttribute.checkValid(type))
                    {
                        continue;
                    }
                    var fileName = autoCreateAttr.getFileName(type);
                    var scriptable = NUnityResourcesUtils.getObjectOnMainResource<ScriptableObject>(fileName);
                    if (!scriptable)
                    {
                        NEditorUtils.createScriptableOnResource(type, fileName);
                    }
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
                    for (int i = 0; i < movedAssets.Length; ++i)
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
                    if (_table == null)
                    {
                        Debug.LogWarning("Can't not load ResourceScriptableTable");
                        return null;
                    }
                    preload();
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
#if ENABLE_RESOURCE_SCRIPTABLE_REF
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
#endif
            var autoCreateAttr = type.GetCustomAttribute<AutoCreateOnResourceAttribute>();
            if (autoCreateAttr != null)
            {
                if (!AutoCreateOnResourceAttribute.checkValid(type))
                {
                    throw new Exception($"{type} is not valid");
                }
#if UNITY_EDITOR
                var fileName = autoCreateAttr.getFileName(type);
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

        private static void preload()
        {
#if UNITY_EDITOR
            removeNullItem();
#endif
            _table.loadedScriptables = new Dictionary<Type, ResourceScriptable>();
            foreach (var item in _table.preloadScriptables)
            {
                if (item)
                {
                    _table.loadedScriptables.Add(item.GetType(), item);
                }
            }
#if UNITY_EDITOR
            _table.unloadResourceScriptable();
#endif
        }

        private static bool checkValid(Type resourceScriptableType)
        {
            if (resourceScriptableType.IsAbstract)
            {
                return false;
            }
            if (resourceScriptableType.ContainsGenericParameters)
            {
                return false;
            }
            var baseType = resourceScriptableType.BaseType;
            while (baseType != null)
            {
                if (baseType.Name.Equals("ResourceScriptable`1"))
                {
                    return true;
                }
                baseType = baseType.BaseType;
            }
            return false;
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
