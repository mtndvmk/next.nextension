#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Nextension
{
    public static class NUnityResourcesUtils
    {
        internal const string MAIN_RESOURCE_PATH = "Assets/Resources";
        internal const string SCRIPTABLE_OBJECT_EXTENSION = ".asset";

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
            var path = System.IO.Path.Combine(dir, fileName);
            if (!System.IO.File.Exists(path))
            {
                return null;
            }
            if (!fileName.Contains('.'))
            {
                fileName += SCRIPTABLE_OBJECT_EXTENSION;
            }
            var @object = AssetDatabase.LoadAssetAtPath<T>(fileName);
            if (@object == null)
            {
                return null;
            }
#else
            fileName = System.IO.Path.GetFileNameWithoutExtension(fileName);
            var @object = Resources.Load<T>(fileName);
#endif

            return @object;
        }
    }
}
