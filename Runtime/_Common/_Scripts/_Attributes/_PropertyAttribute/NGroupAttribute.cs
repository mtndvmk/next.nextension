using System;
using UnityEngine;

namespace Nextension
{
    [AttributeUsage(AttributeTargets.Field)]
    public class NGroupAttribute : ApplyToCollectionPropertyAttribute
    {
        public readonly string groupId;

        public NGroupAttribute(string groupId) : base()
        {
            if (groupId.isNullOrEmpty())
            {
                this.groupId = "Group";
            }
            else
            {
                this.groupId = groupId;
            }
            order = NAttributeOrder.GROUP;
        }
    }
}
