using System;
using System.Collections.Concurrent;

namespace Nextension
{
    public static class CharacterPool
    {
        public const string ALPHABET28 = "976432ZYXWVUTRQPNMLKJHGFEDCA";
        public static string getCharPool(CharPoolType type)
        {
            return type switch
            {
                CharPoolType.ALPHABET28 => ALPHABET28,
                _ => throw new NotImplementedException(),
            };

        }
    }

    public enum CharPoolType
    {
        ALPHABET28 = 28,
    }

    public readonly struct ScrambledBaseData
    {
        public readonly CharPoolType charPoolType;
        public readonly long half;
        public readonly long maxCapacity;
        public readonly long mixMask;

        public ScrambledBaseData(uint length, CharPoolType charPoolType)
        {
            this.charPoolType = charPoolType;
            var charPoolCount = CharacterPool.getCharPool(charPoolType).Length;
            half = NMath.pow(charPoolCount, length / 2);
            maxCapacity = NMath.pow(charPoolCount, length);

            mixMask = 1L;
            while (mixMask < half) mixMask = (mixMask << 1) | 1;
        }
    }

    public static class ScrambledCodeUtil
    {
        private static readonly ConcurrentDictionary<uint, ScrambledBaseData> _baseDataTable = new();

        private static ScrambledBaseData __getBaseData(uint codeLen)
        {
            return _baseDataTable.GetOrAdd(codeLen, len => new ScrambledBaseData(len, CharPoolType.ALPHABET28));
        }

        public static long getMaxNum(uint codeLen)
        {
            if (codeLen < 2 || codeLen % 2 != 0) throw new ArgumentException("Code length must be even and >= 2", nameof(codeLen));
            var data = __getBaseData(codeLen);
            return data.maxCapacity;
        }

        public static string encode(ulong unum, uint codeLen)
        {
            if (codeLen < 2 || codeLen % 2 != 0) throw new ArgumentException("Code length must be even and >= 2", nameof(codeLen));

            var data = __getBaseData(codeLen);

            if (unum < 1 || unum > (ulong)data.maxCapacity) throw new ArgumentOutOfRangeException(nameof(unum), $"Number must be in range 1 to {data.maxCapacity}");

            long x = (long)(unum - 1);
            long right = Math.DivRem(x, data.half, out long left);

            for (uint round = 1; round <= codeLen; round++)
            {
                long nextLeft = right;
                long nextRight = (left + __mix(right, round, data.mixMask, data.half)) % data.half;
                left = nextLeft;
                right = nextRight;
            }

            long value = left * data.half + right;

            var charPool = CharacterPool.getCharPool(data.charPoolType);
            var charPoolCount = charPool.Length;

            Span<char> chars = stackalloc char[(int)codeLen];
            for (int i = (int)codeLen - 1; i >= 0; i--)
            {
                value = Math.DivRem(value, charPoolCount, out long rem);
                chars[i] = charPool[(int)rem];
            }

            return new string(chars);
        }

        public static long decode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Code cannot be null or empty!", nameof(code));

            uint length = (uint)code.Length;
            if (length < 2 || length % 2 != 0) throw new ArgumentException("Code length must be even and >= 2", nameof(code));

            var data = __getBaseData(length);
            var charPool = CharacterPool.getCharPool(data.charPoolType);
            var charPoolCount = charPool.Length;

            // Step 1: Convert string back to the scrambled number
            long value = 0;
            foreach (char c in code)
            {
                int index = charPool.IndexOf(c);
                if (index == -1)
                    throw new ArgumentException($"Character '{c}' is invalid!");

                value = value * charPoolCount + index;
            }

            // Step 2: Unscramble using the inverse Feistel rounds
            long left = Math.DivRem(value, data.half, out long right);

            for (long round = length; round >= 1; round--)
            {
                long rOld = left;
                long lOld = (right - __mix(rOld, round, data.mixMask, data.half)) % data.half;
                if (lOld < 0) lOld += data.half;
                left = lOld;
                right = rOld;
            }

            return right * data.half + left + 1;
        }

        private static long __mix(long h, long round, long mixMask, long half)
        {
            long x = (h ^ (round * 0x9E3779B9L)) & mixMask;
            x = (x ^ (x >> 7)) & mixMask;
            x = (x * 0x45D9F3BL) & mixMask;
            x = (x ^ (x >> 5)) & mixMask;
            return x % half;
        }
    }
}