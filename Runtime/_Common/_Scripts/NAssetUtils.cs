using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Nextension
{
    public static class NAssetUtils
    {
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void setDirty(Object @object)
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(@object);
#endif
        }
        public static bool hasObjectInMainResources(string fileName)
        {
            return getMainObjectInMainResources<Object>(fileName);
        }
        public static T getMainObjectInMainResources<T>(string fileName) where T : Object
        {
#if UNITY_EDITOR
            var dir = NEditorAssetUtils.MAIN_RESOURCE_PATH;
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
                fileName += NEditorAssetUtils.SCRIPTABLE_OBJECT_EXTENSION;
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
            NEditorAssetUtils.saveAsset(@object, isImmediate);
#endif
        }
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void saveAssets(bool isImmediate = false)
        {
#if UNITY_EDITOR
            NEditorAssetUtils.saveAssets(isImmediate);
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