using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nextension
{

    public interface IEnumArrayValue : ISerializedFieldCheckable
    {
        int Length { get; }
        Type getTypeOfEnum();
        Type getTypeOfValue();
        object getEnumAtIndexAsObject(int index);
        object getValueAtIndexAsObject(int index);

#if UNITY_EDITOR
        bool refreshEditorCache();
#endif
        bool ISerializedFieldCheckable.onSerializedChanged(Flag flag)
        {
#if UNITY_EDITOR
            if (flag == Flag.OnLoadOrRecompiled || flag == Flag.OnPreprocessBuild)
            {
                return refreshEditorCache();
            }
#endif
            return false;
        }
    }

    [Serializable]
    public class EnumArrayValue<TEnum, TValue> : IEnumArrayValue, IEnumerable<TValue>, ISerializationCallbackReceiver where TEnum : unmanaged, Enum
    {
        public readonly static EnumArrayValue<TEnum, TValue> Empty = new EnumArrayValue<TEnum, TValue>();

        [SerializeField] private TValue[] enumValues = new TValue[EnumIndex<TEnum>.getCount()];
        [SerializeField] private string indexString;

        #region Editor
#if UNITY_EDITOR
        public bool refreshEditorCache()
        {
            if (NStartRunner.IsPlaying) return false;
            bool isDirty = false;
            if (isDirty || indexString != EnumIndex<TEnum>.IndexString)
            {
                Dictionary<TEnum, TValue> dict = null;
                if (!string.IsNullOrEmpty(indexString))
                {
                    byte[] bytes;
                    if (indexString.StartsWith(':'))
                    {
                        bytes = NUtils.decompressFromDeflateString(indexString);
                    }
                    else
                    {
                        bytes = Convert.FromBase64String(indexString);
                    }

                    var cacheIntArray = NConverter.convertArray<byte, int>(bytes);
                    var cacheCount = cacheIntArray.Length;
                    dict = new(cacheCount);
                    for (int i = 0; i < cacheCount; i++)
                    {
                        var k = NConverter.bitConvertWithoutChecks<int, TEnum>(cacheIntArray[i]);
                        if (!EnumIndex<TEnum>.isValid(k) || i >= enumValues.Length) continue;
                        dict[k] = enumValues[i];
                    }
                }

                indexString = EnumIndex<TEnum>.IndexString;
                enumValues = new TValue[EnumIndex<TEnum>.getCount()];
                if (dict != null)
                {
                    foreach (var item in dict)
                    {
                        set(item.Key, item.Value);
                    }
                }
                return true;
            }
            return isDirty;
        }
#endif
        #endregion

        public int Length => enumValues.Length;
        public EnumArrayValue()
        {

        }
        public EnumArrayValue(EnumArrayValue<TEnum, TValue> src)
        {
            enumValues = src.enumValues.clone();
        }

        public void set(TEnum enumType, TValue val)
        {
            if (this == Empty)
            {
                Debug.LogWarning("Cannot set value to Empty");
                return;
            }
            var index = EnumIndex<TEnum>.getIndex(enumType);
            if (index < 0) return;
            enumValues[index] = val;
        }
        public TValue get(TEnum enumType)
        {
#if UNITY_EDITOR
            refreshEditorCache();
#endif
            return enumValues[EnumIndex<TEnum>.getIndex(enumType)];
        }
        public void setByIndex(int index, TValue val)
        {
            if (this == Empty)
            {
                Debug.LogWarning("Cannot set value to Empty");
                return;
            }
            enumValues[index] = val;
        }
        public TValue getByIndex(int index)
        {
            return enumValues[index];
        }
        public TValue this[TEnum enumType]
        {
            get => get(enumType);
            set => set(enumType, value);
        }
        public TEnum? enumOf(TValue val)
        {
            for (var i = 0; i < enumValues.Length; i++)
            {
                if (enumValues[i].equals(val))
                {
                    return getEnum(i);
                }
            }
            return default;
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
        public TValue[] toArray()
        {
            return enumValues.clone();
        }
        public Type getTypeOfEnum()
        {
            return typeof(TEnum);
        }
        public Type getTypeOfValue()
        {
            return typeof(TValue);
        }
        public object getEnumAtIndexAsObject(int index)
        {
            return getEnum(index);
        }
        public object getValueAtIndexAsObject(int index)
        {
            return enumValues[index];
        }
        public TEnum getEnum(int index)
        {
            return EnumIndex<TEnum>.getEnum(index);
        }
        public EnumArrayValue<TEnum, TValue> clone()
        {
            return new EnumArrayValue<TEnum, TValue>(this);
        }
        public ReadOnlySpan<TEnum> Keys => EnumIndex<TEnum>.asReadOnlySpan();
        public ReadOnlySpan<TValue> Values => enumValues;
        public void OnBeforeSerialize()
        {

        }

        public void OnAfterDeserialize()
        {
#if !UNITY_EDITOR
            indexString = null;
#endif
        }

        public struct Enumerator : IEnumerator<KeyValuePair<TEnum, TValue>>
        {
            public Enumerator(EnumArrayValue<TEnum, TValue> enumArrayValue)
            {
                if (enumArrayValue == null || enumArrayValue.enumValues == null)
                {
                    _enumValues = Empty.enumValues;
                }
                else
                {
                    _enumValues = enumArrayValue.enumValues;
                }
                _current = default;
                _index = 0;
            }

            private readonly TValue[] _enumValues;
            private KeyValuePair<TEnum, TValue> _current;
            private int _index;

            public readonly KeyValuePair<TEnum, TValue> Current => _current;

            readonly object IEnumerator.Current => _current;

            public void Dispose()
            {

            }

            public bool MoveNext()
            {
                if (_index < _enumValues.Length)
                {
                    _current = new(EnumIndex<TEnum>.getEnum(_index), _enumValues[_index]);
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
