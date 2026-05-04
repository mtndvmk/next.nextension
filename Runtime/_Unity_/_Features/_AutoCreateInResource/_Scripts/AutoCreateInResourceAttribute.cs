using System;
using UnityEngine;

namespace Nextension
{
#if UNITY_EDITOR
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class AutoCreateInResourceAttribute : Attribute
    {
        internal readonly string fileName;
        internal bool useTypeName => string.IsNullOrEmpty(fileName);
        public AutoCreateInResourceAttribute() { fileName = null; }
        public AutoCreateInResourceAttribute(string fileName) { this.fileName = fileName; }
        public string getFileName(Type type)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return $"AutoCreated/[AutoCreated] {type.Name}";
            }
            return $"AutoCreated/{fileName}";
        }
    }
#else
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class AutoCreateInResourceAttribute : Attribute
    {
        public AutoCreateInResourceAttribute() { }
        public AutoCreateInResourceAttribute(string fileName) {  }
    }
#endif

    internal class AutoCreateInResourceUtils
    {
        public static bool checkValid(Type type)
        {
            if (type.IsAbstract)
            {
                return false;
            }
            if (type.ContainsGenericParameters)
            {
                return false;
            }
            if (!type.IsSubclassOf(typeof(ScriptableObject)))
            {
                return false;
            }
            return true;
        }
    }
}
