﻿using System;
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
        private static void createTable()
        {
            var enumArr = Enum.GetValues(typeof(T));
            var list = new List<T>(enumArr.Length);
            for (int i = 0; i < enumArr.Length; ++i)
            {
                var enumType = (T)enumArr.GetValue(i);
                list.Add(enumType);
            }
            list.Sort();
            _indexToEnumTable = list.ToArray();
            _enumToIndexTable = new Dictionary<T, int>(_indexToEnumTable.Length);
            for (int i = 0; i < _indexToEnumTable.Length; ++i)
            {
                _enumToIndexTable[_indexToEnumTable[i]] = i;
            }
        }

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
        public static T getRandomEnum()
        {
            return IndexToEnumTable.randItem();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<T> asEnumerable()
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
