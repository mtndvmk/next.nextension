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
        private static string _indexString;
        internal static string IndexString => _indexString;
#endif

        private static void createTable()
        {
            indexToEnumTable = Enum.GetValues(typeof(T)) as T[];

            Array.Sort(indexToEnumTable);

            int enumCount = indexToEnumTable.Length;
            enumToIndexTable = new Dictionary<T, int>(enumCount);
            for (int i = 0; i < enumCount; ++i)
            {
                enumToIndexTable[indexToEnumTable[i]] = i;
            }
#if UNITY_EDITOR
            computeIndexString();
#endif
        }

#if UNITY_EDITOR
        private static void computeIndexString()
        {
            var enumArr = indexToEnumTable;
            var cacheIntArray = new int[enumArr.Length];
            for (int i = 0; i < enumArr.Length; i++)
            {
                cacheIntArray[i] = NConverter.bitConvertDiffSize<T, int>(enumArr[i]);
            }
            var bytes = NConverter.convertArray<int, byte>(cacheIntArray);
            _indexString = Convert.ToBase64String(bytes);
        }
#endif

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
