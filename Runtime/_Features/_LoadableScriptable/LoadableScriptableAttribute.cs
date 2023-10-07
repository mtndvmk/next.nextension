using System;

namespace Nextension
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class LoadableScriptableAttribute : Attribute
    {
        public bool isPreload;
    }
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class NonLoadableScriptableAttribute : Attribute
    {

    }
}