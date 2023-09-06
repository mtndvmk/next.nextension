using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Nextension
{
    [Serializable]
    public class EnumListValue<TEnum, TValue> where TEnum : Enum
    {
        [Serializable]
        private struct EnumValue : IComparable<EnumValue>
        {
            public TEnum enumType;
            public TValue value;

            public int CompareTo(EnumValue other)
            {
                return enumType.CompareTo(other);
            }
        }
        [Serializable]
        private class EnumValueBList : AbsBListCompareable<EnumValue, TEnum>
        {
            protected override TEnum getCompareKeyFromValue(EnumValue item)
            {
                return item.enumType;
            }
            public EnumValueBList() : base() { }
            public EnumValueBList(IEnumerable<EnumValue> values) : base(values) { }
        }

        [SerializeField] private EnumValueBList enumValueList = new EnumValueBList();

        public int Count => enumValueList.Count;
        public void set(TEnum enumType, TValue val)
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
        public TValue get(TEnum enumType)
        {
            if (tryGet(enumType, out var val))
            {
                return val;
            }
            throw new Exception($"Not found value of enum [{enumType}]");
        }
        public bool tryGet(TEnum enumType, out TValue outValue)
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
        public bool contains(TEnum enumType)
        {
            return enumValueList.bFindIndex(enumType) >= 0;
        }
        public bool remove(TEnum enumType)
        {
            return enumValueList.remove(enumType);
        }
        public TValue this[TEnum enumType]
        {
            get => get(enumType);
            set => set(enumType, value);
        }
        public (TEnum enumType, TValue value) this[int index]
        {
            get => (enumValueList[index].enumType, enumValueList[index].value);
        }
        public IEnumerable<(TEnum enumType, TValue value)> enumerateTupleValues()
        {
            foreach (var e in enumValueList.asEnumerable())
            {
                yield return (e.enumType, e.value);
            }
        }
        public EnumArrayValue<TEnum, TValue> toEnumArrayValue()
        {
            return EnumArrayValue<TEnum, TValue>.createFrom(this);
        }
        public IEnumerable<TValue> enumerateValues()
        {
            for (int i = 0; i < Count; ++i)
            {
                yield return enumValueList[i].value;
            }
        }
        public TValue[] asArray()
        {
            return enumerateValues().ToArray();
        }
    }
}
