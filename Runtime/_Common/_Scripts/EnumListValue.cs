using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Nextension
{
    [Serializable]
    public class EnumListValue<TEnum, TValue> where TEnum : unmanaged, Enum
    {
        [Serializable]
        private struct EnumValue
        {
            public TEnum enumType;
            public TValue value;
        }
        [Serializable]
        private class EnumValueBList : AbsBList<EnumValue, TEnum>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected override TEnum getCompareKeyFromValue(EnumValue item)
            {
                return item.enumType;
            }

            protected override int compareKey(TEnum k1, TEnum k2)
            {
                return k1.compareTo(k2);
            }

            public EnumValueBList() : base() { }
        }

        [SerializeField] private EnumValueBList _enumValueList = new();

        public int Count => _enumValueList.Count;
        public void set(TEnum enumType, TValue val)
        {
            var index = _enumValueList.FindIndex(enumType);
            if (index >= 0)
            {
                var enumValue = _enumValueList[index];
                enumValue.value = val;
                _enumValueList[index] = enumValue;
            }
            else
            {
                _enumValueList.AddAndSort(new EnumValue() { enumType = enumType, value = val });
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
            var index = _enumValueList.TryGetValue(enumType, out var val);
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
            return _enumValueList.Contains(enumType);
        }
        public bool remove(TEnum enumType)
        {
            return _enumValueList.RemoveKey(enumType);
        }
        public TValue this[TEnum enumType]
        {
            get => get(enumType);
            set => set(enumType, value);
        }
        public IEnumerable<KeyValuePair<TEnum, TValue>> enumerateTupleValues()
        {
            foreach (var e in _enumValueList)
            {
                yield return new(e.enumType, e.value);
            }
        }
        public IEnumerable<TValue> enumerateValues()
        {
            for (int i = 0; i < Count; ++i)
            {
                yield return _enumValueList[i].value;
            }
        }
    }
}
