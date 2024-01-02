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

        public byte estNumberBytesLength()
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
            var num = _value;
            if (num == 0)
            {
                return new byte[1] { 0 };
            }
            if (num == long.MinValue)
            {
                return new byte[1] { byte.MaxValue };
            }

            int isNegative = (num < 0) ? 128 : 0;
            num = Math.Abs(num);

            var srcBytes = stackalloc byte[8];
            *(long*)srcBytes = num;

            int numLength = estNumberBytesLength();

            byte[] result = new byte[numLength + 1];
            result[0] = (byte)(numLength + isNegative);
            fixed (byte* dst = &result[1])
            {
                Buffer.MemoryCopy(srcBytes, dst, numLength, numLength);
            }
            return result;
        }
        public unsafe void writeTo(byte[] buffer, byte estNumBytesLength, ref int startIndex)
        {
            if (_value == 0)
            {
                buffer[startIndex++] = 0;
            }
            else if (_value == long.MinValue)
            {
                buffer[startIndex++] = byte.MaxValue;
            }
            else
            {
                var srcBytes = stackalloc byte[8];
                *(long*)srcBytes = Math.Abs(_value);
                if (_value < 0)
                {
                    buffer[startIndex++] = (byte)(estNumBytesLength + 128);
                }
                else
                {
                    buffer[startIndex++] = estNumBytesLength;
                }
                fixed (byte* dst = &buffer[startIndex])
                {
                    Buffer.MemoryCopy(srcBytes, dst, estNumBytesLength, estNumBytesLength);
                }
                startIndex += estNumBytesLength;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NInteger fromBytes(byte[] bytes, int startIndex = 0)
        {
            return fromBytes(bytes, ref startIndex);
        }
        public unsafe static NInteger fromBytes(byte[] bytes, ref int startIndex)
        {
            var firstNum = bytes[startIndex++];
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
                int numLength = firstNum - 128;
                var dstBytes = stackalloc byte[8];
                fixed (byte* srcBytes = &bytes[startIndex])
                {
                    Buffer.MemoryCopy(srcBytes, dstBytes, numLength, numLength);
                }
                startIndex += numLength;
                return -*(long*)dstBytes;
            }
            else
            {
                int numLength = firstNum;
                var dstBytes = stackalloc byte[8];
                fixed (byte* srcBytes = &bytes[startIndex])
                {
                    Buffer.MemoryCopy(srcBytes, dstBytes, numLength, numLength);
                }
                startIndex += numLength;
                return *(long*)dstBytes;
            }
        }
    }
}