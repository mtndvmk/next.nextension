using System;
using UnityEngine;

namespace Nextension
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class NDisplayNameAttribute : ApplyToCollectionPropertyAttribute
    {
        public readonly string displayName = string.Empty;
        public readonly Color color = Color.white;
        public NDisplayNameAttribute(string name) : base()
        {
            this.displayName = name;
            this.order = NAttributeOrder.DISPLAY_NAME;
        }
        public NDisplayNameAttribute(NColor color) : base()
        {
            this.color = color.Color;
            this.order = NAttributeOrder.DISPLAY_NAME;
        }
        public NDisplayNameAttribute(string name, NColor color) : base()
        {
            this.displayName = name;
            this.color = color.Color;
            this.order = NAttributeOrder.DISPLAY_NAME;
        }
        public NDisplayNameAttribute(string name, string color) : this(name, (NColor)color)
        {

        }
        public NDisplayNameAttribute(string name, uint color) : this(name, (NColor)color)
        {

        }
    }
}
