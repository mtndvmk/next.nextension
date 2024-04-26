using System.Collections.Generic;
using System.IO;
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
            await new NWaitUntil_Editor(() => !IsCompiling);
        }
        public static bool IsCompiling => AssetDatabase.IsAssetImportWorkerProcess() || EditorApplication.isUpdating || EditorApplication.isCompiling;

        public static string createMainResourcesPath(string fileName)
        {
            var index = fileName.IndexOf(MAIN_RESOURCE_PATH);
            if (index < 0)
            {
                fileName = Path.Combine(MAIN_RESOURCE_PATH, fileName);
            }
            if (!fileName.Contains('.'))
            {
                fileName += SCRIPTABLE_OBJECT_EXTENSION;
            }
            return fileName;
        }
        public static bool delete(Object @object)
        {
            if (!@object || !isFile(@object)) return false;
            try
            {
                var filePath = AssetDatabase.GetAssetPath(@object);
                return AssetDatabase.DeleteAsset(filePath);
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
        public static (string path, ScriptableObject scriptableObject)[] loadAll(string directory)
        {
            if (!Directory.Exists(directory))
            {
                return null;
            }
            var files = Directory.GetFiles(directory, "*.asset");
            List<(string path, ScriptableObject scriptableObject)> result = new();
            foreach (var filePath in files)
            {
                try
                {
                    var so = loadAssetAt<ScriptableObject>(filePath);
                    if (so)
                    {
                        result.Add((filePath, so));
                    }
                }
                catch { }
            }

            return result.ToArray();
        }
        public static Object findAssetAt(string directory, System.Type type, out string path)
        {
            if (!Directory.Exists(directory))
            {
                path = null;
                return null;
            }
            var files = Directory.GetFiles(directory, "*.asset");
            foreach (var filePath in files)
            {
                try
                {
                    var so = loadAssetAt<Object>(filePath);
                    if (so && so.GetType() == type)
                    {
                        path = filePath;
                        return so;
                    }
                }
                catch { }
            }
            path = null;
            return null;
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
                filePath = Path.Combine(MAIN_RESOURCE_PATH, fileName);
            }
            ScriptableObject scriptable;
            if (File.Exists(filePath))
            {
                scriptable = getMainObjectOnResources<ScriptableObject>(fileName);
                if (scriptable == null)
                {
                    throw new System.Exception($"`{fileName}` exists but can't be loaded");
                }
            }
            else
            {
                var dir = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                scriptable = ScriptableObject.CreateInstance(type);
                AssetDatabase.CreateAsset(scriptable, filePath);
                Debug.Log($"Create asset at `{filePath}`");
            }
            return scriptable;
        }
        public static string getGUID(Object @object)
        {
            return AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(@object));
        }
        public static Object loadMainAssetFromGUID(string guid)
        {
            return loadMainAssetFromGUID(guid, out _);
        }
        public static Object loadMainAssetFromGUID(string guid, out string path)
        {
            path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            return AssetDatabase.LoadMainAssetAtPath(path);
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
        public static bool hasObjectOnResources(string fileName)
        {
            return getMainObjectOnResources<Object>(fileName);
        }
        public static T getMainObjectOnResources<T>(string fileName) where T : Object
        {
#if UNITY_EDITOR
            var dir = MAIN_RESOURCE_PATH;
            if (!Directory.Exists(dir))
            {
                return null;
            }

            var index = fileName.IndexOf(dir);
            if (index < 0)
            {
                fileName = Path.Combine(dir, fileName);
            }
            if (!fileName.Contains('.'))
            {
                fileName += SCRIPTABLE_OBJECT_EXTENSION;
            }
            var path = fileName;
            if (!File.Exists(path))
            {
                return null;
            }
            var @object = AssetDatabase.LoadAssetAtPath<T>(path);
            if (@object == null)
            {
                @object = Resources.Load<T>(fileName.removeExtension());
                if (@object == null)
                {
                    @object = AssetDatabase.LoadMainAssetAtPath(path) as T;
                }
                return @object;
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
                AssetDatabase.SaveAssetIfDirty(@object);
            }
            else
            {
                _needSaves.addIfNotPresent(@object);
                saveInNext();
            }
#endif
        }
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void saveAssets(bool isImmediate = false)
        {
#if UNITY_EDITOR
            if (!isImmediate)
            {
                saveInNext();
            }
            else
            {
                AssetDatabase.SaveAssets();
            }
#endif
        }
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void refresh()
        {
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }
    }
}