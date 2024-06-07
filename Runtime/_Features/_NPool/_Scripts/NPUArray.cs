using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Nextension
{
    public struct NPUArray<T> : IList<T>, IDisposable where T : unmanaged
    {
        public static NPUArray<T> get()
        {
            var newArrayInt32 = new NPUArray<T>
            {
                _array = NPArray<byte>.get()
            };
            return newArrayInt32;
        }
        public static NPUArray<T> get(IEnumerable<T> collection)
        {
            var newArrayInt32 = new NPUArray<T>
            {
                _array = NPArray<byte>.get()
            };
            newArrayInt32.AddRange(collection);
            return newArrayInt32;
        }
        public static NPUArray<T> getWithoutTracking()
        {
            var newArrayInt32 = new NPUArray<T>
            {
                _array = NPArray<byte>.getWithoutTracking()
            };
            return newArrayInt32;
        }
        public static NPUArray<T> getWithoutTracking(IEnumerable<T> collection)
        {
            var newArrayInt32 = new NPUArray<T>
            {
                _array = NPArray<byte>.getWithoutTracking()
            };
            newArrayInt32.AddRange(collection);
            return newArrayInt32;
        }

        private NPArray<byte> _array;

        public int Count => _array.Count / NUtils.sizeOf<T>();
        public bool IsCreated => _array != null;
        public bool IsReadOnly => _array.IsReadOnly;

        public T this[int index]
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

        public T GetWithoutChecks(int index)
        {
            return NConverter.fromBytesWithoutChecks<T>(_array.Collection.i_Items, index * NUtils.sizeOf<T>());
        }
        public unsafe void SetWithoutChecks(int index, T value)
        {
            var collection = _array.Collection;
            fixed (byte* bPtr = collection.i_Items)
            {
                T* ptr = (T*)bPtr;
                ptr[index] = value;
            }
        }

        public void ensureCapacityInBytes(int capacityInBytes)
        {
            var byteArray = _array.Collection;
            if (byteArray.Capacity >= capacityInBytes) return;
            byteArray.ensureCapacity(capacityInBytes > 16 ? capacityInBytes : 16);
        }
        public unsafe void Add(T item)
        {
            var sizeOfT = NUtils.sizeOf<T>();
            var byteArray = _array.Collection;
            if (byteArray.Capacity == 0)
            {
                byteArray.ensureCapacity(16);
            }
            else if (byteArray.Count + sizeOfT > byteArray.Capacity)
            {
                byteArray.ensureCapacity(byteArray.Capacity << 1);
            }
            fixed (byte* bPtr = byteArray.i_Items)
            {
                ((T*)bPtr)[byteArray.i_Count / sizeOfT] = item;
                byteArray.i_Count += sizeOfT;
            }
        }
        public unsafe void AddRange(IEnumerable<T> collection)
        {
            if (collection is ICollection<T> collection2)
            {
                AddRange(collection2);
            }
            else
            {
                foreach (var item in collection)
                {
                    Add(item);
                }
            }
        }
        public unsafe void AddRange(ICollection<T> collection)
        {
            int addSizeInBytes = collection.Count * NUtils.sizeOf<T>();
            int startIndex = Count;
            var byteArray = _array.Collection;
            ensureCapacityInBytes(addSizeInBytes + byteArray.i_Count);
            fixed (byte* bPtr = byteArray.i_Items)
            {
                foreach (var item in collection)
                {
                    ((T*)bPtr)[startIndex++] = item;
                }
                byteArray.i_Count += addSizeInBytes;
            }
        }
        public unsafe void AddRange(T[] items)
        {
            var addSizeInBytes = NUtils.sizeOf<T>() * items.Length;
            var byteArray = _array.Collection;
            ensureCapacityInBytes(addSizeInBytes + byteArray.i_Count);
            fixed (T* tPtr = items)
            {
                fixed (byte* dst = &byteArray.i_Items[byteArray.i_Count])
                {
                    Buffer.MemoryCopy(tPtr, dst, addSizeInBytes, addSizeInBytes);
                    byteArray.i_Count += addSizeInBytes;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _array.Clear();
        }
        public bool Contains(T item)
        {
            return IndexOf(item) >= 0;
        }
        public int FirstIndexOf(Predicate<T> predicate)
        {
            for (int i = 0; i < Count; i++)
            {
                if (predicate(GetWithoutChecks(i))) return i;
            }
            return -1;
        }
        public unsafe void CopyTo(T[] array, int arrayIndex)
        {
            if (arrayIndex + Count > array.Length)
            {
                throw new ArgumentOutOfRangeException();
            }
            fixed (T* tPtr = &array[arrayIndex])
            {
                var byteArray = _array.Collection;
                int sizeInBytes = byteArray.Count;
                byte* dstPtr = (byte*)tPtr;
                fixed (byte* srcPtr = byteArray.i_Items)
                {
                    Buffer.MemoryCopy(srcPtr, dstPtr, sizeInBytes, sizeInBytes);
                }
            }
        }
        public unsafe bool Remove(T item)
        {
            var byteArray = _array.Collection;
            var sizeOfT = NUtils.sizeOf<T>();
            int tCount = byteArray.i_Count / sizeOfT;
            fixed (byte* bPtr = byteArray.i_Items)
            {
                T* ptr = (T*)bPtr;
                for (int i = 0; i < tCount;)
                {
                    if (ptr[i++].equals(item))
                    {
                        int remainCount = tCount - i;
                        if (remainCount > 0)
                        {
                            var sizeInBytes = remainCount * sizeOfT;
                            Buffer.MemoryCopy(&ptr[i], &ptr[i - 1], sizeInBytes, sizeInBytes);
                        }
                        byteArray.i_Count -= sizeOfT;
                        return true;
                    }
                }
            }
            return false;
        }
        public unsafe void RemoveAt(int index)
        {
            if ((uint)index >= _array.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            var byteArray = _array.Collection;
            var sizeOfT = NUtils.sizeOf<T>();
            int tCount = byteArray.i_Count / sizeOfT;
            fixed (byte* bPtr = byteArray.i_Items)
            {
                T* ptr = (T*)bPtr;
                int remainCount = tCount - 1 - index;
                if (remainCount > 0)
                {
                    var sizeInBytes = remainCount * sizeOfT;
                    Buffer.MemoryCopy(&ptr[index + 1], &ptr[index], sizeInBytes, sizeInBytes);
                }
                byteArray.i_Count -= sizeOfT;
            }
        }
        public unsafe int IndexOf(T item)
        {
            var collection = _array.Collection;
            var sizeOfT = NUtils.sizeOf<T>();
            int tCount = collection.i_Count / sizeOfT;
            fixed (byte* bPtr = collection.i_Items)
            {
                T* ptr = (T*)bPtr;
                for (int i = 0; i < tCount; i++)
                {
                    if (ptr[i].equals(item))
                    {
                        return i;
                    }
                }
            }
            return -1;
        }
        public unsafe void Insert(int index, T item)
        {
            Span<byte> span = stackalloc byte[NUtils.sizeOf<T>()];
            fixed (byte* ptr = span)
            {
                *(T*)ptr = item;
            }
            _array.Collection.InsertRangeWithoutChecks(index * NUtils.sizeOf<T>(), span);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _array.Dispose();
        }

        private unsafe UnsafeArrayEnumerator<T> GetUnsafeArrayEnumerator()
        {
            fixed (byte* intPtr = _array.Collection.i_Items)
            {
                return new UnsafeArrayEnumerator<T>(intPtr, (uint)Count);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetUnsafeArrayEnumerator();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetUnsafeArrayEnumerator();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeArrayEnumerator<T> GetEnumerator()
        {
            return GetUnsafeArrayEnumerator();
        }
        public unsafe Span<T> AsSpan()
        {
            return _array.Collection.asSpan().asSpan<byte, T>();
        }
        public T[] ToArray()
        {
            return AsSpan().ToArray();
        }
    }
}
