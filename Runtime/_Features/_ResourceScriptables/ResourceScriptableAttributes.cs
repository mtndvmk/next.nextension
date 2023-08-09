using System;

namespace Nextension
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class AutoCreateOnResourceAttribute : Attribute
    {
        private readonly string postName;
        internal readonly bool onlyCreateIfAccess;
        public AutoCreateOnResourceAttribute()
        {
            postName = string.Empty;
        }
        public AutoCreateOnResourceAttribute(bool onlyCreateIfAccess)
        {
            postName = string.Empty;
            this.onlyCreateIfAccess = onlyCreateIfAccess;
        }
        public AutoCreateOnResourceAttribute(string postName)
        {
            this.postName = postName.Trim();
        }
        public AutoCreateOnResourceAttribute(string postName, bool onlyCreateIfAccess)
        {
            this.postName = postName.Trim();
            this.onlyCreateIfAccess = onlyCreateIfAccess;
        }

        public string getFileName(Type type)
        {
            var fileName = "AutoCreated_" + type.Name;
            if (!string.IsNullOrEmpty(postName))
            {
                fileName += "_" + postName;
            }
            return fileName;
        }

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
            return true;
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class PreloadResourceScriptableAttribute : Attribute
    {

    }
}
