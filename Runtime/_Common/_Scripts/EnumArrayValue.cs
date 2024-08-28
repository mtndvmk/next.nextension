using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Nextension
{

    public interface IEnumArrayValue
    {
        int Length { get; }
        int EnumCount { get; }
        Type getTypeOfEnum();
        Type getTypeOfValue();
        object getEnumAtIndexAsObject(int index);
        object getValueAtIndexAsObject(int index);

#if UNITY_EDITOR
        bool refreshEditorCache();
#endif
    }

    [Serializable]
    public class EnumArrayValue<TEnum, TValue> : IEnumArrayValue, IEnumerable<TValue>, ISerializationCallbackReceiver where TEnum : unmanaged, Enum
    {
        [SerializeField] private TValue[] enumValues = new TValue[EnumIndex<TEnum>.getCount()];
#pragma warning disable 0414
        [SerializeField] private int[] enumArrayCache;
        [SerializeField] private int hash;
#pragma warning restore 0414

        #region Editor
#if UNITY_EDITOR
        public bool refreshEditorCache()
        {
            if (NStartRunner.IsPlaying) return false;
            if (enumArrayCache == null || enumArrayCache.Length == 0 || hash != EnumIndex<TEnum>.Hash)
            {
                hash = EnumIndex<TEnum>.Hash;

                Dictionary<TEnum, TValue> dict = null;
                int cacheCount = enumArrayCache == null ? 0 : enumArrayCache.Length;
                if (cacheCount > 0)
                {
                    dict = new(cacheCount);
                    for (int i = 0; i < cacheCount; i++)
                    {
                        var k = NConverter.bitConvertWithoutChecks<int, TEnum>(enumArrayCache[i]);
                        if (!EnumIndex<TEnum>.isValid(k) || i >= enumValues.Length) continue;
                        dict[k] = enumValues[i];
                    }
                }

                var enumArr = EnumIndex<TEnum>.indexToEnumTable;
                enumValues = new TValue[enumArr.Length];
                enumArrayCache = new int[enumArr.Length];
                for (int i = 0; i < enumArr.Length; i++)
                {
                    enumArrayCache[i] = NConverter.bitConvertDiffSize<TEnum, int>(enumArr[i]);
                }

                if (dict != null)
                {
                    foreach (var item in dict)
                    {
                        set(item.Key, item.Value);
                    }
                }
                return true;
            }
            return false;
        }
#endif
        #endregion

        public int Length => enumValues.Length;
        public int EnumCount => EnumIndex<TEnum>.getCount();
        public EnumArrayValue()
        {

        }

        public void set(TEnum enumType, TValue val)
        {
            var index = EnumIndex<TEnum>.getIndex(enumType);
            if (index < 0) return;
            enumValues[index] = val;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue getValue(TEnum enumType)
        {
#if UNITY_EDITOR
            refreshEditorCache();
#endif
            return enumValues[EnumIndex<TEnum>.getIndex(enumType)];
        }
        public void setByIndex(int index, TValue val)
        {
            enumValues[index] = val;
        }
        public TValue getValueByIndex(int index)
        {
            return enumValues[index];
        }
        public TValue this[TEnum enumType]
        {
            get => getValue(enumType);
            set => set(enumType, value);
        }
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
        {
            return new ArrayEnumerator<TValue>(enumValues);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ArrayEnumerator<TValue> enumerateValues()
        {
            return new ArrayEnumerator<TValue>(enumValues);
        }
        public EnumListValue<TEnum, TValue> toEnumListValue(bool isIgnoreDefaultValue = false)
        {
            EnumListValue<TEnum, TValue> enumListValue = new();
            if (isIgnoreDefaultValue)
            {
                foreach (var item in this)
                {
                    if (item.Value.equals(default)) continue;
                    enumListValue.set(item.Key, item.Value);
                }
            }
            else
            {
                foreach (var item in this)
                {
                    enumListValue.set(item.Key, item.Value);
                }
            }
            return enumListValue;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue[] toArray()
        {
            return enumValues.ToArray();
        }
        public Span<TValue> asSpan()
        {
            return enumValues.AsSpan();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Type getTypeOfEnum()
        {
            return typeof(TEnum);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Type getTypeOfValue()
        {
            return typeof(TValue);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object getEnumAtIndexAsObject(int index)
        {
            return getEnum(index);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object getValueAtIndexAsObject(int index)
        {
            return enumValues[index];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TEnum getEnum(int index)
        {
            return EnumIndex<TEnum>.getEnum(index);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EnumArrayValue<TEnum, TValue> clone()
        {
            return new EnumArrayValue<TEnum, TValue>() { enumValues = this.enumValues.ToArray() };
        }
        public void set(EnumListValue<TEnum, TValue> listValue)
        {
            foreach (var (enumType, value) in listValue.enumerateTupleValues())
            {
                set(enumType, value);
            }
        }
        public void set(EnumArrayValue<TEnum, TValue> arrValue)
        {
            Array.Copy(arrValue.enumValues, enumValues, Length);
        }
        public static EnumArrayValue<TEnum, TValue> createFrom(EnumListValue<TEnum, TValue> listValue)
        {
            EnumArrayValue<TEnum, TValue> enumArray = new();
            foreach (var (enumType, value) in listValue.enumerateTupleValues())
            {
                enumArray.set(enumType, value);
            }
            return enumArray;
        }

        public void OnBeforeSerialize()
        {

        }

        public void OnAfterDeserialize()
        {
#if !UNITY_EDITOR
            enumArrayCache = null;
#endif
        }

        public struct Enumerator : IEnumerator<KeyValuePair<TEnum, TValue>>
        {
            public Enumerator(EnumArrayValue<TEnum, TValue> enumArrayValue)
            {
                _enumArrayValue = enumArrayValue;
                _current = default;
                _index = 0;
            }

            private readonly EnumArrayValue<TEnum, TValue> _enumArrayValue;
            private KeyValuePair<TEnum, TValue> _current;
            private int _index;

            public KeyValuePair<TEnum, TValue> Current => _current;

            object IEnumerator.Current => _current;

            public void Dispose()
            {
                
            }

            public bool MoveNext()
            {
                if (_index < _enumArrayValue.Length)
                {
                    _current = new(EnumIndex<TEnum>.getEnum(_index), _enumArrayValue.enumValues[_index]);
                    _index++;
                    return true;
                }
                return false;
            }

            public void Reset()
            {
                _index = 0;
                _current = default;
            }

            public Enumerator GetEnumerator()
            {
                return this;
            }
        }
    }
}
