using System.Collections.Generic;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Nextension
{
    public static class NAssetUtils
    {
        #region Editor Only
#if UNITY_EDITOR
        internal const string MAIN_RESOURCE_PATH = "Assets/Resources/";
        internal const string SCRIPTABLE_OBJECT_EXTENSION = ".asset";
        internal const string META_EXTENSION = ".meta";
        internal class CustomAssetPostprocessor : AssetPostprocessor
        {
#if UNITY_2021_2_OR_NEWER
            private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
#else
            private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
#endif
            {
                bool isDidDomainReload = EditorApplication.isCompiling || EditorApplication.isUpdating;
                bool isNeedSave = false;
                if (!isDidDomainReload)
                {
                    if (deletedAssets.Length > 0)
                    {
                        isNeedSave = true;
                    }
                }
                if (_needSaves.Count > 0)
                {
                    isNeedSave = true;
                }
                if (isNeedSave)
                {
                    saveInNext();
                }
            }
        }

        private static List<Object> _needSaves = new List<Object>();
        private static bool _isWaitingForSave;
        private async static void saveInNext()
        {
            if (_isWaitingForSave)
            {
                return;
            }
            _isWaitingForSave = true;
            await awaitImportWorkerProcess();
            _isWaitingForSave = false;
            foreach (var s in _needSaves)
            {
                if (s) EditorUtility.SetDirty(s);
            }

            AssetDatabase.SaveAssets();
            _needSaves.Clear();
        }
        internal static async Task awaitImportWorkerProcess()
        {
            await new NWaitUntil_Editor(() => !AssetDatabase.IsAssetImportWorkerProcess() && !EditorApplication.isUpdating && !EditorApplication.isCompiling);
        }

        public static bool delete(Object @object)
        {
            if (!@object || !isFile(@object)) return false;
            try
            {
                var file = AssetDatabase.GetAssetPath(@object);
                System.IO.File.Delete(file);
                var metaFile = file + META_EXTENSION;
                System.IO.File.Delete(metaFile);
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }

        public static bool isFile(Object @object)
        {
            return !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(@object));
        }
        public static bool getPathInResource(Object @object, out string path)
        {
            var assetPath = AssetDatabase.GetAssetPath(@object);
            var index = assetPath.IndexOf(MAIN_RESOURCE_PATH);
            if (index < 0)
            {
                path = null;
                return false;
            }
            path = assetPath.Remove(0, index + MAIN_RESOURCE_PATH.Length);
            return true;
        }
        public static bool hasAssetAt(string path)
        {
            return loadAssetAt<Object>(path) != null;
        }
        public static T loadAssetAt<T>(string path) where T : Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }
        public static T createOnResource<T>(string fileName = null) where T : ScriptableObject
        {
            return (T)createOnResource(typeof(T), fileName);
        }
        public static ScriptableObject createOnResource(System.Type type, string fileName)
        {
            if (!type.IsSubclassOf(typeof(ScriptableObject)))
            {
                throw new System.Exception($"[{type.FullName}] must inherit from ScriptableObject");
            }
            if (string.IsNullOrEmpty(fileName))
            {
                throw new System.Exception($"`{nameof(fileName)}` can't be null or empty");
            }
            if (!fileName.Contains('.'))
            {
                fileName += SCRIPTABLE_OBJECT_EXTENSION;
            }
            string filePath;
            if (fileName.Contains(MAIN_RESOURCE_PATH))
            {
                filePath = fileName;
            }
            else
            {
                filePath = System.IO.Path.Combine(MAIN_RESOURCE_PATH, fileName);
            }
            ScriptableObject scriptable;
            if (System.IO.File.Exists(filePath))
            {
                scriptable = getObjectOnMainResource<ScriptableObject>(fileName);
                if (scriptable == null)
                {
                    throw new System.Exception($"`{fileName}` exists but can't be loaded");
                }
            }
            else
            {
                var dir = System.IO.Path.GetDirectoryName(filePath);
                if (!System.IO.Directory.Exists(dir))
                {
                    System.IO.Directory.CreateDirectory(dir);
                }
                scriptable = ScriptableObject.CreateInstance(type);
                AssetDatabase.CreateAsset(scriptable, filePath);
                Debug.Log($"Create asset at `{filePath}`");
            }
            return scriptable;
        }
#endif
        #endregion

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void setDirty(Object @object)
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(@object);
#endif
        }
        public static bool hasObjectOnMainResource(string fileName)
        {
            return getObjectOnMainResource<Object>(fileName);
        }
        public static T getObjectOnMainResource<T>(string fileName) where T : Object
        {
#if UNITY_EDITOR
            var dir = MAIN_RESOURCE_PATH;
            if (!System.IO.Directory.Exists(dir))
            {
                return null;
            }

            var index = fileName.IndexOf(dir);
            if (index < 0)
            {
                fileName = System.IO.Path.Combine(dir, fileName);
            }
            if (!fileName.Contains('.'))
            {
                fileName += SCRIPTABLE_OBJECT_EXTENSION;
            }
            var path = fileName;
            if (!System.IO.File.Exists(path))
            {
                return null;
            }
            var @object = AssetDatabase.LoadAssetAtPath<T>(path);
            if (@object == null)
            {
                return null;
            }
#else
            var @object = Resources.Load<T>(fileName.removeExtension());
#endif
            return @object;
        }
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void saveAsset(Object @object, bool isImmediate = false)
        {
#if UNITY_EDITOR
            if (!@object) return;
            NAssetUtils.setDirty(@object);
            if (isImmediate)
            {
                AssetDatabase.SaveAssets();
            }
            else
            {
                _needSaves.add(@object);
                saveInNext();
            }
#endif
        }
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static async void saveAssets(bool isImmediate = false)
        {
#if UNITY_EDITOR
            if (!isImmediate)
            {
                await awaitImportWorkerProcess();
            }
            AssetDatabase.SaveAssets();
#endif
        }
    }
}