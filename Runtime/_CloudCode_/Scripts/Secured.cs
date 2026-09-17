using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Nextension
{
    public struct Secured<T> : IEquatable<Secured<T>> where T : unmanaged
    {
        private T _value;
        private uint _key;

        [ThreadStatic]
        private static bool __rndInitialized;
        [ThreadStatic]
        private static NRandom64 __rnd;

        private static uint __nextRand()
        {
            if (!__rndInitialized)
            {
                __rnd = NRandom64.FromIndex(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), Environment.CurrentManagedThreadId);
                __rndInitialized = true;
            }
            return __rnd.NextUInt(1, uint.MaxValue);
        }

        public Secured(T value)
        {
            _key = __nextRand();
            _value = value;
            __crypt(ref _value, _key);
        }

        public T Value
        {
            readonly get
            {
                T val = _value;
                __crypt(ref val, _key);
                return val;
            }
            set
            {
                _key = __nextRand();
                _value = value;
                __crypt(ref _value, _key);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void __crypt(ref T val, uint key)
        {
            if (Unsafe.SizeOf<T>() == 4)
            {
                uint v = Unsafe.As<T, uint>(ref val);
                v ^= key;
                val = Unsafe.As<uint, T>(ref v);
            }
            else if (Unsafe.SizeOf<T>() == 1)
            {
                byte v = Unsafe.As<T, byte>(ref val);
                v ^= (byte)key;
                val = Unsafe.As<byte, T>(ref v);
            }
            else if (Unsafe.SizeOf<T>() == 8)
            {
                ulong v = Unsafe.As<T, ulong>(ref val);
                ulong key64 = ((ulong)key << 32) | key;
                v ^= key64;
                val = Unsafe.As<ulong, T>(ref v);
            }
            else if (Unsafe.SizeOf<T>() == 2)
            {
                ushort v = Unsafe.As<T, ushort>(ref val);
                v ^= (ushort)key;
                val = Unsafe.As<ushort, T>(ref v);
            }
            else
            {
                throw new InvalidOperationException($"Type {typeof(T)} is not supported");
            }
        }

        public static implicit operator T(Secured<T> secured) => secured.Value;

        public static implicit operator Secured<T>(T value) => new Secured<T>(value);

        public override readonly string ToString() => Value.ToString();

        public readonly override bool Equals(object obj)
        {
            if (obj is Secured<T> secured) return Equals(secured);
            if (obj is T val) return EqualityComparer<T>.Default.Equals(Value, val);
            return false;
        }

        public readonly bool Equals(Secured<T> other) => EqualityComparer<T>.Default.Equals(Value, other.Value);

        public override int GetHashCode() => Value.GetHashCode();

        public static bool operator ==(Secured<T> left, Secured<T> right) => left.Equals(right);
        public static bool operator !=(Secured<T> left, Secured<T> right) => !left.Equals(right);
    }
}