using System;

namespace Nextension
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class SingletonScriptableAttribute : Attribute
    {
        public bool isPreload;
    }
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class NonSingletonScriptableAttribute : Attribute
    {

    }
}