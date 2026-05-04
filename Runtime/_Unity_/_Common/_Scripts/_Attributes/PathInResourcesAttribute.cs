using System;

namespace Nextension
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class PathInResourcesAttribute : Attribute
    {
        internal string path;
        public PathInResourcesAttribute(string path)
        {
            this.path = path;
        }
    }
}
