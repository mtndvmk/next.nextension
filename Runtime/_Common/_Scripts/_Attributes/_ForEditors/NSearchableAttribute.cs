using System;
using UnityEngine;

namespace Nextension
{
    [AttributeUsage(AttributeTargets.Field)]
    public class NSearchableAttribute : PropertyAttribute
    {
        public enum NSearchType
        {
            DEFAULT,
            TYPE_AS_STRING
        }
        public readonly NSearchType searchType;
        public Type baseType;

        public NSearchableAttribute(NSearchType searchType = NSearchType.DEFAULT, Type baseType = null)
        {
            this.searchType = searchType;
            this.baseType = baseType;
        }
    }
}
