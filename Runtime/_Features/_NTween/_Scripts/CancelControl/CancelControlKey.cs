using UnityEngine;

namespace Nextension.Tween
{
    internal struct CancelControlKey
    {
        private readonly long _key;
        public override int GetHashCode()
        {
            return _key.GetHashCode();
        }
        internal CancelControlKey(long key)
        {
            _key = key;
        }
        internal long LongKey => _key;
        internal bool isDefault() { return _key == 0; }
        internal static long getLongKey(uint uintKey) => (long)uintKey << 32;
        internal static long getLongKey(Object objKey) => objKey.GetHashCode();
    }
}