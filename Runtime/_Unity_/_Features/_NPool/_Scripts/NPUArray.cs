using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Nextension
{
    public readonly struct NPUArray<T> :IDisposable where T : unmanaged
    {
        public static NPUArray<T> get()
        {
            return new NPUArray<T>(NPNativeList<byte>.get());
        }
        public static NPUArray<T> get(T t0)
        {
            var arr = get();
            arr.Add(t0);
            return arr;
        }
        public static NPUArray<T> get(T t0, T t1)
        {
            var arr = get();
            arr.Add(t0);
            arr.Add(t1);
            return arr;
        }
        public static NPUArray<T> get(T t0, T t1, T t2)
        {
            var arr = get();
            arr.Add(t0);
            arr.Add(t1);
            arr.Add(t2);
            return arr;
        }
        public static NPUArray<T> get(T t0, T t1, T t2, T t3)
        {
            var arr = get();
            arr.Add(t0);
            arr.Add(t1);
            arr.Add(t2);
            arr.Add(t3);
            return arr;
        }
        public static NPUArray<T> get(int capacity)
        {
            var npArray = new NPUArray<T>(NPNativeList<byte>.get());
            npArray.EnsureCapacity(capacity);
            return npArray;
        }
        public static NPUArray<T> get(IEnumerable<T> collection)
        {
            var npArray = new NPUArray<T>(NPNativeList<byte>.get());
            npArray.AddRange(collection);
            return npArray;
        }
        public static NPUArray<T> getWithoutTracking()
        {
            var npArray = new NPUArray<T>(NPNativeList<byte>.getWithoutTracking());
            return npArray;
        }
        public static NPUArray<T> getWithoutTracking(int capacity)
        {
            var npArray = new NPUArray<T>(NPNativeList<byte>.getWithoutTracking());
            npArray.EnsureCapacity(capacity);
            return npArray;
        }
        public static NPUArray<T> getWithoutTracking(IEnumerable<T> collection)
        {
            var npArray = new NPUArray<T>(NPNativeList<byte>.getWithoutTracking());
            npArray.AddRange(collection);
            return npArray;
        }

        public readonly void StopTracking()
        {
            i_array.stopTracking();
        }

        private NPUArray(NPNativeList<byte> arr)
        {
            i_array = arr;
        }

        internal readonly NPNativeList<byte> i_array;

        public readonly int Count => IsCreated ? i_array.Count / NUtils.sizeOf<T>() : 0;
        public readonly bool IsCreated => i_array != null;
        public readonly bool IsReadOnly => i_array.IsReadOnly;
        public readonly int Capacity => IsCreated ? i_array.Capacity / NUtils.sizeOf<T>() : throw new InvalidOperationException("Array is not created.");
        public readonly bool IsDisposed => !IsCreated || i_array.IsDisposed;
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

        public readonly unsafe T GetWithoutChecks(int index)
        {
            T* ptr = (T*)i_array.Collection.GetUnsafePtr();
            return ptr[index];
        }
        
        public readonly unsafe void SetWithoutChecks(int index, T value)
        {
            byte* bPtr = i_array.Collection.GetUnsafePtr();
            T* ptr = (T*)bPtr;
            ptr[index] = value;
        }

        public readonly void EnsureCapacity(int capacity)
        {
            var capacityInBytes = capacity * NUtils.sizeOf<T>();
            EnsureCapacityInBytes(capacityInBytes);
        }
        
        public readonly void EnsureCapacityInBytes(int capacityInBytes)
        {
            var byteArray = i_array.Collection;
            if (byteArray.Capacity >= capacityInBytes) return;
            byteArray.ensureCapacity(capacityInBytes > 16 ? capacityInBytes : 16);
        }
        
        public readonly unsafe void Add(T item)
        {
            var sizeOfT = NUtils.sizeOf<T>();
            var byteArray = i_array.Collection;
            if (byteArray.Capacity == 0)
            {
                byteArray.ensureCapacity(16);
            }
            else if (byteArray.Count + sizeOfT > byteArray.Capacity)
            {
                byteArray.ensureCapacity(byteArray.Capacity << 1);
            }
            byte* bPtr = byteArray.GetUnsafePtr();
            ((T*)bPtr)[byteArray.i_Count / sizeOfT] = item;
            byteArray.i_Count += sizeOfT;
        }

        public readonly bool AddIfNotPresent(T item)
        {
            if (Contains(item)) return false;
            Add(item);
            return true;
        }
        
        public unsafe readonly void AddRange<TCollection>(TCollection collection) where TCollection : IEnumerable<T>
        {
            if (collection is IList<T> list)
            {
                int addSizeInBytes = list.Count * NUtils.sizeOf<T>();
                int startIndex = Count;
                var byteArray = i_array.Collection;
                EnsureCapacityInBytes(addSizeInBytes + byteArray.i_Count);
                byte* bPtr = byteArray.GetUnsafePtr(byteArray.i_Count);
                for (int i = 0; i < list.Count; i++)
                {
                    ((T*)bPtr)[startIndex++] = list[i];
                }
                byteArray.i_Count += addSizeInBytes;
            }
            else if (collection is IReadOnlyList<T> rolist)
            {
                int addSizeInBytes = rolist.Count * NUtils.sizeOf<T>();
                int startIndex = Count;
                var byteArray = i_array.Collection;
                EnsureCapacityInBytes(addSizeInBytes + byteArray.i_Count);
                byte* bPtr = byteArray.GetUnsafePtr(byteArray.i_Count);
                for (int i = 0; i < rolist.Count; i++)
                {
                    ((T*)bPtr)[startIndex++] = rolist[i];
                }
                byteArray.i_Count += addSizeInBytes;
            }
            else if (collection is ICollection<T> collection2)
            {
                int addSizeInBytes = collection2.Count * NUtils.sizeOf<T>();
                int startIndex = Count;
                var byteArray = i_array.Collection;
                EnsureCapacityInBytes(addSizeInBytes + byteArray.i_Count);
                byte* bPtr = byteArray.GetUnsafePtr(byteArray.i_Count);
                foreach (var item in collection2)
                {
                    ((T*)bPtr)[startIndex++] = item;
                }
                byteArray.i_Count += addSizeInBytes;
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
        
        public readonly unsafe void AddRange(ReadOnlySpan<T> items)
        {
            if (items.Length == 0) return;
            var addSizeInBytes = NUtils.sizeOf<T>() * items.Length;
            var byteArray = i_array.Collection;
            EnsureCapacityInBytes(addSizeInBytes + byteArray.i_Count);
            fixed (T* tPtr = items)
            {
                byte* dst = byteArray.GetUnsafePtr(byteArray.i_Count);
                Buffer.MemoryCopy(tPtr, dst, addSizeInBytes, addSizeInBytes);
                byteArray.i_Count += addSizeInBytes;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Clear()
        {
            i_array.Clear();
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
        
        public readonly unsafe void CopyTo(T[] dst, int dstIndex)
        {
            if (dstIndex + Count > dst.Length)
            {
                throw new ArgumentOutOfRangeException();
            }
            fixed (T* tPtr = &dst[dstIndex])
            {
                var byteArray = i_array.Collection;
                int sizeInBytes = byteArray.Count;
                byte* dstPtr = (byte*)tPtr;
                byte* srcPtr = byteArray.GetUnsafePtr();
                Buffer.MemoryCopy(srcPtr, dstPtr, sizeInBytes, sizeInBytes);
            }
        }
        
        public readonly void CopyTo(NPUArray<T> dst)
        {
            dst.EnsureCapacity(Count);
            dst.i_array.CopyFrom(i_array);
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
            i_array.Collection.RemoveRange(index * NUtils.sizeOf<T>(), NUtils.sizeOf<T>());
        }

        public readonly void RemoveRange(int index, int count)
        {
            if (index < 0 || count < 0) throw new ArgumentOutOfRangeException(index < 0 ? nameof(index) : nameof(count));
            if (Count - index < count) throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");

            if (count > 0)
            {
                var sizeOfT = NUtils.sizeOf<T>();
                i_array.Collection.RemoveRange(index * sizeOfT, count * sizeOfT);
            }
        }

        public readonly unsafe int IndexOf(T item, int startIndex = 0)
        {
            var collection = i_array.Collection;
            var sizeOfT = NUtils.sizeOf<T>();
            int tCount = collection.i_Count / sizeOfT;
            byte* bPtr = collection.GetUnsafePtr();
            T* ptr = (T*)bPtr;
            for (int i = startIndex; i < tCount; i++)
            {
                if (ptr[i].equals(item))
                {
                    return i;
                }
            }
            return -1;
        }

        public readonly unsafe void Insert(int index, T item)
        {
            var sizeOfT = NUtils.sizeOf<T>();
            byte* bPtr = stackalloc byte[sizeOfT];
            *(T*)bPtr = item;
            i_array.InsertRangeWithoutChecks(index * sizeOfT, new ReadOnlySpan<byte>(bPtr, sizeOfT));
        }

        public readonly void InsertRange(int index, ReadOnlySpan<T> span)
        {
            if (span.Length == 0) return;
            var sizeOfT = NUtils.sizeOf<T>();
            i_array.InsertRangeWithoutChecks(index * sizeOfT, span.asSpan<T, byte>());
        }

        public readonly unsafe void InsertRange(int index, T* ptr, int length)
        {
            if (length <= 0) return;
            var sizeOfT = NUtils.sizeOf<T>();
            i_array.InsertRangeWithoutChecks(index * sizeOfT, new ReadOnlySpan<byte>((byte*)ptr, length * sizeOfT));
        }

        public readonly void Sort()
        {
            NUtils.quickSort(AsSpan());
        }

        public readonly void Sort(Comparison<T> comparision)
        {
            NUtils.quickSort(AsSpan(), comparision);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Dispose()
        {
            i_array.Dispose();
        }

        private readonly unsafe UnsafeArrayEnumerator<T> GetUnsafeEnumerator()
        {
            byte* bPtr = i_array.Collection.GetUnsafePtr();
            {
                var tPtr = (T*)bPtr;
                return new UnsafeArrayEnumerator<T>(tPtr, (uint)Count);
            }
        }
 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly UnsafeArrayEnumerator<T> GetEnumerator()
        {
            return GetUnsafeEnumerator();
        }
        
        public readonly Span<T> AsSpan()
        {
            return i_array.Collection.AsSpan().asSpan<byte, T>();
        }
        
        public readonly Span<T> AsSpan(int count)
        {
            return i_array.Collection.AsSpan(count).asSpan<byte, T>();
        }
        
        public readonly T[] ToArray()
        {
            return AsSpan().ToArray();
        }
    }
}
