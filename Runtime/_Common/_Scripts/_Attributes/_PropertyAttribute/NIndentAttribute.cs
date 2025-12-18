using System;
using UnityEngine;

namespace Nextension
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class NIndentAttribute : ApplyToCollectionPropertyAttribute
    {
        public readonly int indentLevel;
        public readonly string bullet = "↳";
        public NIndentAttribute(int indentLevel = 1) : base()
        {
            this.indentLevel = indentLevel;
            this.order = NAttributeOrder.INDENT;
        }
        public NIndentAttribute(string bullet, int indentLevel = 1) : base()
        {
            this.bullet = bullet;
            this.indentLevel = indentLevel;
            this.order = NAttributeOrder.INDENT;
        }
    }
}
