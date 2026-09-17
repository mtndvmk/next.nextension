using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Nextension
{
    public readonly struct PUCounter<T> : IEquatable<PUCounter<T>>, IDisposable where T : unmanaged
    {
        private static int _stride => Unsafe.SizeOf<UKVPair<T, long>>();

        public static PUCounter<T> get()
        {
            var plist = PNList<byte>.get();
            return new PUCounter<T>(plist, plist.ReturnToken);
        }

        public static PUCounter<T> get(int capacity)
        {
            var counter = get();
            counter.EnsureCapacity(capacity);
            return counter;
        }

        public static PUCounter<T> get(IEnumerable<T> collection)
        {
            var counter = get();
            counter.AddRange(collection);
            return counter;
        }

        private PUCounter(PNList<byte> list, long returnToken)
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
        public readonly int Capacity => IsCreated ? i_list.Capacity / _stride : throw new InvalidOperationException("Counter is not created.");
        public readonly bool IsDisposed => !IsCreated || !i_pList.IsRented;
        public readonly UKVPair<T, long>[] Items => ToArray();

        public readonly long TotalCount
        {
            get
            {
                long total = 0;
                int count = Count;
                for (int i = 0; i < count; i++)
                {
                    total += GetCountWithoutChecks(i);
                }
                return total;
            }
        }

        public readonly long this[T key]
        {
            get => GetCount(key);
            set => SetCount(key, value);
        }

        public readonly UKVPair<T, long> GetPairWithoutChecks(int index)
        {
            return Unsafe.ReadUnaligned<UKVPair<T, long>>(ref i_list.i_Items[index * _stride]);
        }

        public readonly T GetKeyWithoutChecks(int index)
        {
            return GetPairWithoutChecks(index).Key;
        }

        public readonly long GetCountWithoutChecks(int index)
        {
            return GetPairWithoutChecks(index).Value;
        }

        public readonly void SetPairWithoutChecks(int index, UKVPair<T, long> pair)
        {
            Unsafe.WriteUnaligned(ref i_list.i_Items[index * _stride], pair);
        }

        public readonly void SetCountWithoutChecks(int index, long count)
        {
            var pair = GetPairWithoutChecks(index);
            pair.Value = count;
            SetPairWithoutChecks(index, pair);
        }

        public readonly void SetAtWithoutChecks(int index, T key, long count)
        {
            SetPairWithoutChecks(index, new UKVPair<T, long>(key, count));
        }

        public readonly T GetKey(int index)
        {
            if ((uint)index >= (uint)Count)
            {
                throw new IndexOutOfRangeException();
            }
            return GetKeyWithoutChecks(index);
        }

        public readonly long GetCountAt(int index)
        {
            if ((uint)index >= (uint)Count)
            {
                throw new IndexOutOfRangeException();
            }
            return GetCountWithoutChecks(index);
        }

        public readonly UKVPair<T, long> GetPair(int index)
        {
            if ((uint)index >= (uint)Count)
            {
                throw new IndexOutOfRangeException();
            }
            return GetPairWithoutChecks(index);
        }

        public readonly void SetCountAt(int index, long count)
        {
            if ((uint)index >= (uint)Count)
            {
                throw new IndexOutOfRangeException();
            }
            SetCountWithoutChecks(index, count);
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

        public readonly void SetWithoutChecks(T key, long count)
        {
            var list = i_list;
            var requiredCapacity = list.i_Count + _stride;
            if (requiredCapacity > list.Capacity)
            {
                var newCapacity = list.Capacity == 0 ? 16 : list.Capacity << 1;
                list.EnsureCapacity(newCapacity > requiredCapacity ? newCapacity : requiredCapacity);
            }
            Unsafe.WriteUnaligned(ref list.i_Items[list.i_Count], new UKVPair<T, long>(key, count));
            list.i_Count += _stride;
        }

        public readonly long Increase(T key)
        {
            return Increase(key, 1);
        }

        public readonly long Increase(T key, long amount)
        {
            int index = IndexOf(key);
            if (index >= 0)
            {
                long newCount = GetCountWithoutChecks(index) + amount;
                SetCountWithoutChecks(index, newCount);
                return newCount;
            }
            SetWithoutChecks(key, amount);
            return amount;
        }

        public readonly long Decrease(T key)
        {
            return Increase(key, -1);
        }

        public readonly long Decrease(T key, long amount)
        {
            return Increase(key, -amount);
        }

        public readonly void SetCount(T key, long count)
        {
            int index = IndexOf(key);
            if (index >= 0)
            {
                SetCountWithoutChecks(index, count);
            }
            else
            {
                SetWithoutChecks(key, count);
            }
        }

        public readonly void AddRange<TCollection>(TCollection collection) where TCollection : IEnumerable<T>
        {
            foreach (var key in collection)
            {
                Increase(key, 1);
            }
        }

        public readonly void AddRange(ReadOnlySpan<T> keys)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                Increase(keys[i], 1);
            }
        }

        public readonly void AddRange(PUCounter<T> other)
        {
            int count = other.Count;
            for (int i = 0; i < count; i++)
            {
                Increase(other.GetKeyWithoutChecks(i), other.GetCountWithoutChecks(i));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Clear()
        {
            i_list.Clear();
        }

        public readonly bool Contains(T key)
        {
            return IndexOf(key) >= 0;
        }

        public readonly long GetCount(T key)
        {
            int index = IndexOf(key);
            return index >= 0 ? GetCountWithoutChecks(index) : 0;
        }

        public readonly bool TryGetCount(T key, out long count)
        {
            int index = IndexOf(key);
            if (index >= 0)
            {
                count = GetCountWithoutChecks(index);
                return true;
            }
            count = 0;
            return false;
        }

        public readonly bool Remove(T key)
        {
            int index = IndexOf(key);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        public readonly bool Remove(T key, out long count)
        {
            int index = IndexOf(key);
            if (index >= 0)
            {
                count = GetCountWithoutChecks(index);
                RemoveAt(index);
                return true;
            }
            count = 0;
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
                SetAtWithoutChecks(index, GetKeyWithoutChecks(lastIndex), GetCountWithoutChecks(lastIndex));
            }
            RemoveAt(lastIndex);
        }

        public readonly bool RemoveSwapBack(T key)
        {
            int index = IndexOf(key);
            if (index < 0) return false;
            RemoveAtSwapBack(index);
            return true;
        }

        public readonly int IndexOf(T key)
        {
            var list = i_list;
            int count = Count;
            for (int i = 0; i < count; i++)
            {
                var currentKey = Unsafe.ReadUnaligned<UKVPair<T, long>>(ref list.i_Items[i * _stride]).Key;
                if (EqualityComparer<T>.Default.Equals(currentKey, key))
                {
                    return i;
                }
            }
            return -1;
        }

        public readonly void StopTracking()
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Dispose()
        {
            i_pList?.SafeReturn(i_returnToken);
        }

        public readonly Span<UKVPair<T, long>> AsSpan()
        {
            if (!IsCreated) return Span<UKVPair<T, long>>.Empty;
            return MemoryMarshal.Cast<byte, UKVPair<T, long>>(i_list.AsSpan());
        }

        public readonly T[] ToKeyArray()
        {
            int count = Count;
            if (count == 0) return Array.Empty<T>();
            var result = new T[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = GetKeyWithoutChecks(i);
            }
            return result;
        }

        public readonly UKVPair<T, long>[] ToArray()
        {
            return AsSpan().ToArray();
        }

        public readonly Dictionary<T, long> ToDictionary()
        {
            var result = new Dictionary<T, long>(Count);
            foreach (var item in AsSpan())
            {
                result[item.Key] = item.Value;
            }
            return result;
        }

        public bool Equals(PUCounter<T> other)
        {
            return i_list != null && i_list.Equals(other.i_list);
        }

        public readonly UArrayEnumerator<UKVPair<T, long>> GetEnumerator()
        {
            if (!IsCreated) return UArrayEnumerator<UKVPair<T, long>>.Empty;
            return new UArrayEnumerator<UKVPair<T, long>>(i_list.i_Items, i_list.i_Count);
        }
    }
}
