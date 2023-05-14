using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nextension.NByteBase
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public abstract class NByteBaseIdAttribute : Attribute
    {
        internal const short DEFAULT_ID = short.MinValue;
        private const short MIN_LIMIT = -32767;
        private const short MAX_LIMIT = 32767;
        public short Id { get; private set; } = DEFAULT_ID;
        public string Error { get; protected set; }
        public bool IsError => !string.IsNullOrEmpty(Error);
        public NByteBaseIdAttribute(int id)
        {
            if (id < MIN_LIMIT || id > MAX_LIMIT)
            {
                Error = $"Id must from {MIN_LIMIT} to {MAX_LIMIT}";
            }
            if (IsError)
            {
                throw new Exception(Error);
            }
            Id = Convert.ToInt16(id);
        }
    }
    /// <summary>
    /// Id must from 0 to 32,767
    /// </summary>
    internal class NByteBaseDefaultAttribute : NByteBaseIdAttribute
    {
        public NByteBaseDefaultAttribute(ushort id) : base(-id)
        {
        }
    }
    /// <summary>
    /// Id must from 1 to 32,767
    /// </summary>
    public class NByteBaseCustomAttribute : NByteBaseIdAttribute
    {
        public NByteBaseCustomAttribute(ushort id) : base(id)
        {
            if (id == 0)
            {
                Error = "NByteBaseCustomAttribute Id must greater than 0";
            }
        }
    }

    public abstract class AbsNByteBase
    {
        private short? _id;
        internal short Id
        {
            get
            {
                if (!_id.HasValue)
                {
                    _id = NByteBaseDatabase.getId(GetType());
                }
                return _id.Value;
            }
        }
        public byte[] getBytes()
        {
            var idBytes = NConverter.getBytes(Id);
            var valueBytes = onSerialize();
            var bytes = NUtils.merge(idBytes, valueBytes);
            return bytes;
        }
        public void setBytes(byte[] inData, ref int startIndex)
        {
            _id = NConverter.toInt16(inData, ref startIndex);
            onDeserialize(inData, ref startIndex);
        }
        public abstract object getValue();
        public abstract void setValue(object value);
        public abstract bool isEqual(AbsNByteBase other);

        protected abstract byte[] onSerialize();
        protected abstract void onDeserialize(byte[] inData, ref int startIndex);
        public abstract Type getValueType();

    }
    public abstract class NByteBase<T> : AbsNByteBase
    {
        public NByteBase()
        {

        }
        public NByteBase(T inValue)
        {
            Value = inValue;
        }
        public void setValue(T value)
        {
            this.Value = value;
        }
        public virtual T Value { get; protected set; }

        public override object getValue()
        {
            return Value;
        }
        public override void setValue(object value)
        {
            setValue((T)value);
        }
        public override Type getValueType()
        {
            return typeof(T);
        }
        public override bool isEqual(AbsNByteBase other)
        {
            if (other == null) return false;
            if (getValueType() != other.getValueType()) return false;
            var casted = (NByteBase<T>)other;
            return Value.Equals(casted.Value);
        }
    }
    /// <summary>
    /// Support up to 255 items
    /// </summary>
    public abstract class NListOf<T> : NByteBase<List<T>>
    {
        public NListOf()
        {
            Value = new List<T>();
        }
        protected override void onDeserialize(byte[] inData, ref int startIndex)
        {
            byte count = inData[startIndex++];
            while (Value.Count < count)
            {
                var itemLength = NConverter.toUInt16(inData, ref startIndex);
                var itemBytes = NUtils.getBlock(inData, startIndex, itemLength); startIndex += itemLength;
                var item = onDeserializeItem(itemBytes);
                Value.Add(item);
            }
        }

        protected override byte[] onSerialize()
        {
            byte count = 0;
            byte[] bytes = new byte[0];
            foreach (var item in Value)
            {
                if (item == null) continue;
                var itemBytes = onSerializeItem(item);
                var lengthBytes = NConverter.getBytes(Convert.ToUInt16(itemBytes.Length));
                bytes = NUtils.merge(bytes, lengthBytes, itemBytes);
                count++;
            }
            return NUtils.merge(new byte[] { count }, bytes);
        }
        /// <summary>
        /// Support up to 65,535 bytes
        /// </summary>
        /// <returns></returns>
        protected abstract byte[] onSerializeItem(T item);
        protected abstract T onDeserializeItem(byte[] itemBytes);

        public int Count => Value.Count;
        public void add(T item)
        {
            Value.Add(item);
        }
        public bool remove(T item)
        {
            return Value.Remove(item);
        }
        public bool removeAt(int index)
        {
            if (Count <= index)
            {
                return false;
            }
            Value.RemoveAt(index);
            return true;
        }
        public void clear()
        {
            Value.Clear();
        }
        public T this[byte index]
        {
            get
            {
                return Value[index];
            }
            set
            {
                Value[index] = value;
            }
        }
    }
    [NByteBaseDefault(0)]
    public class NByte : NByteBase<byte>
    {
        public NByte(byte inValue = default) : base(inValue)
        {
        }

        protected override void onDeserialize(byte[] inData, ref int startIndex)
        {
            Value = inData[startIndex];
            startIndex++;
        }

        protected override byte[] onSerialize()
        {
            return new byte[] { Value };
        }
    }

    [NByteBaseDefault(1)]
    public sealed class NInt16 : NByteBase<short>
    {
        public NInt16(short inValue = default) : base(inValue)
        {
        }

        protected override void onDeserialize(byte[] inData, ref int startIndex)
        {
            Value = NConverter.toInt16(inData, ref startIndex);
        }

        protected override byte[] onSerialize()
        {
            return NConverter.getBytes(Value);
        }
    }

    [NByteBaseDefault(2)]
    public sealed class NUInt16 : NByteBase<ushort>
    {
        public NUInt16(ushort inValue = default) : base(inValue)
        {
        }

        protected override void onDeserialize(byte[] inData, ref int startIndex)
        {
            Value = NConverter.toUInt16(inData, ref startIndex);
        }

        protected override byte[] onSerialize()
        {
            return NConverter.getBytes(Value);
        }
    }

    [NByteBaseDefault(3)]
    public sealed class NInt32 : NByteBase<int>
    {
        public NInt32(int inValue = default) : base(inValue)
        {
        }

        protected override void onDeserialize(byte[] inData, ref int startIndex)
        {
            Value = NConverter.toInt32(inData, ref startIndex);
        }

        protected override byte[] onSerialize()
        {
            return NConverter.getBytes(Value);
        }
    }
    
    [NByteBaseDefault(4)]
    public sealed class NUInt32 : NByteBase<uint>
    {
        public NUInt32(uint inValue = default) : base(inValue)
        {
        }

        protected override void onDeserialize(byte[] inData, ref int startIndex)
        {
            Value = NConverter.toUInt32(inData, ref startIndex);
        }

        protected override byte[] onSerialize()
        {
            return NConverter.getBytes(Value);
        }
    }

    [NByteBaseDefault(5)]
    public sealed class NFloat : NByteBase<float>
    {
        public NFloat(float inValue = default) : base(inValue)
        {
        }

        protected override void onDeserialize(byte[] inData, ref int startIndex)
        {
            Value = NConverter.toFloat(inData, ref startIndex);
        }

        protected override byte[] onSerialize()
        {
            return NConverter.getBytes(Value);
        }
    }

    [NByteBaseDefault(6)]
    public sealed class NInt64 : NByteBase<long>
    {
        public NInt64(long inValue = default) : base(inValue)
        {
        }

        protected override void onDeserialize(byte[] inData, ref int startIndex)
        {
            Value = NConverter.toInt64(inData, ref startIndex);
        }

        protected override byte[] onSerialize()
        {
            return NConverter.getBytes(Value);
        }
    }

    [NByteBaseDefault(7)]
    public sealed class NUInt64 : NByteBase<ulong>
    {
        public NUInt64(ulong inValue = default) : base(inValue)
        {
        }

        protected override void onDeserialize(byte[] inData, ref int startIndex)
        {
            Value = NConverter.toUInt64(inData, ref startIndex);
        }

        protected override byte[] onSerialize()
        {
            return NConverter.getBytes(Value);
        }
    }

    /// <summary>
    /// Support up to 65,535 bytes - UTF8 encoding
    /// </summary>
    [NByteBaseDefault(8)]
    public sealed class NString : NByteBase<string>
    {
        public NString(string inValue = default) : base(inValue)
        {
        }

        protected override void onDeserialize(byte[] inData, ref int startIndex)
        {
            var length = NConverter.toUInt16(inData, ref startIndex);
            Value = NConverter.getUTF8String(inData, ref startIndex, length);
        }

        protected override byte[] onSerialize()
        {
            var length = Convert.ToUInt16(Value.Length);
            return NUtils.merge(NConverter.getBytes(length), NConverter.getBytes(Value));
        }
    }
    [NByteBaseDefault(9)]
    public sealed class NByteArray : NByteBase<byte[]>
    {
        public NByteArray(byte[] inValue = default) : base(inValue)
        {
        }

        protected override void onDeserialize(byte[] inData, ref int startIndex)
        {
            var length = NConverter.toUInt16(inData, ref startIndex);
            Value = NUtils.getBlock(inData, startIndex, length);
            startIndex += length;
        }

        protected override byte[] onSerialize()
        {
            var length = Convert.ToUInt16(Value.Length);
            return NUtils.merge(NConverter.getBytes(length), Value);
        }

        public override bool isEqual(AbsNByteBase obj)
        {
            var other = obj as NByteArray;
            if (other == null)
            {
                return false;
            }
            return Value.isSameItem(other.Value);
        }
    }

    /// <summary>
    /// Support up to 255 items
    /// </summary>
    [NByteBaseDefault(10)]
    public class NList : NByteBase<List<AbsNByteBase>>
    {
        public NList()
        {
            Value = new List<AbsNByteBase>();
        }
        protected override void onDeserialize(byte[] inData, ref int startIndex)
        {
            Value = new List<AbsNByteBase>();
            var count = inData[startIndex++];

            while (Value.Count < count)
            {
                var nByteBase = NByteBaseUtils.getNext(inData, ref startIndex);
                Value.Add(nByteBase);
            }
        }

        protected override byte[] onSerialize()
        {
            byte[] bytes = new byte[0];
            byte count = 0;
            for (int i = 0; i < Value.Count; i++)
            {
                if (Value[i] != null)
                {
                    var valueBytes = Value[i].getBytes();
                    bytes = NUtils.merge(bytes, valueBytes);
                    count++;
                }
            }
            bytes = NUtils.merge(new byte[] { count }, bytes);
            return bytes;
        }
        public void add(AbsNByteBase nByteBase)
        {
            Value.Add(nByteBase);
        }
        public bool remove(AbsNByteBase nByteBase)
        {
            return Value.Remove(nByteBase);
        }
        public bool removeAt(int index)
        {
            if (Count <= index)
            {
                return false;
            }
            Value.RemoveAt(index);
            return true;
        }
        public void clear()
        {
            Value.Clear();
        }
        public AbsNByteBase this[byte index]
        {
            get => Value[index];
            set => Value[index] = value;
        }
        public int Count => Value.Count;
    }

    [NByteBaseDefault(11)]
    public class NVector2 : NByteBase<Vector2>
    {
        public NVector2(Vector2 inValue = default) : base(inValue)
        {
        }

        protected override void onDeserialize(byte[] inData, ref int startIndex)
        {
            var x = NConverter.toFloat(inData, ref startIndex);
            var y = NConverter.toFloat(inData, ref startIndex);
            Value = new Vector2(x, y);
        }

        protected override byte[] onSerialize()
        {
            return NUtils.merge(NConverter.getBytes(Value.x), NConverter.getBytes(Value.y));
        }
    }

    [NByteBaseDefault(12)]
    public class NVector3 : NByteBase<Vector3>
    {
        public NVector3(Vector3 inValue = default) : base(inValue)
        {
        }

        protected override void onDeserialize(byte[] inData, ref int startIndex)
        {
            Value = NConverter.toVector3(inData, ref startIndex);
        }

        protected override byte[] onSerialize()
        {
            return NConverter.getBytes(Value);
        }
    }

    [NByteBaseDefault(13)]
    public class NKeyValue : NByteBase<KeyValuePair<AbsNByteBase, AbsNByteBase>>
    {
        public NKeyValue(KeyValuePair<AbsNByteBase, AbsNByteBase> inValue = default) : base(inValue)
        {

        }
        public NKeyValue(AbsNByteBase key, AbsNByteBase value)
        {
            setKeyValue(key, value);
        }

        public AbsNByteBase NKey => Value.Key;
        public AbsNByteBase NValue => Value.Value;

        protected override void onDeserialize(byte[] inData, ref int startIndex)
        {
            var key = NByteBaseUtils.getNext(inData, ref startIndex);
            var value = NByteBaseUtils.getNext(inData, ref startIndex);
            Value = new KeyValuePair<AbsNByteBase, AbsNByteBase>(key, value);
        }

        protected override byte[] onSerialize()
        {
            var kBytes = Value.Key.getBytes();
            var vBytes = Value.Value.getBytes();
            return NUtils.merge(kBytes, vBytes);
        }

        public void setKeyValue(AbsNByteBase key, AbsNByteBase value)
        {
            Value = new KeyValuePair<AbsNByteBase, AbsNByteBase>(key, value);
        }
        public override bool isEqual(AbsNByteBase other)
        {
            var otherKV = other as NKeyValue;
            return otherKV.NKey.isEqual(NKey) && otherKV.NValue.isEqual(NValue);
        }
        public bool isKeyOrValueIsNull()
        {
            return NKey == null || NValue == null;
        }
    }

    /// <summary>
    /// Support up to 255 items
    /// </summary>
    [NByteBaseDefault(14)]
    public class NHashtable : NByteBase<List<NKeyValue>>
    {
        private bool _isOptimized;
        public override List<NKeyValue> Value
        {
            get
            {
                if (base.Value == null)
                {
                    base.Value = new List<NKeyValue>();
                }
                return base.Value;
            }
        }
        protected override void onDeserialize(byte[] inData, ref int startIndex)
        {
            Value = new List<NKeyValue>();
            var isOptimized = inData[startIndex++] == 1;
            var count = inData[startIndex++];

            while (Value.Count < count)
            {
                var nByteBase = NByteBaseUtils.getNext(inData, ref startIndex) as NKeyValue;
                Value.Add(nByteBase);
            }
        }
        protected override byte[] onSerialize()
        {
            byte[] bytes = new byte[0];
            byte optimizeBytes = 0;
            if (_isOptimized)
            {
                optimizeBytes = 1;
                //bytes[0] = 1;
                //var tempList = new List<NKeyValue>();
                //for (int i = Value.Count - 1; i >= 0; i--)
                //{
                //    if (Value[i] != null)
                //    {
                //        tempList.Add(Value[i]);
                //    }
                //}
                //tempList.Sort((a, b) =>
                //{
                //    return a.NKey.Id.CompareTo(b.NKey.Id);
                //});
                //short tId = NByteBaseIdAttribute.DEFAULT_ID;
                //byte tCount = 0;
                //byte[] tBytes = new byte[0];
                //AbsNByteBase tKey = null;
                //for (int i = 0; i < tempList.Count; i++)
                //{
                //    if (tId != tempList[i].Id)
                //    {
                //        if (tCount > 0 && tBytes.Length > 0 && tKey != null)
                //        {
                //            var tIdBytes = NConverter.getBytes(tId);
                //            var tCountBytes = new byte[] {tCount};
                //            bytes = NUtils.merge(bytes, tIdBytes, tCountBytes, tKey.getBytes(), tBytes);
                //        }
                //        tCount = 0;
                //        tBytes = new byte[0];
                //    }
                //    tId = tempList[i].Id;
                //    tCount++;
                //    tBytes = NUtils.merge(tBytes, tempList[i].NValue.getBytes());
                //}
                throw new Exception("Current not supported");
            }
            else
            {
                byte count = 0;
                for (int i = 0; i < Value.Count; i++)
                {
                    if (Value[i] != null && !Value[i].isKeyOrValueIsNull())
                    {
                        var valueBytes = Value[i].getBytes();
                        bytes = NUtils.merge(bytes, valueBytes);
                        count++;
                    }
                }

                bytes = NUtils.merge(new byte[] { optimizeBytes, count }, bytes);
            }
            return bytes;
        }
        public override bool isEqual(AbsNByteBase other)
        {
            var otherHash = other as NHashtable;
            if (otherHash == null)
            {
                return false;
            }
            if (otherHash.Count != Count)
            {
                return false;
            }
            int isEqualCount = 0;

            removeNullOrEmptyKey();

            foreach (var kv in Value)
            {
                if (otherHash[kv.NKey] == null) return false;
                if (!otherHash[kv.NKey].isEqual(kv.NValue))
                {
                    return false;
                }
                else
                {
                    isEqualCount++;
                }
            }
            if (isEqualCount == Count)
            {
                return true;
            }
            return false;
        }
        //public byte[] getBytes(bool isOptimzed)
        //{
        //    _isOptimized = isOptimzed;
        //    return base.getBytes();
        //}

        /// <summary>
        /// set the key to null is equivalent to removing key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public bool set(AbsNByteBase key, AbsNByteBase value)
        {
            if (key == null)
            {
                Debug.LogWarning("Can not set with key is null");
                return false;
            }
            if (value == null)
            {
                remove(key);
                return true;
            }
            else
            {
                for (int i = Value.Count - 1; i >= 0; i--)
                {
                    if (Value[i] == null || Value[i].NKey == null)
                    {
                        Value.RemoveAt(i);
                        continue;
                    }
                    if (Value[i].NKey.isEqual(key))
                    {
                        Value[i] = new NKeyValue(key, value);
                        return true;
                    }
                }
                Value.Add(new NKeyValue(key, value));
                return true;
            }
        }
        public bool remove(AbsNByteBase key)
        {
            if (key == null)
            {
                Debug.LogWarning("Can not remove with key is null");
                return false;
            }
            for (int i = Value.Count - 1; i >= 0; i--)
            {
                if (Value[i] == null || Value[i].NKey == null)
                {
                    Value.RemoveAt(i);
                    continue;
                }
                if (Value[i].NKey.isEqual(key))
                {
                    Value.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }
        public AbsNByteBase get(AbsNByteBase key)
        {
            if (key == null)
            {
                Debug.LogWarning("key is null");
            }
            for (int i = Value.Count - 1; i >= 0; i--)
            {
                if (Value[i] == null)
                {
                    Value.RemoveAt(i);
                    continue;
                }
                if (Value[i].NKey.isEqual(key))
                {
                    return Value[i].NValue;
                }
            }
            return null;
        }
        public AbsNByteBase this[AbsNByteBase key]
        {
            get
            {
                return get(key);
            }
            set
            {
                set(key, value);
            }
        }
        public void removeNullOrEmptyKey()
        {
            for (int i = Value.Count - 1; i >= 0; i--)
            {
                if (Value[i] == null || Value[i].NKey == null)
                {
                    Value.RemoveAt(i);
                    continue;
                }
            }
        }
        public int Count => Value.Count;
    }

    /// <summary>
    /// Support up to 255 items
    /// </summary>
    [NByteBaseDefault(15)]
    public class NIdValuePair : NByteBase<Dictionary<byte, AbsNByteBase>>
    {
        public NIdValuePair()
        {
            Value = new Dictionary<byte, AbsNByteBase>();
        }
        protected override void onDeserialize(byte[] inData, ref int startIndex)
        {
            byte count = inData[startIndex++];
            while (Value.Count < count)
            {
                byte id = inData[startIndex++];
                AbsNByteBase value = NByteBaseUtils.getNext(inData, ref startIndex);
                Value.Add(id, value);
            }
        }

        protected override byte[] onSerialize()
        {
            byte count = 0;
            byte[] bytes = new byte[0];
            foreach (var item in Value)
            {
                if (item.Value == null) continue;
                var idBytes = new byte[] { item.Key };
                bytes = NUtils.merge(bytes, idBytes, item.Value.getBytes());
                count++;
            }
            bytes = NUtils.merge(new byte[] { count }, bytes);
            return bytes;
        }
        public void set(byte id, AbsNByteBase value)
        {
            if (Value.ContainsKey(id))
            {
                Value[id] = value;
            }
            else
            {
                Value.Add(id, value);
            }
        }
        public AbsNByteBase get(byte id)
        {
            if (Value.ContainsKey(id))
            {
                return Value[id];
            }
            return null;
        }
        public bool remove(byte id)
        {
            return Value.Remove(id);
        }
        public AbsNByteBase this[byte id]
        {
            get
            {
                return get(id);
            }
            set
            {
                set(id, value);
            }
        }
        public int Count => Value.Count;
    }
    /// <summary>
    /// Support up to 255 items, key is UTF-8 Encoding, support upto 255 bytes
    /// </summary>
    [NByteBaseDefault(16)]
    public class NStringValuePair : NByteBase<Dictionary<string, AbsNByteBase>>
    {
        public NStringValuePair()
        {
            Value = new Dictionary<string, AbsNByteBase>();
        }
        protected override void onDeserialize(byte[] inData, ref int startIndex)
        {
            byte count = inData[startIndex++];
            while (Value.Count < count)
            {
                byte keyLength = inData[startIndex++];
                var key = NConverter.getUTF8String(inData, ref startIndex, keyLength);
                AbsNByteBase value = NByteBaseUtils.getNext(inData, ref startIndex);
                Value.Add(key, value);
            }
        }

        protected override byte[] onSerialize()
        {
            byte count = 0;
            byte[] bytes = new byte[0];
            foreach (var item in Value)
            {
                if (item.Value == null || item.Key == null) continue;
                var keyLengthBytes = new byte[] { Convert.ToByte(item.Key.Length) };
                byte[] keyBytes = NConverter.getBytes(item.Key);
                bytes = NUtils.merge(bytes, keyLengthBytes, keyBytes, item.Value.getBytes());
                count++;
            }
            bytes = NUtils.merge(new byte[] { count }, bytes);
            return bytes;
        }
        public void set(string id, AbsNByteBase value)
        {
            if (id == null) throw new ArgumentNullException("id");
            if (Value.ContainsKey(id))
            {
                Value[id] = value;
            }
            else
            {
                Value.Add(id, value);
            }
        }
        public AbsNByteBase get(string id)
        {
            if (Value.ContainsKey(id))
            {
                return Value[id];
            }
            return null;
        }
        public bool remove(string id)
        {
            return Value.Remove(id);
        }
        public AbsNByteBase this[string id]
        {
            get
            {
                return get(id);
            }
            set
            {
                set(id, value);
            }
        }
        public int Count => Value.Count;
    }
    [NByteBaseDefault(17)]
    public class NMixInt : NByteBase<int>
    {
        protected override void onDeserialize(byte[] inData, ref int startIndex)
        {
            var isSigned = inData[startIndex] > 128;
            var absLength = inData[startIndex];
            if (isSigned)
            {
                absLength -= 128;
            }
            startIndex++;
            if (absLength == 1)
            {
                Value = inData[startIndex++];
            }
            else if (absLength == 2)
            {
                Value = NConverter.toUInt16(inData, ref startIndex);
            }
            else
            {
                Value = NConverter.toInt32(inData, ref startIndex);
            }
            if (isSigned)
            {
                Value = -Value;
            }
        }
        protected override byte[] onSerialize()
        {
            bool isSigned = Value < 0;
            var absValue = Value > 0 ? Value : -Value;
            byte byteLengthAbsValue;
            byte[] absValueBytes;
            if (absValue < byte.MaxValue)
            {
                byteLengthAbsValue = 1;
                absValueBytes = new byte[] { Convert.ToByte(absValue) };
            }
            else if (absValue < ushort.MaxValue)
            {
                byteLengthAbsValue = 2;
                absValueBytes = NConverter.getBytes(Convert.ToUInt16(absValue));
            }
            else
            {
                byteLengthAbsValue = 4;
                absValueBytes = NConverter.getBytes(absValue);
            }
            if (isSigned)
            {
                byteLengthAbsValue += 128;
            }
            return NUtils.merge(new byte[] { byteLengthAbsValue }, absValueBytes);
        }
        public T getValue<T>()
        {
            return (T)getValue();
        }
    }
}