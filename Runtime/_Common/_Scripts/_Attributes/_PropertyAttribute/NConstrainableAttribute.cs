using System;
using UnityEngine;

namespace Nextension
{
    [AttributeUsage(AttributeTargets.Field)]
    public class NConstrainableAttribute : PropertyAttribute 
    {
        public NConstrainableAttribute() 
        {
            this.order = NAttributeOrder.CONSTRAINABLE;
        }
    }
}
