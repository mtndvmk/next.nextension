using System;

namespace Nextension
{
    public static class NMix64Util
    {
        public static ulong mix(this ulong z)
        {
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }
        public static ulong mix(this string z)
        {
            return mix(z.AsSpan());
        }
        public static ulong mix(this ReadOnlySpan<char> z)
        {
            ulong h = 0x9E3779B97F4A7C15UL;
            for (int i = 0; i < z.Length; i++)
            {
                h = (h ^ z[i]) * 0x100000001B3UL;
            }
            return h.mix();
        }
        public static ulong mix(this ulong z, ulong other)
        {
            return mix((z * 0x9E3779B97F4A7C15UL) ^ (other * 0x6C62272E07BB0142UL));
        }
    }
}