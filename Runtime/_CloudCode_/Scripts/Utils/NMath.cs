using System;
using System.Runtime.CompilerServices;

namespace Nextension
{
    public static class NMath
    {
        public static readonly float Epsilon = 1E-45F;

        private static readonly float ApproxEpsilon = Epsilon * 8f;

        private static readonly byte[] __deBruijnTable32 = new byte[32]
        {
            0,  1, 28,  2, 29, 14, 24,  3,
            30, 22, 20, 15, 25, 17,  4,  8,
            31, 27, 13, 23, 21, 19, 16,  7,
            26, 12, 18,  6, 11,  5, 10,  9
        };
        private static readonly byte[] __deBruijnTable64 = new byte[64]
        {
            0,  1, 48,  2, 57, 49, 28,  3,
            61, 58, 50, 42, 38, 29, 17,  4,
            62, 55, 59, 36, 53, 51, 43, 22,
            45, 39, 33, 30, 24, 18, 12,  5,
            63, 47, 56, 27, 60, 41, 37, 16,
            54, 35, 52, 21, 44, 32, 23, 11,
            46, 26, 40, 15, 34, 20, 31, 10,
            25, 14, 19,  9, 13,  8,  7,  6
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long pow(long baseVal, uint exp)
        {
            if (exp == 0) return 1L;

            long result = 1;
            while (true)
            {
                if ((exp & 1) != 0) result *= baseVal;

                exp >>= 1;

                if (exp == 0) break;

                baseVal *= baseVal;
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int nextPOT(int n)
        {
            if (n <= 0) return 1;
            n--;
            n |= n >> 1;
            n |= n >> 2;
            n |= n >> 4;
            n |= n >> 8;
            n |= n >> 16;
            return n + 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool isPOT(int n)
        {
            return (n & (n - 1)) == 0 && n > 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool isPOT(float n)
        {
            int int_n = (int)n;
            if (approximately(n, int_n))
            {
                return false;
            }
            return isPOT(int_n);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool approximately(float a, float b)
        {
            return Math.Abs(b - a) < Math.Max(1E-06f * Math.Max(Math.Abs(a), Math.Abs(b)), ApproxEpsilon);
        }

        public static sbyte getTrailingBit1Index(int value)
        {
            if (value == 0) return -1;

            var lsb = (uint)(value & -value);

            return (sbyte)__deBruijnTable32[(lsb * 0x077CB531U) >> 27];
        }

        public static sbyte getTrailingBit1Index(long value)
        {
            if (value == 0) return -1;

            var lsb = (ulong)(value & -value);

            return (sbyte)__deBruijnTable64[(lsb * 0x03F79D71B4CB0A89UL) >> 58];
        }

        /// <summary>
        /// return true if bit at bitIndex is 1, otherwise return false
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool checkBit1Index(int value, int index)
        {
            return (value & (1 << index)) != 0;
        }
        /// <summary>
        /// return true if bit at bitIndex is 1, otherwise return false
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool checkBit1Index(long value, int index)
        {
            return (value & (1L << index)) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong zigZagEncode(long val)
        {
            return (ulong)((val << 1) ^ (val >> 63));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long zigZagDecode(ulong val)
        {
            return (long)((val >> 1) ^ (0UL - (val & 1)));
        }
    }
}
