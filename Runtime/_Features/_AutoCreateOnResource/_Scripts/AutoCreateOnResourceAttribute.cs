using System;
using UnityEngine;

namespace Nextension
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class AutoCreateOnResourceAttribute : Attribute
    {
#if UNITY_EDITOR
        internal readonly string fileName;
        internal bool useTypeName => string.IsNullOrEmpty(fileName);
        public AutoCreateOnResourceAttribute() { fileName = null; }
        public AutoCreateOnResourceAttribute(string fileName) { this.fileName = fileName; }
        public string getFileName(Type type)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return "AutoCreated/[AutoCreated] " + type.Name;
            }
            return "AutoCreated/[AutoCreated] " + fileName;
        }
#endif
    }

    public class AutoCreateOnResourceUtils
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
