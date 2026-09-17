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
            readonly get => _value;
            set => _value = value;
        }

        public readonly override bool Equals(object other)
        {
            if (other == null || !(other is NInteger))
            {
                return false;
            }
            NInteger otherInteger = (NInteger)other;
            return _value.Equals(otherInteger._value);
        }
        public readonly bool Equals(NInteger other)
        {
            return CompareTo(other) == 0;
        }
        public readonly override int GetHashCode()
        {
            return _value.GetHashCode();
        }
        public static implicit operator long(NInteger nInteger)
        {
            return nInteger._value;
        }
        public static implicit operator NInteger(long value)
        {
            return new NInteger(value);
        }
        public static bool operator ==(NInteger left, NInteger right)
        {
            return left.CompareTo(right) == 0;
        }
        public static bool operator !=(NInteger left, NInteger right)
        {
            return left.CompareTo(right) != 0;
        }
        public readonly int CompareTo(NInteger other)
        {
            return _value.CompareTo(other._value);
        }
        public static bool operator <(NInteger left, NInteger right)
        {
            return left.CompareTo(right) < 0;
        }
        public static bool operator <=(NInteger left, NInteger right)
        {
            return left.CompareTo(right) <= 0;
        }
        public static bool operator >(NInteger left, NInteger right)
        {
            return left.CompareTo(right) > 0;
        }
        public static bool operator >=(NInteger left, NInteger right)
        {
            return left.CompareTo(right) >= 0;
        }

        public static byte estNumPartBytesLength(long value)
        {
            if (value == 0 || value == long.MinValue) return 0;
            ulong num = (ulong)(value < 0 ? -value : value);

            byte length = 1;
            if (num > 0xFFFFFFFF) { length += 4; num >>= 32; }
            if (num > 0xFFFF) { length += 2; num >>= 16; }
            if (num > 0xFF) { length += 1; }

            return length;
        }
        public readonly byte estNumPartBytesLength()
        {
            return estNumPartBytesLength(_value);
        }

        public readonly byte[] getBytes()
        {
            byte numPartBytesLength = estNumPartBytesLength(_value);
            byte[] result = new byte[numPartBytesLength + 1];
            int startIndex = 0;
            writeTo(result, ref startIndex);
            return result;
        }
        public readonly unsafe void writeTo(byte[] dst, ref int startIndex)
        {
            byte* dstPtr = (byte*)Unsafe.AsPointer(ref dst[0]);
            writeTo(dstPtr, ref startIndex);
        }
        public readonly unsafe void writeTo(byte* dst, ref int startIndex)
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
                ulong absValue = (ulong)(_value < 0 ? -_value : _value);
                byte numPartBytesLength = estNumPartBytesLength(_value);
                dst[startIndex++] = _value < 0 ? (byte)(numPartBytesLength | 128) : numPartBytesLength;

                byte* dstPtr = dst + startIndex;
                switch (numPartBytesLength)
                {
                    case 8:
                        *(ulong*)dstPtr = absValue;
                        break;
                    case 7:
                    case 6:
                    case 5:
                        *(uint*)dstPtr = (uint)absValue;
                        *(uint*)(dstPtr + numPartBytesLength - 4) = (uint)(absValue >> ((numPartBytesLength - 4) * 8));
                        break;
                    case 4:
                        *(uint*)dstPtr = (uint)absValue;
                        break;
                    case 3:
                        *(ushort*)dstPtr = (ushort)absValue;
                        dstPtr[2] = (byte)(absValue >> 16);
                        break;
                    case 2:
                        *(ushort*)dstPtr = (ushort)absValue;
                        break;
                    case 1:
                        *dstPtr = (byte)absValue;
                        break;
                }

                startIndex += numPartBytesLength;
            }
        }

        public readonly unsafe void writeTo(NBytesWriter writer)
        {
            var numPartBytesLength = estNumPartBytesLength(_value);
            var resultBytes = stackalloc byte[numPartBytesLength + 1];
            int startIndex = 0;
            writeTo(resultBytes, ref startIndex);
            writer.writeBytesWithoutLength(new Span<byte>(resultBytes, numPartBytesLength + 1));
        }

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

            int numPartBytesLength = firstNum > 128 ? firstNum & 127 : firstNum;
            ulong absValue = 0;

            byte* srcPtr = (byte*)Unsafe.AsPointer(ref Unsafe.AsRef(in src[startIndex]));
            switch (numPartBytesLength)
            {
                case 8:
                    absValue = *(ulong*)srcPtr;
                    break;
                case 7:
                case 6:
                case 5:
                    absValue = *(uint*)srcPtr | ((ulong)*(uint*)(srcPtr + numPartBytesLength - 4) << ((numPartBytesLength - 4) * 8));
                    break;
                case 4:
                    absValue = *(uint*)srcPtr;
                    break;
                case 3:
                    absValue = *(ushort*)srcPtr | ((ulong)srcPtr[2] << 16);
                    break;
                case 2:
                    absValue = *(ushort*)srcPtr;
                    break;
                case 1:
                    absValue = *srcPtr;
                    break;
            }

            startIndex += numPartBytesLength;
            return firstNum > 128 ? -(long)absValue : (long)absValue;
        }
    }
}