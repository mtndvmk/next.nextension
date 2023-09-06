using System.Collections.Generic;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Nextension
{
#if UNITY_EDITOR
    public static class NAssetUtils
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

        private static List<Object> _needSaves = new List<Object>();
        private static bool _isDomainReload = false;
        private static bool _isSaving = false;
        private async static void saveInNext()
        {
            await new NWaitUntil_Editor(() => _isDomainReload == false);
            foreach (var s in _needSaves)
            {
                if (s != null)
                {
                    EditorUtility.SetDirty(s);
                }
            }

            AssetDatabase.SaveAssets();
            _needSaves.Clear();
            _isSaving = false;
        }

        public static void setDirty(Object @object)
        {
            EditorUtility.SetDirty(@object);
        }
        public static void saveAsset(Object @object)
        {
            if (!@object)
            {
                return;
            }
            NAssetUtils.setDirty(@object);
            _needSaves.add(@object);
            saveAssets();
        }

        public static async void saveAssets()
        {
            await awaitImportWorkerProcess();
            AssetDatabase.SaveAssets();
        }

        private static async Task awaitImportWorkerProcess()
        {
            await new NWaitUntil_Editor(() => !AssetDatabase.IsAssetImportWorkerProcess() && !EditorApplication.isUpdating);
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
            if (!fileName.Contains('.'))
            {
                fileName += NUnityResourcesUtils.SCRIPTABLE_OBJECT_EXTENSION;
            }
            var dir = NUnityResourcesUtils.MAIN_RESOURCE_PATH;
            if (!System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }
            var path = System.IO.Path.Combine(dir, fileName);
            ScriptableObject scriptable;
            if (System.IO.File.Exists(path))
            {
                scriptable = NUnityResourcesUtils.getObjectOnMainResource<ScriptableObject>(fileName);
            }
            else
            {
                scriptable = ScriptableObject.CreateInstance(type);
                AssetDatabase.CreateAsset(scriptable, path);
                Debug.Log("Create asset at " + path);
            }
            return scriptable;
        }
    }
#else
    public static class NAssetUtils
    {
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void saveAsset(ScriptableObject scriptableObject)
        {
            
        }
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static async void saveAssets()
        {
            
        }
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void setDirty(Object @object)
        {
            
        }
    }
#endif
}
