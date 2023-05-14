using System;

namespace Nextension.NByteBase
{
    [AttributeUsage(AttributeTargets.Field)]
    public class NByteBaseableIdAttribute : Attribute
    {
        public byte OrderId { get; private set; }
        public NByteBaseableIdAttribute(byte orderId)
        {
            OrderId = orderId;
        }
    }

    public enum NByteBaseableMode
    {
        ALL,
        NORMAL_FIELD_ONLY,
        ID_FIELD_ONLY,
    }
}
