using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Nextension
{
    [Serializable]
    public class EnumListValue<T1, T2> where T1 : Enum
    {
        [Serializable]
        private struct EnumValue : IComparable<EnumValue>
        {
            public T1 enumType;
            public T2 value;

            public int CompareTo(EnumValue other)
            {
                return enumType.CompareTo(other);
            }
        }
        [Serializable]
        private class EnumValueBList : AbsBListCompareable<EnumValue, T1>
        {
            protected override T1 getCompareKeyFromValue(EnumValue item)
            {
                return item.enumType;
            }
            public EnumValueBList() : base() { }
            public EnumValueBList(IEnumerable<EnumValue> values) : base(values) { }
        }

        [SerializeField] private EnumValueBList enumValueList = new EnumValueBList();

        public int Count => enumValueList.Count;
        public void set(T1 enumType, T2 val)
        {
            var index = enumValueList.bFindIndex(enumType);
            if (index >= 0)
            {
                var enumValue = enumValueList[index];
                enumValue.value = val;
                enumValueList[index] = enumValue;
            }
            else
            {
                enumValueList.addAndSort(new EnumValue() { enumType = enumType, value = val });
            }        
        }
        public T2 get(T1 enumType)
        {
            if (tryGet(enumType, out var val))
            {
                return val;
            }
            throw new Exception($"Not found value of enum [{enumType}]");
        }
        public bool tryGet(T1 enumType, out T2 outValue)
        {
            var index = enumValueList.bTryGetValue(enumType, out var val);
            if (index >= 0)
            {
                outValue = val.value;
                return true;
            }
            else
            {
                outValue = default;
                return false;
            }
        }
        public bool isContain(T1 enumType)
        {
            return enumValueList.bFindIndex(enumType) >= 0;
        }
        public bool remove(T1 enumType)
        {
            return enumValueList.remove(enumType);
        }
        public T2 this[T1 enumType]
        {
            get => get(enumType);
            set => set(enumType, value);
        }
        public (T1 enumType, T2 value) this[int index]
        {
            get => (enumValueList[index].enumType, enumValueList[index].value);
        }
        public IEnumerable<(T1 enumType, T2 value)> enumerateTupleValues()
        {
            foreach (var e in enumValueList.asEnumerable())
            {
                yield return (e.enumType, e.value);
            }
        }
        public EnumArrayValue<T1, T2> toEnumArrayValue()
        {
            return EnumArrayValue<T1, T2>.createFrom(this);
        }
        public IEnumerable<T2> enumerateValues()
        {
            for (int i = 0; i < Count; ++i)
            {
                yield return enumValueList[i].value;
            }
        }
        public T2[] asArray()
        {
            return enumerateValues().ToArray();
        }
    }
}
