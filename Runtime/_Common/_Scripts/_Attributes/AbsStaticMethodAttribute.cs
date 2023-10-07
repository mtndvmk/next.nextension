using System;

namespace Nextension
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public abstract class AbsStaticMethodAttribute : Attribute
    {
        /// <summary>
        /// Callbacks with higher values are called before ones with lower values.
        /// </summary>
        internal int priority;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="priority">Callbacks with higher values are called before ones with lower values.</param>
        public AbsStaticMethodAttribute(int priority) { this.priority = priority; }
    }
    public class StartupMethodAttribute : AbsStaticMethodAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="priority">Callbacks with higher values are called before ones with lower values.</param>
        public StartupMethodAttribute(int priority = 0) : base(priority) { }
    }
    public class QuittingMethodAttribute : AbsStaticMethodAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="priority">Callbacks with higher values are called before ones with lower values.</param>
        public QuittingMethodAttribute(int priority = 0) : base(priority) { }
    }
    public class EditorQuittingMethodAttribute : AbsStaticMethodAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="priority">Callbacks with higher values are called before ones with lower values.</param>
        public EditorQuittingMethodAttribute(int priority = 0) : base(priority) { }
    }
}