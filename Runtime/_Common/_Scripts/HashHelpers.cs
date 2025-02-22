// mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// System.Collections.HashHelpers
using System;

namespace Nextension
{
    internal static class HashHelpers
    {
        public static readonly int[] primes = new int[72]
        {
        3, 7, 11, 17, 23, 29, 37, 47, 59, 71,
        89, 107, 131, 163, 197, 239, 293, 353, 431, 521,
        631, 761, 919, 1103, 1327, 1597, 1931, 2333, 2801, 3371,
        4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591, 17519, 21023,
        25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363,
        156437, 187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403,
        968897, 1162687, 1395263, 1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559,
        5999471, 7199369
        };

        public static bool IsPrime(int candidate)
        {
            if (((uint)candidate & (true ? 1u : 0u)) != 0)
            {
                var num = (int)Math.Sqrt(candidate);
                for (int i = 3; i <= num; i += 2)
                {
                    if (candidate % i == 0)
                    {
                        return false;
                    }
                }
                return true;
            }
            return candidate == 2;
        }

        public static int GetPrime(uint min)
        {
            for (int i = 0; i < 72; i++)
            {
                var num = HashHelpers.primes[i];
                if (num >= min)
                {
                    return num;
                }
            }
            for (int j = (int)min | 1; j < int.MaxValue; j += 2)
            {
                if (HashHelpers.IsPrime(j) && (j - 1) % 101 != 0)
                {
                    return j;
                }
            }
            return (int)min;
        }
        public static int GetPrime(int min)
        {
            return HashHelpers.GetPrime((uint)min);
        }
        public static int ExpandPrime(uint oldSize)
        {
            var num = oldSize << 1;
            if (num > 2146435069u && 2146435069 > oldSize)
            {
                return 2146435069;
            }
            return HashHelpers.GetPrime(num);
        }
        public static int ExpandPrime(int oldSize)
        {
            if (oldSize < 0) throw new InvalidOperationException($"{nameof(oldSize)} must be equal or greater than zero");
            return ExpandPrime((uint)oldSize);
        }
    }
}