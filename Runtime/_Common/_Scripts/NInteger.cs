using System;
using System.Runtime.CompilerServices;

namespace Nextension
{
    public struct NInteger : IComparable<NInteger>, IEquatable<NInteger>
    {
        private long _value;

        public NInteger(long value)
        {
            _value = value;
        }

        public long Value
        {
            get => _value;
            set => _value = value;
        }

        public override bool Equals(object other)
        {
            if (other == null || other is not NInteger otherInteger)
            {
                return false;
            }
            return _value.Equals(otherInteger._value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(NInteger other)
        {
            return CompareTo(other) == 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator long(NInteger nInteger)
        {
            return nInteger._value;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NInteger(long value)
        {
            return new NInteger(value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NInteger left, NInteger right)
        {
            return left.CompareTo(right) == 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NInteger left, NInteger right)
        {
            return left.CompareTo(right) != 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(NInteger other)
        {
            return _value.CompareTo(other._value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(NInteger left, NInteger right)
        {
            return left.CompareTo(right) < 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(NInteger left, NInteger right)
        {
            return left.CompareTo(right) <= 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(NInteger left, NInteger right)
        {
            return left.CompareTo(right) > 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(NInteger left, NInteger right)
        {
            return left.CompareTo(right) >= 0;
        }

        public byte estNumBytesLength()
        {
            if (_value == 0 || _value == long.MinValue) return 0;
            var num = Math.Abs(_value);
            if (num > uint.MaxValue)
            {
                if (num < 1L << 40) return 5;
                if (num < 1L << 48) return 6;
                if (num < 1L << 56) return 7;
                return 8;
            }
            else
            {
                if (num < 1 << 8) return 1;
                if (num < 1 << 16) return 2;
                if (num < 1 << 24) return 3;
                return 4;
            }
        }
        public unsafe byte[] getBytes()
        {
            if (_value == 0)
            {
                return new byte[1] { 0 };
            }
            if (_value == long.MinValue)
            {
                return new byte[1] { byte.MaxValue };
            }

            var absValue = Math.Abs(_value);

            var srcBytes = stackalloc byte[8];
            *(long*)srcBytes = absValue;

            byte numBytesLength = estNumBytesLength();
            byte firstNum = _value < 0 ? (byte)(numBytesLength | 128) : numBytesLength;

            byte[] result = new byte[numBytesLength + 1];
            result[0] = firstNum;
            fixed (byte* dst = &result[1])
            {
                Buffer.MemoryCopy(srcBytes, dst, numBytesLength, numBytesLength);
            }
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="numBytesLength">numBytesLength = estNumBytesLength()</param>
        /// <param name="startIndex"></param>
        public unsafe void writeTo(byte[] dst, byte numBytesLength, ref int startIndex)
        {
            if (_value == 0)
            {
                dst[startIndex++] = 0;
            }
            else if (_value == long.MinValue)
            {
                dst[startIndex++] = byte.MaxValue;
            }
            else
            {
                var srcBytes = stackalloc byte[8];
                *(long*)srcBytes = Math.Abs(_value);

                byte firstNum;
                if (_value < 0)
                {
                    firstNum = (byte)(numBytesLength | 128);
                }
                else
                {
                   firstNum = numBytesLength;
                }
                dst[startIndex++] = firstNum;
                fixed (byte* dstPtr = &dst[startIndex])
                {
                    Buffer.MemoryCopy(srcBytes, dstPtr, numBytesLength, numBytesLength);
                }
                startIndex += numBytesLength;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NInteger fromBytes(ReadOnlySpan<byte> src, int startIndex = 0)
        {
            return fromBytes(src, ref startIndex);
        }
        public unsafe static NInteger fromBytes(ReadOnlySpan<byte> src, ref int startIndex)
        {
            var firstNum = src[startIndex++];
            if (firstNum == 0)
            {
                return new NInteger(0);
            }
            if (firstNum == byte.MaxValue)
            {
                return new NInteger(long.MinValue);
            }

            if (firstNum > 128)
            {
                int numBytesLength = firstNum & 127;
                var dstBytes = stackalloc byte[8];
                fixed (byte* srcBytes = &src[startIndex])
                {
                    Buffer.MemoryCopy(srcBytes, dstBytes, numBytesLength, numBytesLength);
                }
                startIndex += numBytesLength;
                return -*(long*)dstBytes;
            }
            else
            {
                int numLength = firstNum;
                var dstBytes = stackalloc byte[8];
                fixed (byte* srcBytes = &src[startIndex])
                {
                    Buffer.MemoryCopy(srcBytes, dstBytes, numLength, numLength);
                }
                startIndex += numLength;
                return *(long*)dstBytes;
            }
        }
    }
}