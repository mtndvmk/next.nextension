using System;
using UnityEngine;

namespace Nextension
{
    [AttributeUsage(AttributeTargets.Field)]
    public class NShowIfAttribute : PropertyAttribute
    {
        public readonly string predicateName;
        public NShowIfAttribute(string predicateName)
        {
            this.predicateName = predicateName;
        }
    }
}
