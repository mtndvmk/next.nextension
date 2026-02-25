#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Nextension
{
    public static class NEditorAssetUtils
    {
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

        private static HashSet<Object> _needSaves = new();
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
            await TaskEditor.waitUntil(() => !IsCompiling);
        }
        public static bool IsCompiling => AssetDatabase.IsAssetImportWorkerProcess() || EditorApplication.isUpdating || EditorApplication.isCompiling;

        public static string generateMainResourcesPath(string fileName)
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
        public static bool isInAssets(Object @object)
        {
            var assetPath = AssetDatabase.GetAssetPath(@object);
            return assetPath.StartsWith("Assets/");
        }
        public static bool getPathInMainResources(Object @object, out string path)
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
        public static bool getPathInResources(Object @object, out string path)
        {
            var assetPath = AssetDatabase.GetAssetPath(@object);
            var index = assetPath.LastIndexOf("/Resources/");
            if (index < 0)
            {
                path = null;
                return false;
            }
            path = assetPath.Remove(0, index + 11);
            return true;
        }
        public static bool hasAssetAt(string path)
        {
            return loadAssetAt<Object>(path) != null;
        }
        public static bool isValidAssetPath(string path)
        {
            return path.StartsWith("Assets/") || path.Contains("/Assets/");
        }
        public static string getRelativeAssetPath(string path)
        {
            if (!path.StartsWith("Assets/"))
            {
                var indexOfAsset = path.IndexOf("/Assets/") + 1;
                if (indexOfAsset <= 0) throw new System.Exception($"Invalid asset path: {path}");
                path = path[indexOfAsset..];
            }
            return path;
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
        public static bool tryLoadAssetAt<T>(string path, out T asset) where T : Object
        {
            try
            {
                asset = loadAssetAt<T>(path);
                return true;
            }
            catch (System.Exception)
            {
                asset = null;
                return false;
            }
        }
        public static T loadAssetAt<T>(string path) where T : Object
        {
            path = getRelativeAssetPath(path);
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }
        public static T createInMainResources<T>(string fileName = null) where T : ScriptableObject
        {
            return (T)createInMainResources(typeof(T), fileName);
        }
        public static ScriptableObject createInMainResources(System.Type type, string fileName)
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
                scriptable = NAssetUtils.getMainObjectInMainResources<ScriptableObject>(fileName);
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
                Debug.Log($"Create asset at `{filePath}`", scriptable);
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
        public static void saveAsset(Object @object, bool isImmediate = false)
        {
            if (!@object) return;
            NAssetUtils.setDirty(@object);
            if (isImmediate)
            {
                AssetDatabase.SaveAssetIfDirty(@object);
            }
            else
            {
                _needSaves.Add(@object);
                saveInNext();
            }
        }
        public static void saveAssets(bool isImmediate = false)
        {
            if (!isImmediate)
            {
                saveInNext();
            }
            else
            {
                AssetDatabase.SaveAssets();
            }
        }
        public static void refreshDatabase()
        {
            AssetDatabase.Refresh();
        }
    }
}
#endif