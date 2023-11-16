using System;
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
        object getEnumAtIndex(int index);
        object getValueAtIndex(int index);

#if UNITY_EDITOR
        object getEditorCache();
        void applyEditorCache(object cache);
        void refreshEditorCache();
#endif
    }

    [Serializable]
    public class EnumArrayValue<TEnum, TValue> : IEnumArrayValue, ISerializationCallbackReceiver where TEnum : unmanaged, Enum
    {
        [SerializeField] private TValue[] enumValues = new TValue[EnumIndex<TEnum>.getCount()];
#pragma warning disable 0414
        [SerializeField] private TEnum[] enumArrayCache;
#pragma warning restore 0414

        #region Editor
#if UNITY_EDITOR
        public object getEditorCache()
        {
            if (enumArrayCache == null || enumArrayCache.Length == 0)
            {
                refreshEditorCache();
            }

            Dictionary<TEnum, TValue> dict = new(enumArrayCache.Length);
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
        #endregion

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TValue> enumerateValues()
        {
            return enumValues;
        }
        public IEnumerable<(TEnum enumType, TValue value)> enumerateTupleValues()
        {
            int length = enumValues.Length;
            for (int i = 0; i < length; ++i)
            {
                yield return (EnumIndex<TEnum>.getEnum(i), enumValues[i]);
            }
        }
        public EnumListValue<TEnum, TValue> toEnumListValue(bool isIgnoreDefaultValue = false)
        {
            EnumListValue<TEnum, TValue> enumListValue = new();
            if (isIgnoreDefaultValue)
            {
                foreach (var (enumType, value) in enumerateTupleValues())
                {
                    if (value.equals(default)) continue;
                    enumListValue.set(enumType, value);
                }
            }
            else
            {
                foreach (var (enumType, value) in enumerateTupleValues())
                {
                    enumListValue.set(enumType, value);
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
        public object getEnumAtIndex(int index)
        {
            return getEnum(index);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object getValueAtIndex(int index)
        {
            return enumValues[index];
        }

        public TValue this[int index]
        {
            get => enumValues[index];
            set => enumValues[index] = value;
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
    }
}
