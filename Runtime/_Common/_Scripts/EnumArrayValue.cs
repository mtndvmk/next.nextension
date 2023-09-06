using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Nextension
{
    public static class EnumIndex<T> where T : Enum
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
            var list = new List<T>();
            var enumArr = Enum.GetValues(typeof(T));
            for (int i = 0; i < enumArr.Length; ++i)
            {
                var enumType = (T)enumArr.GetValue(i);
                list.Add(enumType);
            }
            list.Sort();
            _indexToEnumTable = list.ToArray();
            _enumToIndexTable = new Dictionary<T, int>();
            for (int i = 0; i < list.Count; ++i)
            {
                _enumToIndexTable[list[i]] = i;
            }
        }

        public static int getIndex(T enumType)
        {
            if (EnumToIndexTable.TryGetValue(enumType, out var index))
            {
                return index;
            }
            return -1;
        }
        public static T getEnum(int index)
        {
            return IndexToEnumTable[index];
        }
        public static int getCount()
        {
            return IndexToEnumTable.Length;
        }
        public static T getRandomEnum()
        {
            return IndexToEnumTable.randItem();
        }
        public static IEnumerable<T> asEnumerable()
        {
            return IndexToEnumTable.AsEnumerable<T>();
        }
        public static bool isValid(T enumType)
        {
            return EnumToIndexTable.ContainsKey(enumType);
        }
    }

    public interface IEnumArrayValue
    {
        int Length { get; }
        int EnumCount { get; }
        Type getTypeOfEnum();
        Type getTypeOfValue();
        object getEnumAtIndex(int index);
        object getValueAtIndex(int index);

#if UNITY_EDITOR
        object getEditorCache();
        void applyEditorCache(object cache);
        void refreshEditorCache();
#endif
    }

    [Serializable]
    public class EnumArrayValue<TEnum, TValue> : IEnumArrayValue where TEnum : Enum
    {
        [SerializeField] private TValue[] enumValues = new TValue[EnumIndex<TEnum>.getCount()];
        [SerializeField] private TEnum[] enumArrayCache;

#if UNITY_EDITOR
        public object getEditorCache()
        {
            if (enumArrayCache == null || enumArrayCache.Length == 0)
            {
                refreshEditorCache();
            }

            Dictionary<TEnum, TValue> dict = new Dictionary<TEnum, TValue>();
            for (int i = 0; i < enumArrayCache.Length; ++i)
            {
                var k = enumArrayCache[i];
                if (!EnumIndex<TEnum>.isValid(k) || i >= enumValues.Length) continue;
                dict[k] = enumValues[i];
            }
            return dict;
        }
        public void refreshEditorCache()
        {
            if (enumArrayCache != EnumIndex<TEnum>.IndexToEnumTable)
            {
                enumArrayCache = EnumIndex<TEnum>.IndexToEnumTable;
            }
        }
        public void applyEditorCache(object cache)
        {
            var dict = (Dictionary<TEnum, TValue>)cache;
            if (dict != null)
            {
                foreach (var item in dict)
                {
                    set(item.Key, item.Value);
                }
            }
        }
#endif
        public int Length => enumValues.Length;
        public int EnumCount => EnumIndex<TEnum>.getCount();
        public EnumArrayValue()
        {

        }
        public EnumArrayValue(params (TEnum, TValue)[] enumValues)
        {
            for (int i = 0; i < enumValues.Length; ++i)
            {
                this[enumValues[i].Item1] = enumValues[i].Item2;
            }
        }

        public TValue get(TEnum enumType)
        {
            return enumValues[EnumIndex<TEnum>.getIndex(enumType)];
        }
        public void set(TEnum enumType, TValue val)
        {
            var index = EnumIndex<TEnum>.getIndex(enumType);
            if (index < 0) return;
            enumValues[index] = val;
        }
        public TValue this[TEnum enumType]
        {
            get => get(enumType);
            set => set(enumType, value);
        }
        public IEnumerable<TValue> enumerateValues()
        {
            for (int i = 0; i < Length; ++i)
            {
                yield return enumValues[i];
            }
        }
        public IEnumerable<(TEnum enumType, TValue value)> enumerateTupleValues()
        {
            for (int i = 0; i < Length; ++i)
            {
                yield return (EnumIndex<TEnum>.getEnum(i), enumValues[i]);
            }
        }
        public EnumListValue<TEnum, TValue> toEnumListValue(bool isIgnoreDefaultValue = false)
        {
            EnumListValue<TEnum, TValue> enumListValue = new EnumListValue<TEnum, TValue>();
            foreach (var e in enumerateTupleValues())
            {
                if (isIgnoreDefaultValue && object.Equals(e.value, default)) continue;
                enumListValue.set(e.enumType, e.value);
            }
            return enumListValue;
        }
        public TValue[] toArray()
        {
            return enumValues.ToArray();
        }

        public Type getTypeOfEnum()
        {
            return typeof(TEnum);
        }
        public Type getTypeOfValue()
        {
            return typeof(TValue);
        }
        public object getEnumAtIndex(int index)
        {
            return getEnum(index);
        }
        public object getValueAtIndex(int index)
        {
            return enumValues[index];
        }

        public TValue this[int index]
        {
            get => enumValues[index];
            set => enumValues[index] = value;
        }
        public TEnum getEnum(int index)
        {
            return EnumIndex<TEnum>.getEnum(index);
        }
        public EnumArrayValue<TEnum, TValue> clone()
        {
            return new EnumArrayValue<TEnum, TValue>() { enumValues = this.enumValues.ToArray() };
        }
        public void set(EnumListValue<TEnum, TValue> listValue)
        {
            foreach (var e in listValue.enumerateTupleValues())
            {
                set(e.enumType, e.value);
            }
        }
        public void set(EnumArrayValue<TEnum, TValue> arrValue)
        {
            Array.Copy(arrValue.enumValues, enumValues, Length);
        }
        public static EnumArrayValue<TEnum, TValue> createFrom(EnumListValue<TEnum, TValue> listValue)
        {
            EnumArrayValue<TEnum, TValue> enumArray = new EnumArrayValue<TEnum, TValue>();
            foreach (var e in listValue.enumerateTupleValues())
            {
                enumArray.set(e.enumType, e.value);
            }
            return enumArray;
        }
    }
}
