using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Nextension
{
    public readonly struct PPairUList<TKey, TValue> : IEquatable<PPairUList<TKey, TValue>>, IDisposable
        where TKey : unmanaged
        where TValue : unmanaged
    {
        private static int _sizeOfKey => Unsafe.SizeOf<TKey>();
        private static int _stride => Unsafe.SizeOf<UKVPair<TKey, TValue>>();

        public static PPairUList<TKey, TValue> get()
        {
            var plist = PNList<byte>.get();
            return new PPairUList<TKey, TValue>(plist, plist.ReturnToken);
        }

        public static PPairUList<TKey, TValue> get(int capacity)
        {
            var plist = get();
            plist.EnsureCapacity(capacity);
            return plist;
        }

        public static PPairUList<TKey, TValue> get(IEnumerable<KeyValuePair<TKey, TValue>> collection)
        {
            var plist = get();
            plist.AddRange(collection);
            return plist;
        }

        private PPairUList(PNList<byte> list, long returnToken)
        {
            i_pList = list;
            i_returnToken = returnToken;
        }

        internal readonly PNList<byte> i_pList;
        internal readonly NList<byte> i_list => i_pList.Collection;
        internal readonly long i_returnToken;

        public readonly int Count => IsCreated ? i_list.Count / _stride : 0;
        public readonly bool IsCreated => i_pList != null;
        public readonly bool IsReadOnly => false;
        public readonly int Capacity => IsCreated ? i_list.Capacity / _stride : throw new InvalidOperationException("List is not created.");
        public readonly bool IsDisposed => !IsCreated || !i_pList.IsRented;
        public readonly KeyValuePair<TKey, TValue>[] Items => ToArray();

        public readonly TValue this[TKey key]
        {
            get
            {
                int index = IndexOfKey(key);
                if (index < 0)
                {
                    throw new KeyNotFoundException($"Key '{key}' was not found.");
                }
                return GetValueWithoutChecks(index);
            }
            set
            {
                int index = IndexOfKey(key);
                if (index >= 0)
                {
                    SetValueWithoutChecks(index, value);
                }
                else
                {
                    AddWithoutChecks(key, value);
                }
            }
        }

        public readonly TKey GetKeyWithoutChecks(int index)
        {
            return Unsafe.ReadUnaligned<TKey>(ref i_list.i_Items[index * _stride]);
        }

        public readonly TValue GetValueWithoutChecks(int index)
        {
            return Unsafe.ReadUnaligned<TValue>(ref i_list.i_Items[index * _stride + _sizeOfKey]);
        }

        public readonly KeyValuePair<TKey, TValue> GetPairWithoutChecks(int index)
        {
            return new KeyValuePair<TKey, TValue>(GetKeyWithoutChecks(index), GetValueWithoutChecks(index));
        }

        public readonly void SetKeyWithoutChecks(int index, TKey key)
        {
            Unsafe.WriteUnaligned(ref i_list.i_Items[index * _stride], key);
        }

        public readonly void SetValueWithoutChecks(int index, TValue value)
        {
            Unsafe.WriteUnaligned(ref i_list.i_Items[index * _stride + _sizeOfKey], value);
        }

        public readonly void SetAtWithoutChecks(int index, TKey key, TValue value)
        {
            int byteIndex = index * _stride;
            Unsafe.WriteUnaligned(ref i_list.i_Items[byteIndex], key);
            Unsafe.WriteUnaligned(ref i_list.i_Items[byteIndex + _sizeOfKey], value);
        }

        public readonly TKey GetKey(int index)
        {
            if ((uint)index >= (uint)Count)
            {
                throw new IndexOutOfRangeException();
            }
            return GetKeyWithoutChecks(index);
        }

        public readonly TValue GetValue(int index)
        {
            if ((uint)index >= (uint)Count)
            {
                throw new IndexOutOfRangeException();
            }
            return GetValueWithoutChecks(index);
        }

        public readonly KeyValuePair<TKey, TValue> GetPair(int index)
        {
            if ((uint)index >= (uint)Count)
            {
                throw new IndexOutOfRangeException();
            }
            return GetPairWithoutChecks(index);
        }

        public readonly void SetKey(int index, TKey key)
        {
            if ((uint)index >= (uint)Count)
            {
                throw new IndexOutOfRangeException();
            }
            SetKeyWithoutChecks(index, key);
        }

        public readonly void SetValue(int index, TValue value)
        {
            if ((uint)index >= (uint)Count)
            {
                throw new IndexOutOfRangeException();
            }
            SetValueWithoutChecks(index, value);
        }

        public readonly void SetAt(int index, TKey key, TValue value)
        {
            if ((uint)index >= (uint)Count)
            {
                throw new IndexOutOfRangeException();
            }
            SetAtWithoutChecks(index, key, value);
        }

        public readonly void EnsureCapacity(int capacity)
        {
            EnsureCapacityInBytes(capacity * _stride);
        }

        public readonly void EnsureCapacityInBytes(int capacityInBytes)
        {
            var list = i_list;
            if (list.Capacity >= capacityInBytes) return;
            list.EnsureCapacity(capacityInBytes > 16 ? capacityInBytes : 16);
        }

        public readonly void AddWithoutChecks(TKey key, TValue value)
        {
            var list = i_list;
            var requiredCapacity = list.i_Count + _stride;
            if (requiredCapacity > list.Capacity)
            {
                var newCapacity = list.Capacity == 0 ? 16 : list.Capacity << 1;
                list.EnsureCapacity(newCapacity > requiredCapacity ? newCapacity : requiredCapacity);
            }
            int byteIndex = list.i_Count;
            Unsafe.WriteUnaligned(ref list.i_Items[byteIndex], key);
            Unsafe.WriteUnaligned(ref list.i_Items[byteIndex + _sizeOfKey], value);
            list.i_Count += _stride;
        }

        public readonly void Add(TKey key, TValue value)
        {
            if (ContainsKey(key))
            {
                throw new ArgumentException($"An item with key '{key}' has already been added.");
            }
            AddWithoutChecks(key, value);
        }

        public readonly bool TryAdd(TKey key, TValue value)
        {
            if (ContainsKey(key)) return false;
            AddWithoutChecks(key, value);
            return true;
        }

        public readonly bool SetOrAdd(TKey key, TValue value)
        {
            int index = IndexOfKey(key);
            if (index >= 0)
            {
                SetValueWithoutChecks(index, value);
                return false;
            }
            AddWithoutChecks(key, value);
            return true;
        }

        public readonly void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> collection)
        {
            foreach (var pair in collection)
            {
                SetOrAdd(pair.Key, pair.Value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Clear()
        {
            i_list.Clear();
        }

        public readonly bool ContainsKey(TKey key)
        {
            return IndexOfKey(key) >= 0;
        }

        public readonly bool ContainsValue(TValue value)
        {
            return IndexOfValue(value) >= 0;
        }

        public readonly bool TryGetValue(TKey key, out TValue value)
        {
            int index = IndexOfKey(key);
            if (index >= 0)
            {
                value = GetValueWithoutChecks(index);
                return true;
            }
            value = default;
            return false;
        }

        public readonly bool Remove(TKey key)
        {
            int index = IndexOfKey(key);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        public readonly bool Remove(TKey key, out TValue value)
        {
            int index = IndexOfKey(key);
            if (index >= 0)
            {
                value = GetValueWithoutChecks(index);
                RemoveAt(index);
                return true;
            }
            value = default;
            return false;
        }

        public readonly void RemoveAt(int index)
        {
            if ((uint)index >= (uint)Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            var list = i_list;
            int byteIndex = index * _stride;
            int remaining = list.i_Count - byteIndex - _stride;
            if (remaining > 0)
            {
                Array.Copy(list.i_Items, byteIndex + _stride, list.i_Items, byteIndex, remaining);
            }
            list.i_Count -= _stride;
            Array.Clear(list.i_Items, list.i_Count, _stride);
        }

        public readonly void RemoveAtSwapBack(int index)
        {
            int lastIndex = Count - 1;
            if ((uint)index > (uint)lastIndex)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            if (index != lastIndex)
            {
                SetAtWithoutChecks(index, GetKeyWithoutChecks(lastIndex), GetValueWithoutChecks(lastIndex));
            }
            RemoveAt(lastIndex);
        }

        public readonly bool RemoveKeySwapBack(TKey key)
        {
            int index = IndexOfKey(key);
            if (index < 0) return false;
            RemoveAtSwapBack(index);
            return true;
        }

        public readonly bool RemoveKeySwapBack(TKey key, out TValue value)
        {
            int index = IndexOfKey(key);
            if (index < 0)
            {
                value = default;
                return false;
            }
            value = GetValueWithoutChecks(index);
            RemoveAtSwapBack(index);
            return true;
        }

        public readonly int IndexOfKey(TKey key, int startIndex = 0)
        {
            var list = i_list;
            int count = Count;
            for (int i = startIndex; i < count; i++)
            {
                int byteIndex = i * _stride;
                TKey currentKey = Unsafe.ReadUnaligned<TKey>(ref list.i_Items[byteIndex]);
                if (EqualityComparer<TKey>.Default.Equals(currentKey, key))
                {
                    return i;
                }
            }
            return -1;
        }

        public readonly int IndexOfValue(TValue value, int startIndex = 0)
        {
            var list = i_list;
            int count = Count;
            for (int i = startIndex; i < count; i++)
            {
                int byteIndex = i * _stride + _sizeOfKey;
                TValue currentValue = Unsafe.ReadUnaligned<TValue>(ref list.i_Items[byteIndex]);
                if (EqualityComparer<TValue>.Default.Equals(currentValue, value))
                {
                    return i;
                }
            }
            return -1;
        }

        public readonly void Insert(int index, TKey key, TValue value)
        {
            if (ContainsKey(key))
            {
                throw new ArgumentException($"An item with key '{key}' has already been added.");
            }
            InsertWithoutChecks(index, key, value);
        }

        public readonly void InsertWithoutChecks(int index, TKey key, TValue value)
        {
            var list = i_list;
            int byteIndex = index * _stride;
            var requiredCapacity = list.i_Count + _stride;
            if (requiredCapacity > list.Capacity)
            {
                var newCapacity = list.Capacity == 0 ? 16 : list.Capacity << 1;
                list.EnsureCapacity(newCapacity > requiredCapacity ? newCapacity : requiredCapacity);
            }
            int remaining = list.i_Count - byteIndex;
            if (remaining > 0)
            {
                Array.Copy(list.i_Items, byteIndex, list.i_Items, byteIndex + _stride, remaining);
            }
            Unsafe.WriteUnaligned(ref list.i_Items[byteIndex], key);
            Unsafe.WriteUnaligned(ref list.i_Items[byteIndex + _sizeOfKey], value);
            list.i_Count += _stride;
        }

        public readonly void StopTracking()
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Dispose()
        {
            if (i_pList != null)
            {
                i_pList.SafeReturn(i_returnToken);
            }
        }

        public readonly KeyValuePair<TKey, TValue>[] ToArray()
        {
            int count = Count;
            if (count == 0) return Array.Empty<KeyValuePair<TKey, TValue>>();
            var result = new KeyValuePair<TKey, TValue>[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = GetPairWithoutChecks(i);
            }
            return result;
        }

        public bool Equals(PPairUList<TKey, TValue> other)
        {
            return i_list != null && i_list.Equals(other.i_list);
        }

        public readonly UArrayEnumerator<UKVPair<TKey, TValue>> GetEnumerator()
        {
            if (!IsCreated) return UArrayEnumerator<UKVPair<TKey, TValue>>.Empty;
            return new UArrayEnumerator<UKVPair<TKey, TValue>>(i_list.i_Items, i_list.i_Count);
        }

        public readonly Span<UKVPair<TKey, TValue>> AsSpan()
        {
            if (!IsCreated) return Span<UKVPair<TKey, TValue>>.Empty;
            return MemoryMarshal.Cast<byte, UKVPair<TKey, TValue>>(i_list.AsSpan());
        }

        public readonly void SortByKey()
        {
            var span = AsSpan();
            if (span.Length <= 1) return;
            SortUtil.quickSort(span, static (a, b) => Comparer<TKey>.Default.Compare(a.Key, b.Key));
        }
    }
}
