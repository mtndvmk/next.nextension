using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Nextension
{
    public static class EnumIndex<T> where T : unmanaged, Enum
    {
        static EnumIndex()
        {
            createTable();
        }

        internal static T[] indexToEnumTable;
        internal static Dictionary<T, int> enumToIndexTable;

#if UNITY_EDITOR
        private static int _hash;
        internal static int Hash => _hash;
#endif

        private static void createTable()
        {
            indexToEnumTable = Enum.GetValues(typeof(T)) as T[];

            Array.Sort(indexToEnumTable);

#if UNITY_EDITOR
            uint hash = (uint)NUtils.sizeOf<T>();
#endif
            int enumCount = indexToEnumTable.Length;
            enumToIndexTable = new Dictionary<T, int>(enumCount);
            for (int i = 0; i < enumCount; ++i)
            {
                enumToIndexTable[indexToEnumTable[i]] = i;
#if UNITY_EDITOR
                uint seed = NConverter.bitConvertWithoutChecks<T, uint>(indexToEnumTable[i]);
                if (seed == 0) seed = 0x6E624EB7u;
                hash ^= new Unity.Mathematics.Random(seed).state;
#endif
            }

#if UNITY_EDITOR
            _hash = NConverter.bitConvertWithoutChecks<uint, int>(hash ^ (uint)indexToEnumTable.Length);
#endif
        }

        public static int EnumCount => indexToEnumTable.Length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int getIndex(T enumType)
        {
            if (enumToIndexTable.TryGetValue(enumType, out var index))
            {
                return index;
            }
            return -1;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T getEnum(int index)
        {
            return indexToEnumTable[index];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int getCount()
        {
            return indexToEnumTable.Length;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T getRandomEnum(uint seed = 0)
        {
            return indexToEnumTable.randItem(seed);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T getRandomEnum(Unity.Mathematics.Random rand)
        {
            return indexToEnumTable.randItem(rand);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T getRandomEnum(ref Unity.Mathematics.Random rand)
        {
            return indexToEnumTable.randItem(ref rand);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> asReadOnlySpan()
        {
            return indexToEnumTable;
        }
        public static ArrayEnumerator<T> getEnumerator()
        {
            return new ArrayEnumerator<T>(indexToEnumTable);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> asSpan()
        {
            return indexToEnumTable;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool isValid(T enumType)
        {
            return enumToIndexTable.ContainsKey(enumType);
        }
    }
}
