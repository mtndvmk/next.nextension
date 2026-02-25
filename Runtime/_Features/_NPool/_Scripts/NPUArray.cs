using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Nextension
{
    public readonly struct NPUArray<T> : IList<T>, IDisposable where T : unmanaged
    {
        public static NPUArray<T> get()
        {
            var newArrayInt32 = new NPUArray<T>(NPArray<byte>.get());
            return newArrayInt32;
        }
        public static NPUArray<T> get(T t0)
        {
            var newArrayInt32 = new NPUArray<T>(NPArray<byte>.get())
            {
                t0
            };
            return newArrayInt32;
        }
        public static NPUArray<T> get(T t0, T t1)
        {
            var newArrayInt32 = new NPUArray<T>(NPArray<byte>.get())
            {
                t0,
                t1
            };
            return newArrayInt32;
        }
        public static NPUArray<T> get(T t0, T t1, T t2)
        {
            var newArrayInt32 = new NPUArray<T>(NPArray<byte>.get())
            {
                t0,
                t1,
                t2
            };
            return newArrayInt32;
        }
        public static NPUArray<T> get(T t0, T t1, T t2, T t3)
        {
            var newArrayInt32 = new NPUArray<T>(NPArray<byte>.get())
            {
                t0,
                t1,
                t2,
                t3
            };
            return newArrayInt32;
        }
        public static NPUArray<T> get(int capacity)
        {
            var newArrayInt32 = new NPUArray<T>(NPArray<byte>.get());
            newArrayInt32.ensureCapacity(capacity);
            return newArrayInt32;
        }
        public static NPUArray<T> get(IEnumerable<T> collection)
        {
            var newArrayInt32 = new NPUArray<T>(NPArray<byte>.get());
            newArrayInt32.AddRange(collection);
            return newArrayInt32;
        }
        public static NPUArray<T> getWithoutTracking()
        {
            var newArrayInt32 = new NPUArray<T>(NPArray<byte>.getWithoutTracking());
            return newArrayInt32;
        }
        public static NPUArray<T> getWithoutTracking(int capacity)
        {
            var newArrayInt32 = new NPUArray<T>(NPArray<byte>.getWithoutTracking());
            newArrayInt32.ensureCapacity(capacity);
            return newArrayInt32;
        }
        public static NPUArray<T> getWithoutTracking(IEnumerable<T> collection)
        {
            var newArrayInt32 = new NPUArray<T>(NPArray<byte>.getWithoutTracking());
            newArrayInt32.AddRange(collection);
            return newArrayInt32;
        }

        public void stopTracking()
        {
            i_array.stopTracking();
        }

        private NPUArray(NPArray<byte> arr)
        {
            i_array = arr;
        }

        internal readonly NPArray<byte> i_array;

        public int Count => i_array.Count / NUtils.sizeOf<T>();
        public bool IsCreated => i_array != null;
        public bool IsReadOnly => i_array.IsReadOnly;
        public int Capacity => i_array.Capacity / NUtils.sizeOf<T>();
        public bool IsDisposed => i_array.IsDisposed;
        public T[] Items => ToArray();

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
            return NConverter.fromBytesWithoutChecks<T>(i_array.Collection.i_Items, index * NUtils.sizeOf<T>());
        }
        public unsafe void SetWithoutChecks(int index, T value)
        {
            var collection = i_array.Collection;
            fixed (byte* bPtr = collection.i_Items)
            {
                T* ptr = (T*)bPtr;
                ptr[index] = value;
            }
        }

        public void ensureCapacity(int capacity)
        {
            var capacityInBytes = capacity * NUtils.sizeOf<T>();
            ensureCapacityInBytes(capacityInBytes);
        }
        public void ensureCapacityInBytes(int capacityInBytes)
        {
            var byteArray = i_array.Collection;
            if (byteArray.Capacity >= capacityInBytes) return;
            byteArray.ensureCapacity(capacityInBytes > 16 ? capacityInBytes : 16);
        }
        public unsafe void Add(T item)
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
        public unsafe void AddRange(T[] items)
        {
            AddRange(items.AsSpan());
        }
        public unsafe void AddRange(ReadOnlySpan<T> items)
        {
            if (items.Length == 0) return;
            var addSizeInBytes = NUtils.sizeOf<T>() * items.Length;
            var byteArray = i_array.Collection;
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
        public unsafe void AddRange(ICollection<T> collection)
        {
            int addSizeInBytes = collection.Count * NUtils.sizeOf<T>();
            int startIndex = Count;
            var byteArray = i_array.Collection;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            i_array.Clear();
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
                var byteArray = i_array.Collection;
                int sizeInBytes = byteArray.Count;
                byte* dstPtr = (byte*)tPtr;
                fixed (byte* srcPtr = byteArray.i_Items)
                {
                    Buffer.MemoryCopy(srcPtr, dstPtr, sizeInBytes, sizeInBytes);
                }
            }
        }
        public void CopyTo(NPUArray<T> dst)
        {
            dst.ensureCapacity(Count);
            dst.i_array.CopyFrom(i_array);
        }
        public unsafe bool Remove(T item)
        {
            var byteArray = i_array.Collection;
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
            if ((uint)index >= i_array.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            var byteArray = i_array.Collection;
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
            var collection = i_array.Collection;
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
            var sizeOfT = NUtils.sizeOf<T>();
            Span<byte> span = stackalloc byte[sizeOfT];
            fixed (byte* ptr = span)
            {
                *(T*)ptr = item;
            }
            i_array.Collection.InsertRangeWithoutChecks(index * sizeOfT, span);
        }

        public unsafe void InsertRange(int index, ReadOnlySpan<T> span)
        {
            fixed (void* ptr = span)
            {
                var sizeOfT = NUtils.sizeOf<T>();
                ReadOnlySpan<byte> bSpan = new ReadOnlySpan<byte>(ptr, span.Length * sizeOfT);
                i_array.Collection.InsertRangeWithoutChecks(index * sizeOfT, bSpan);
            }
        }

        public unsafe void InsertRange(int index, T* ptr, int length)
        {
            var sizeOfT = NUtils.sizeOf<T>();

            var byteIndex = index * sizeOfT;
            var byteLength = length * sizeOfT;
            var byteCount = i_array.Count;
            ensureCapacityInBytes(byteCount + byteLength);

            var byteArray = i_array.Collection.i_Items;

            fixed (byte* byteArrPtr = byteArray)
            {
                var wPtr = byteArrPtr + byteIndex;
                if (byteIndex < byteCount)
                {
                    var tmpCount = byteCount - byteIndex;
                    Buffer.MemoryCopy(wPtr, wPtr + byteLength, tmpCount, tmpCount);
                }

                Buffer.MemoryCopy((byte*)ptr, wPtr, byteLength, byteLength);
                i_array.Collection.i_Count += byteLength;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            i_array.Dispose();
        }

        private unsafe Enumerator GetUnsafeEnumerator()
        {
            return new Enumerator(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetUnsafeEnumerator();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetUnsafeEnumerator();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator()
        {
            return GetUnsafeEnumerator();
        }
        public unsafe Span<T> AsSpan()
        {
            return i_array.Collection.AsSpan().asSpan<byte, T>();
        }
        public T[] ToArray()
        {
            return AsSpan().ToArray();
        }
        public unsafe struct Enumerator : IEnumerator<T>
        {
            public Enumerator(NPUArray<T> targetArray)
            {
                this.targetArray = targetArray;
                fixed (byte* intPtr = targetArray.i_array.Collection.i_Items)
                {
                    this.array = (T*)intPtr;
                }
                count = (uint)targetArray.Count;
                _current = default;
                _index = 0;
            }

            internal readonly NPUArray<T> targetArray;
            internal readonly T* array;
            internal readonly uint count;

            private uint _index;
            private T _current;

            public readonly T Current => _current;
            readonly object IEnumerator.Current => _current;

            public void Dispose()
            {
                targetArray.Dispose();
            }
            public bool MoveNext()
            {
                if (_index < count)
                {
                    _current = array[_index++];
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
