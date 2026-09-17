using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Nextension
{
    public readonly struct PUList<T> : IEquatable<PUList<T>>, IDisposable where T : unmanaged
    {
        public static PUList<T> get()
        {
            var plist = PNList<byte>.get();
            return new PUList<T>(plist, plist.ReturnToken);
        }
        public static PUList<T> get(T t0)
        {
            var plist = get();
            plist.Add(t0);
            return plist;
        }
        public static PUList<T> get(T t0, T t1)
        {
            var plist = get();
            plist.Add(t0);
            plist.Add(t1);
            return plist;
        }
        public static PUList<T> get(T t0, T t1, T t2)
        {
            var plist = get();
            plist.Add(t0);
            plist.Add(t1);
            plist.Add(t2);
            return plist;
        }
        public static PUList<T> get(T t0, T t1, T t2, T t3)
        {
            var plist = get();
            plist.Add(t0);
            plist.Add(t1);
            plist.Add(t2);
            plist.Add(t3);
            return plist;
        }
        public static PUList<T> get(int capacity)
        {
            var plist = get();
            plist.EnsureCapacity(capacity);
            return plist;
        }
        public static PUList<T> get(IEnumerable<T> collection)
        {
            var plist = get();
            plist.AddRange(collection);
            return plist;
        }

        private PUList(PNList<byte> list, long returnToken)
        {
            i_pList = list;
            i_returnToken = returnToken;
        }

        internal readonly PNList<byte> i_pList;
        internal readonly NList<byte> i_list => i_pList.Collection;
        internal readonly long i_returnToken;

        public readonly int Count => !IsDisposed ? i_list.Count / Unsafe.SizeOf<T>() : 0;
        public readonly bool IsCreated => i_pList != null;
        public readonly bool IsReadOnly => false;
        public readonly int Capacity => !IsDisposed ? i_list.Count / Unsafe.SizeOf<T>() : throw new InvalidOperationException("List is not created.");
        public readonly bool IsDisposed => !IsCreated || i_pList.ReturnToken != i_returnToken;
        public readonly T[] Items => ToArray();

        public readonly T this[int index]
        {
            get
            {
                if ((uint)index >= Count)
                {
                    throw new IndexOutOfRangeException();
                }
                return GetWithoutChecks(index);
            }
            set
            {
                if ((uint)index >= Count)
                {
                    throw new IndexOutOfRangeException();
                }
                SetWithoutChecks(index, value);
            }
        }

        public readonly T GetWithoutChecks(int index)
        {
            return Unsafe.ReadUnaligned<T>(ref i_list.i_Items[index * Unsafe.SizeOf<T>()]);
        }

        public readonly void SetWithoutChecks(int index, T value)
        {
            Unsafe.WriteUnaligned(ref i_list.i_Items[index * Unsafe.SizeOf<T>()], value);
        }

        public readonly void EnsureCapacity(int capacity)
        {
            EnsureCapacityInBytes(capacity * Unsafe.SizeOf<T>());
        }

        public readonly void EnsureCapacityInBytes(int capacityInBytes)
        {
            var list = i_list;
            if (list.Capacity >= capacityInBytes) return;
            list.EnsureCapacity(capacityInBytes > 16 ? capacityInBytes : 16);
        }

        public readonly void Add(T item)
        {
            var sizeOfT = Unsafe.SizeOf<T>();
            var list = i_list;
            var requiredCapacity = list.i_Count + sizeOfT;
            if (requiredCapacity > list.Capacity)
            {
                var newCapacity = list.Capacity == 0 ? 16 : list.Capacity << 1;
                list.EnsureCapacity(newCapacity > requiredCapacity ? newCapacity : requiredCapacity);
            }
            Unsafe.WriteUnaligned(ref list.i_Items[list.i_Count], item);
            list.i_Count += sizeOfT;
        }

        public readonly bool AddIfNotPresent(T item)
        {
            if (Contains(item)) return false;
            Add(item);
            return true;
        }

        public readonly void AddRange<TCollection>(TCollection collection) where TCollection : IEnumerable<T>
        {
            if (collection is NList<T> nList)
            {
                AddRange(nList.AsSpan());
            }
            else if (collection is IList<T> list)
            {
                int count = list.Count;
                int addSizeInBytes = count * Unsafe.SizeOf<T>();
                var listBytes = i_list;
                EnsureCapacityInBytes(addSizeInBytes + listBytes.i_Count);
                int byteStart = listBytes.i_Count;
                for (int i = 0; i < count; i++)
                {
                    Unsafe.WriteUnaligned(ref listBytes.i_Items[byteStart + i * Unsafe.SizeOf<T>()], list[i]);
                }
                listBytes.i_Count += addSizeInBytes;
            }
            else if (collection is IReadOnlyList<T> rolist)
            {
                int count = rolist.Count;
                int addSizeInBytes = count * Unsafe.SizeOf<T>();
                var listBytes = i_list;
                EnsureCapacityInBytes(addSizeInBytes + listBytes.i_Count);
                int byteStart = listBytes.i_Count;
                for (int i = 0; i < count; i++)
                {
                    Unsafe.WriteUnaligned(ref listBytes.i_Items[byteStart + i * Unsafe.SizeOf<T>()], rolist[i]);
                }
                listBytes.i_Count += addSizeInBytes;
            }
            else if (collection is ICollection<T> collection2)
            {
                int count = collection2.Count;
                int addSizeInBytes = count * Unsafe.SizeOf<T>();
                var listBytes = i_list;
                EnsureCapacityInBytes(addSizeInBytes + listBytes.i_Count);
                int byteStart = listBytes.i_Count;
                int offset = 0;
                foreach (var item in collection2)
                {
                    Unsafe.WriteUnaligned(ref listBytes.i_Items[byteStart + offset], item);
                    offset += Unsafe.SizeOf<T>();
                }
                listBytes.i_Count += addSizeInBytes;
            }
            else
            {
                foreach (var item in collection)
                {
                    Add(item);
                }
            }
        }

        public readonly void AddRange(T[] items)
        {
            AddRange(items.AsSpan());
        }

        public readonly void AddRange(ReadOnlySpan<T> items)
        {
            if (items.Length == 0) return;
            var addSizeInBytes = Unsafe.SizeOf<T>() * items.Length;
            var list = i_list;
            EnsureCapacityInBytes(addSizeInBytes + list.i_Count);
            MemoryMarshal.AsBytes(items).CopyTo(list.i_Items.AsSpan(list.i_Count));
            list.i_Count += addSizeInBytes;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Clear()
        {
            i_list.Clear();
        }

        public readonly bool Contains(T item)
        {
            return IndexOf(item) >= 0;
        }

        public readonly int FirstIndexOf(Predicate<T> predicate)
        {
            for (int i = 0; i < Count; i++)
            {
                if (predicate(GetWithoutChecks(i))) return i;
            }
            return -1;
        }

        public readonly void CopyTo(T[] dst)
        {
            CopyTo(dst, 0);
        }

        public readonly void CopyTo(T[] dst, int dstIndex)
        {
            if (dstIndex + Count > dst.Length)
            {
                throw new ArgumentOutOfRangeException();
            }
            if (Count > 0)
            {
                AsSpan().CopyTo(dst.AsSpan(dstIndex, Count));
            }
        }

        public readonly void CopyTo(PUList<T> dst)
        {
            dst.AddRange(AsSpan());
        }

        public readonly bool Remove(T item)
        {
            int index = IndexOf(item);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        public readonly void RemoveAt(int index)
        {
            if ((uint)index >= (uint)Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            var sizeOfT = Unsafe.SizeOf<T>();
            var list = i_list;
            int byteIndex = index * sizeOfT;
            int remaining = list.i_Count - byteIndex - sizeOfT;
            if (remaining > 0)
            {
                Array.Copy(list.i_Items, byteIndex + sizeOfT, list.i_Items, byteIndex, remaining);
            }
            list.i_Count -= sizeOfT;
            Array.Clear(list.i_Items, list.i_Count, sizeOfT);
        }

        public readonly void removeAtSwapBack(int index)
        {
            var lastIndex = Count - 1;
            this[index] = this[lastIndex];
            RemoveAt(lastIndex);
        }

        public readonly bool removeSwapBack(T item)
        {
            var index = IndexOf(item);
            if (index < 0) return false;
            removeAtSwapBack(index);
            return true;
        }

        public readonly T takeAndRemoveAtSwapBack(int index)
        {
            var item = this[index];
            removeAtSwapBack(index);
            return item;
        }

        public readonly void RemoveRange(int index, int count)
        {
            if (index < 0 || count < 0) throw new ArgumentOutOfRangeException(index < 0 ? nameof(index) : nameof(count));
            if (Count - index < count) throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");

            if (count > 0)
            {
                var sizeOfT = Unsafe.SizeOf<T>();
                var list = i_list;
                int byteIndex = index * sizeOfT;
                int removeBytes = count * sizeOfT;
                int remaining = list.i_Count - byteIndex - removeBytes;
                if (remaining > 0)
                {
                    Array.Copy(list.i_Items, byteIndex + removeBytes, list.i_Items, byteIndex, remaining);
                }
                list.i_Count -= removeBytes;
                Array.Clear(list.i_Items, list.i_Count, removeBytes);
            }
        }

        public readonly int IndexOf(T item, int startIndex = 0)
        {
            var list = i_list;
            var sizeOfT = Unsafe.SizeOf<T>();
            int tCount = list.i_Count / sizeOfT;
            for (int i = startIndex; i < tCount; i++)
            {
                if (EqualityComparer<T>.Default.Equals(Unsafe.ReadUnaligned<T>(ref list.i_Items[i * sizeOfT]), item))
                {
                    return i;
                }
            }
            return -1;
        }

        public readonly void Insert(int index, T item)
        {
            var sizeOfT = Unsafe.SizeOf<T>();
            var list = i_list;
            int byteIndex = index * sizeOfT;
            var requiredCapacity = list.i_Count + sizeOfT;
            if (requiredCapacity > list.Capacity)
            {
                var newCapacity = list.Capacity == 0 ? 16 : list.Capacity << 1;
                list.EnsureCapacity(newCapacity > requiredCapacity ? newCapacity : requiredCapacity);
            }
            int remaining = list.i_Count - byteIndex;
            if (remaining > 0)
            {
                Array.Copy(list.i_Items, byteIndex, list.i_Items, byteIndex + sizeOfT, remaining);
            }
            Unsafe.WriteUnaligned(ref list.i_Items[byteIndex], item);
            list.i_Count += sizeOfT;
        }

        public readonly void InsertRange(int index, ReadOnlySpan<T> span)
        {
            if (span.Length == 0) return;
            var sizeOfT = Unsafe.SizeOf<T>();
            i_list.InsertRangeWithoutChecks(index * sizeOfT, MemoryMarshal.AsBytes(span));
        }

        public readonly void Sort()
        {
            SortUtil.quickSort(AsSpan());
        }

        public readonly void Sort(Comparison<T> comparision)
        {
            SortUtil.quickSort(AsSpan(), comparision);
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

        public readonly UArrayEnumerator<T> GetEnumerator()
        {
            if (!IsCreated) return UArrayEnumerator<T>.Empty;
            return new UArrayEnumerator<T>(i_list.i_Items, i_list.i_Count);
        }

        public readonly Span<T> AsSpan()
        {
            return MemoryMarshal.Cast<byte, T>(i_list.AsSpan());
        }

        public readonly MemoryStream GetStream()
        {
            return new MemoryStream(i_list.i_Items, 0, i_list.Count, false);
        }

        public readonly Span<T> EnsureCapacityAsSpan(int capacity)
        {
            var capacityByteCount = capacity * Unsafe.SizeOf<T>();
            EnsureCapacityInBytes(capacityByteCount);
            return MemoryMarshal.Cast<byte, T>(i_list.AsSpan(0, capacityByteCount));
        }

        public readonly void SetCount(int count)
        {
            i_list.SetCount(count * Unsafe.SizeOf<T>());
        }

        public readonly T[] ToArray()
        {
            return AsSpan().ToArray();
        }

        public bool Equals(PUList<T> other)
        {
            return i_list.Equals(other.i_list);
        }


    }
}
