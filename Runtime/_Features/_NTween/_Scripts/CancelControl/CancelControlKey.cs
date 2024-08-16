using System.Runtime.CompilerServices;
using UnityEngine;

namespace Nextension.Tween
{
    internal struct CancelControlKey
    {
        public readonly long longKey;
        public override int GetHashCode()
        {
            return longKey.GetHashCode();
        }
        internal CancelControlKey(long key)
        {
            longKey = key;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool isDefault() { return longKey == 0; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static long getLongKey(uint uintKey) => (long)uintKey << 32;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static long getLongKey(Object objKey) => objKey.GetHashCode();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool isObjectKey(long longKey)
        {
            return (longKey & 0xffffffff) != 0;
        }
    }
}