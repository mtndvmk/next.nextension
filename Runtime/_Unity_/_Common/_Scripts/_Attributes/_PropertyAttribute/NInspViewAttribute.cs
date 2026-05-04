using System;
using UnityEngine;

namespace Nextension
{
    [AttributeUsage(AttributeTargets.Field)]
    public class NInspViewAttribute : PropertyAttribute
    {
        public NInspViewAttribute()
        {
            this.order = NAttributeOrder.INSPECTOR_VIEWABLE;
        }
    }
}
