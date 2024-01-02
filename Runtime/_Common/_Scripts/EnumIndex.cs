using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Nextension
{
    public static class EnumIndex<T> where T : unmanaged, Enum
    {
        private static T[] _indexToEnumTable;
        internal static T[] IndexToEnumTable
        {
            get
            {
                if (_indexToEnumTable == null)
                {
                    createTable();
                }
                return _indexToEnumTable;
            }
        }
        private static Dictionary<T, int> _enumToIndexTable;
        internal static Dictionary<T, int> EnumToIndexTable
        {
            get
            {
                if (_enumToIndexTable == null)
                {
                    createTable();
                }
                return _enumToIndexTable;
            }
        }

#if UNITY_EDITOR
        private static uint _hash;
        internal static int Hash
        {
            get
            {
                if (_enumToIndexTable == null)
                {
                    createTable();
                }
                return NConverter.bitConvertWithoutChecks<uint, int>(_hash);
            }
        }
#endif

        private static void createTable()
        {
            _indexToEnumTable = Enum.GetValues(typeof(T)) as T[];

            Array.Sort(_indexToEnumTable);

#if UNITY_EDITOR
            _hash = (uint)NUtils.sizeOf<T>();
#endif
            int enumCount = _indexToEnumTable.Length;
            _enumToIndexTable = new Dictionary<T, int>(enumCount);
            for (int i = 0; i < enumCount; ++i)
            {
                _enumToIndexTable[_indexToEnumTable[i]] = i;
#if UNITY_EDITOR
                uint seed = NConverter.bitConvertWithoutChecks<T, uint>(_indexToEnumTable[i]);
                if (seed == 0) seed = 0x6E624EB7u;
                _hash ^= new Unity.Mathematics.Random(seed).state;
#endif
            }

#if UNITY_EDITOR
            _hash ^= (uint)_indexToEnumTable.Length;
#endif
        }

        public static int EnumCount => IndexToEnumTable.Length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int getIndex(T enumType)
        {
            if (EnumToIndexTable.TryGetValue(enumType, out var index))
            {
                return index;
            }
            return -1;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T getEnum(int index)
        {
            return IndexToEnumTable[index];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int getCount()
        {
            return IndexToEnumTable.Length;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T getRandomEnum(uint seed = 0)
        {
            return IndexToEnumTable.randItem(seed);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> asReadOnlySpan()
        {
            return IndexToEnumTable;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> asSpan()
        {
            return IndexToEnumTable;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool isValid(T enumType)
        {
            return EnumToIndexTable.ContainsKey(enumType);
        }
    }
}
