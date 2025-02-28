using System;
using UnityEngine;

namespace Nextension
{
    [AttributeUsage(AttributeTargets.Field)]
    public class NReadOnlyAttribute : PropertyAttribute 
    {
        public NReadOnlyAttribute() 
        {
            order = 1;
        }
    }
}
