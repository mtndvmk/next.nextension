using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Nextension
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct UKVPair<TKey, TValue> : IEquatable<UKVPair<TKey, TValue>>
        where TKey : unmanaged
        where TValue : unmanaged
    {
        public TKey Key;
        public TValue Value;

        public UKVPair(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

        public readonly void Deconstruct(out TKey key, out TValue value)
        {
            key = Key;
            value = Value;
        }

        public readonly bool Equals(UKVPair<TKey, TValue> other)
        {
            return EqualityComparer<TKey>.Default.Equals(Key, other.Key)
                && EqualityComparer<TValue>.Default.Equals(Value, other.Value);
        }

        public override readonly bool Equals(object obj)
        {
            return obj is UKVPair<TKey, TValue> other && Equals(other);
        }

        public override readonly int GetHashCode()
        {
            return (Key.GetHashCode() * 397) ^ Value.GetHashCode();
        }

        public override readonly string ToString()
        {
            return $"[{Key}, {Value}]";
        }

        public static bool operator ==(UKVPair<TKey, TValue> lhs, UKVPair<TKey, TValue> rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(UKVPair<TKey, TValue> lhs, UKVPair<TKey, TValue> rhs)
        {
            return !lhs.Equals(rhs);
        }
    }
}
