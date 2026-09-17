using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Nextension
{
    public struct NRandom64
    {
        public ulong state;

        public NRandom64(ReadOnlySpan<char> seed)
        {
            ulong h = 0x9E3779B97F4A7C15UL;
            for (int i = 0; i < seed.Length; i++)
            {
                h = (h ^ seed[i]) * 0x100000001B3UL;
            }
            state = h.mix();
        }

        public NRandom64(ReadOnlySpan<char> seed, long index)
        {
            ulong h = 0x9E3779B97F4A7C15UL;
            for (int i = 0; i < seed.Length; i++)
            {
                h = (h ^ seed[i]) * 0x100000001B3UL;
            }
            var uindex = Unsafe.As<long, ulong>(ref index);
            state = h.mix(uindex).mix();
        }

        public NRandom64(uint seed) => state = seed;
        public NRandom64(int seed) => state = Unsafe.As<int, uint>(ref seed);
        public NRandom64(long seed) => state = Unsafe.As<long, ulong>(ref seed);
        public NRandom64(ulong seed) => state = seed;

        public static NRandom64 FromIndex(long seed, long index)
            => FromIndex(Unsafe.As<long, ulong>(ref seed), Unsafe.As<long, ulong>(ref index));

        public static NRandom64 FromIndex(long seed, ulong index)
            => FromIndex(Unsafe.As<long, ulong>(ref seed), index);

        public static NRandom64 FromIndex(ulong seed, long index)
            => FromIndex(seed, Unsafe.As<long, ulong>(ref index));

        public static NRandom64 FromIndex(ulong seed, ulong index)
        {
            return new NRandom64 { state = seed.mix(index).mix() };
        }

        public ulong NextULong()
        {
            state += 0x9E3779B97F4A7C15UL;
            return state.mix();
        }

        public uint NextUInt() => (uint)(NextULong() >> 32);

        public uint NextUInt(uint max) => (uint)((NextUInt() * (ulong)max) >> 32);

        public uint NextUInt(uint min, uint max)
        {
            __checkMinMax(min, (ulong)max);
            return min + NextUInt(max - min);
        }

        public int NextInt() => (int)(NextULong() >> 33);

        public int NextInt(int max)
        {
            __checkMax(max);
            return (int)NextUInt((uint)max);
        }

        public int NextInt(int min, int max)
        {
            __checkMinMax(min, max);
            return min + (int)NextUInt((uint)(max - min));
        }

        public ulong NextULong(ulong max) => max == 0 ? 0 : NextULong() % max;

        public ulong NextULong(ulong min, ulong max)
        {
            __checkMinMax(min, max);
            return min + NextULong(max - min);
        }

        public long NextLong() => (long)(NextULong() >> 1);

        public long NextLong(long max)
        {
            __checkMax(max);
            return (long)NextULong((ulong)max);
        }

        public long NextLong(long min, long max)
        {
            __checkMinMax(min, max);
            return min + (long)NextULong((ulong)(max - min));
        }

        public float NextFloat() => (NextULong() >> 40) * (1.0f / 16777216.0f);

        public float NextFloat(float max) => NextFloat() * max;

        public float NextFloat(float min, float max) => min + NextFloat() * (max - min);

        public bool NextBool() => (NextULong() & 1UL) == 1UL;

        public bool NextRate100(int rate) => NextInt(0, 100) < rate;

        public bool NextRate100(float rate) => NextInt(0, 10000) < (int)(rate * 100f);

        public bool NextRate01(float rate) => NextFloat() < rate;

        [Conditional("UNITY_EDITOR")]
        private static void __checkMax(long max)
        {
            if (max < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(max), "max must be non-negative");
            }
        }

        [Conditional("UNITY_EDITOR")]
        private static void __checkMinMax(long min, long max)
        {
            if (min > max)
            {
                throw new ArgumentOutOfRangeException(nameof(max), "max must be greater than or equal to min");
            }
        }

        [Conditional("UNITY_EDITOR")]
        private static void __checkMinMax(ulong min, ulong max)
        {
            if (min > max)
            {
                throw new ArgumentOutOfRangeException(nameof(max), "max must be greater than or equal to min");
            }
        }
    }
}
