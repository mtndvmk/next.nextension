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
    public class EnumArrayValue<T1, T2> : IEnumArrayValue where T1 : Enum
    {
        [SerializeField] private T2[] enumValues = new T2[EnumIndex<T1>.getCount()];
        [SerializeField] private T1[] enumArrayCache;

#if UNITY_EDITOR
        public object getEditorCache()
        {
            if (enumArrayCache == null || enumArrayCache.Length == 0)
            {
                refreshEditorCache();
            }

            Dictionary<T1, T2> dict = new Dictionary<T1, T2>();
            for (int i = 0; i < enumArrayCache.Length; ++i)
            {
                var k = enumArrayCache[i];
                if (!EnumIndex<T1>.isValid(k) || i >= enumValues.Length) continue;
                dict[k] = enumValues[i];
            }
            return dict;
        }
        public void refreshEditorCache()
        {
            if (enumArrayCache != EnumIndex<T1>.IndexToEnumTable)
            {
                enumArrayCache = EnumIndex<T1>.IndexToEnumTable;
            }
        }
        public void applyEditorCache(object cache)
        {
            var dict = (Dictionary<T1, T2>)cache;
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
        public int EnumCount => EnumIndex<T1>.getCount();
        public EnumArrayValue()
        {

        }
        public EnumArrayValue(params (T1, T2)[] enumValues)
        {
            for (int i = 0; i < enumValues.Length; ++i)
            {
                this[enumValues[i].Item1] = enumValues[i].Item2;
            }
        }

        public T2 get(T1 enumType)
        {
            return enumValues[EnumIndex<T1>.getIndex(enumType)];
        }
        public void set(T1 enumType, T2 val)
        {
            var index = EnumIndex<T1>.getIndex(enumType);
            if (index < 0) return;
            enumValues[index] = val;
        }
        public T2 this[T1 enumType]
        {
            get => get(enumType);
            set => set(enumType, value);
        }
        public IEnumerable<T2> enumerateValues()
        {
            for (int i = 0; i < Length; ++i)
            {
                yield return enumValues[i];
            }
        }
        public IEnumerable<(T1 enumType, T2 value)> enumerateTupleValues()
        {
            for (int i = 0; i < Length; ++i)
            {
                yield return (EnumIndex<T1>.getEnum(i), enumValues[i]);
            }
        }
        public EnumListValue<T1, T2> toEnumListValue(bool isIgnoreDefaultValue = false)
        {
            EnumListValue<T1, T2> enumListValue = new EnumListValue<T1, T2>();
            foreach (var e in enumerateTupleValues())
            {
                if (isIgnoreDefaultValue && object.Equals(e.value, default)) continue;
                enumListValue.set(e.enumType, e.value);
            }
            return enumListValue;
        }
        public T2[] toArray()
        {
            return enumValues.ToArray();
        }

        public Type getTypeOfEnum()
        {
            return typeof(T1);
        }
        public Type getTypeOfValue()
        {
            return typeof(T2);
        }
        public object getEnumAtIndex(int index)
        {
            return getEnum(index);
        }
        public object getValueAtIndex(int index)
        {
            return enumValues[index];
        }

        public T2 this[int index]
        {
            get => enumValues[index];
            set => enumValues[index] = value;
        }
        public T1 getEnum(int index)
        {
            return EnumIndex<T1>.getEnum(index);
        }
        public EnumArrayValue<T1, T2> clone()
        {
            return new EnumArrayValue<T1, T2>() { enumValues = this.enumValues.ToArray() };
        }
        public void set(EnumListValue<T1, T2> listValue)
        {
            foreach (var e in listValue.enumerateTupleValues())
            {
                set(e.enumType, e.value);
            }
        }
        public void set(EnumArrayValue<T1, T2> arrValue)
        {
            Array.Copy(arrValue.enumValues, enumValues, Length);
        }
        public static EnumArrayValue<T1, T2> createFrom(EnumListValue<T1, T2> listValue)
        {
            EnumArrayValue<T1, T2> enumArray = new EnumArrayValue<T1, T2>();
            foreach (var e in listValue.enumerateTupleValues())
            {
                enumArray.set(e.enumType, e.value);
            }
            return enumArray;
        }
    }
}
