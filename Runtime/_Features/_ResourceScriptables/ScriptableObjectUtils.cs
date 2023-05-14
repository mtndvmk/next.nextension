using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Nextension
{
#if UNITY_EDITOR
    public static class NEditorUtils
    {
        internal class CustomAssetPostprocessor : AssetPostprocessor
        {
#if UNITY_2021_2_OR_NEWER
            private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
#else
            private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
#endif
            {
                bool isDidDomainReload = EditorApplication.isCompiling | EditorApplication.isUpdating;
                _isDomainReload = isDidDomainReload;
                bool isNeedSave = false;
                if (!isDidDomainReload)
                {
                    if (deletedAssets.Length > 0)
                    {
                        ResourceScriptableTable.removeNullItem();
                        isNeedSave = true;
                    }
                }
                if (_needSaves.Count > 0)
                {
                    isNeedSave = true;
                }
                if (!_isSaving && isNeedSave)
                {
                    _isSaving = true;
                    saveInNext();
                }
            }
        }

        internal const string SCRIPTABLE_OBJECT_EXTENSION = ".asset";
        internal const string MAIN_RESOURCE_PATH = "Assets/Resources";

        private static List<ScriptableObject> _needSaves = new List<ScriptableObject>();
        private static bool _isDomainReload = false;
        private static bool _isSaving = false;
        private async static void saveInNext()
        {
            await new NWaitUntil_Editor(() => _isDomainReload == false);
            foreach (var s in _needSaves)
            {
                if (s != null)
                {
                    UnityEditor.EditorUtility.SetDirty(s);
                }
            }

            UnityEditor.AssetDatabase.SaveAssets();
            _needSaves.Clear();
            _isSaving = false;
        }

        public static void setDirty(Object @object)
        {
            EditorUtility.SetDirty(@object);
        }
        public static void saveAsset(ScriptableObject scriptableObject)
        {
            if (!scriptableObject)
            {
                return;
            }
            NEditorUtils.setDirty(scriptableObject);
            _needSaves.add(scriptableObject);
            saveAssets();
        }

        public static async void saveAssets()
        {
            await awaitImportWorkerProcess();
            AssetDatabase.SaveAssets();
        }

        public static async Task awaitImportWorkerProcess()
        {
            var isWorker = AssetDatabase.IsAssetImportWorkerProcess();
            await new NWaitUntil_Editor(() => !isWorker);
        }
        public static bool isFile(ScriptableObject scriptableObject)
        {
            return !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(scriptableObject));
        }
        public static ScriptableObject createScriptableOnResource(System.Type type, string fileName = null)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = type.Name;
            }
            var dir = MAIN_RESOURCE_PATH;
            if (!System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }
            var path = System.IO.Path.Combine(dir, fileName + NEditorUtils.SCRIPTABLE_OBJECT_EXTENSION);
            ScriptableObject scriptable;
            if (System.IO.File.Exists(path))
            {
                scriptable = Resources.Load<ScriptableObject>(fileName);
            }
            else
            {
                scriptable = ScriptableObject.CreateInstance(type);
                AssetDatabase.CreateAsset(scriptable, path);
                Debug.Log("Create asset at " + path);
            }
            return scriptable;
        }
        public static bool hasScriptableOnResource(string fileName)
        {
            return getScriptableOnResource(fileName) != null;
        }
        public static ScriptableObject getScriptableOnResource(string fileName)
        {
            var dir = MAIN_RESOURCE_PATH;
            if (!System.IO.Directory.Exists(dir))
            {
                return null;
            }
            var path = System.IO.Path.Combine(dir, fileName + NEditorUtils.SCRIPTABLE_OBJECT_EXTENSION);
            if (!System.IO.File.Exists(path))
            {
                return null;
            }
            var scriptable = Resources.Load<ScriptableObject>(fileName);
            if (scriptable == null)
            {
                return null;
            }

            return scriptable;
        }
        public static T createScriptableOnResource<T>(string fileName = null) where T : ScriptableObject
        {
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = typeof(T).Name;
            }
            var dir = MAIN_RESOURCE_PATH;
            if (!System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }
            var path = System.IO.Path.Combine(dir, fileName + NEditorUtils.SCRIPTABLE_OBJECT_EXTENSION);
            T scriptable;
            if (System.IO.File.Exists(path))
            {
                scriptable = Resources.Load<T>(fileName);
            }
            else
            {
                scriptable = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(scriptable, path);
                Debug.Log("Create asset at " + path);
            }
            return scriptable;
        }
    }
#else
    public static class NEditorUtils
    {
        [Conditional("UNITY_EDITOR")]
        public static void saveAsset(ScriptableObject scriptableObject)
        {
            
        }
        [Conditional("UNITY_EDITOR")]
        public static async void saveAssets()
        {
            
        }
        [Conditional("UNITY_EDITOR")]
        public static void setDirty(Object @object)
        {
            
        }
    }
#endif
}
