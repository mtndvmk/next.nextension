using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Nextension
{
    public class NNativeList<T> : IList<T>, ICollection<T>, IEnumerable<T>, IDisposable where T : unmanaged
    {
        internal NativeArray<T> i_Items;
        internal int i_Count;
        private Allocator i_Allocator;

        public NNativeList(int initialCapacity, Allocator allocator = Allocator.Persistent)
        {
            i_Allocator = allocator;
            i_Items = new NativeArray<T>(initialCapacity, allocator, NativeArrayOptions.UninitializedMemory);
            i_Count = 0;
        }
        public NNativeList() : this(Allocator.Persistent) { }
        public NNativeList(Allocator allocator) : this(0, allocator)
        {
        }

        public T this[int index]
        {
            get => (uint)index < (uint)i_Count ? i_Items[index] : throw new IndexOutOfRangeException();
            set
            {
                if ((uint)index >= (uint)i_Count) throw new IndexOutOfRangeException();
                i_Items[index] = value;
            }
        }

        public unsafe T GetWithoutChecks(int index)
        {
            return UnsafeUtility.ReadArrayElement<T>(GetUnsafePtr(), index);
        }

        public unsafe T* GetUnsafePtr()
        {
            return (T*)i_Items.GetUnsafePtr();
        }

        public unsafe T* GetUnsafePtr(int index)
        {
            return GetUnsafePtr() + index;
        }

        public unsafe void SetWithoutChecks(int index, T value)
        {
            UnsafeUtility.WriteArrayElement(GetUnsafePtr(), index, value);
        }

        public void ensureCapacity(int capacity)
        {
            if (!i_Items.IsCreated)
            {
                i_Items = new NativeArray<T>(capacity, i_Allocator, NativeArrayOptions.UninitializedMemory);
                return;
            }

            if (i_Items.Length < capacity)
            {
                var newItems = new NativeArray<T>(capacity, i_Allocator, NativeArrayOptions.UninitializedMemory);
                if (i_Count > 0)
                {
                    NativeArray<T>.Copy(i_Items, 0, newItems, 0, i_Count);
                }
                i_Items.Dispose();
                i_Items = newItems;
            }
        }

        public int Count => i_Count;
        public int Capacity => i_Items.Length;
        public bool IsReadOnly => false;
        public bool IsCreated => i_Items.IsCreated;

        public void Add(T item)
        {
            if (i_Count == i_Items.Length)
            {
                ensureCapacity(i_Count == 0 ? 4 : i_Count << 1);
            }
            i_Items[i_Count++] = item;
        }

        public void Clear()
        {
            i_Count = 0;
        }

        public bool Contains(T item)
        {
            return IndexOf(item) >= 0;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (array.Length - arrayIndex < Count) throw new ArgumentException("Destination array is not long enough.");

            for (int i = 0; i < Count; i++)
            {
                array[arrayIndex + i] = i_Items[i];
            }
        }

        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        public int IndexOf(T item)
        {
            for (int i = 0; i < i_Count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(i_Items[i], item))
                {
                    return i;
                }
            }
            return -1;
        }

        public unsafe void Insert(int index, T item)
        {
            if (index < 0 || index > i_Count) throw new ArgumentOutOfRangeException(nameof(index));

            if (i_Count == i_Items.Length)
            {
                ensureCapacity(i_Count == 0 ? 4 : i_Count << 1);
            }

            if (index < i_Count)
            {
                var ptr = GetUnsafePtr();
                UnsafeUtility.MemMove(ptr + index + 1, ptr + index, (i_Count - index) * UnsafeUtility.SizeOf<T>());
            }

            i_Items[index] = item;
            i_Count++;
        }

        public unsafe void InsertRangeWithoutChecks(int index, IEnumerable<T> collection)
        {
            if (collection is ICollection<T> collection2)
            {
                int count = collection2.Count;
                if (count > 0)
                {
                    ensureCapacity(this.i_Count + count);
                    var ptr = GetUnsafePtr();
                    int sizeOfT = UnsafeUtility.SizeOf<T>();

                    if (index < this.i_Count)
                    {
                        UnsafeUtility.MemMove(ptr + index + count, ptr + index, (long)(this.i_Count - index) * sizeOfT);
                    }

                    if (this == collection2)
                    {
                        // Copy prefix to the insertion point
                        UnsafeUtility.MemMove(ptr + index, ptr, (long)index * sizeOfT);
                        // Copy shifted suffix to the second copy position
                        UnsafeUtility.MemMove(ptr + index * 2, ptr + index + count, (long)(this.i_Count - index) * sizeOfT);
                    }
                    else
                    {
                        // Copy external collection into the gap
                        var dstSpan = new Span<T>(ptr + index, count);
                        if (collection2 is NNativeList<T> nativeSrc)
                        {
                            NativeArray<T>.Copy(nativeSrc.i_Items, 0, i_Items, index, count);
                        }
                        else
                        {
                            int i = 0;
                            foreach (var item in collection2)
                            {
                                dstSpan[i++] = item;
                            }
                        }
                    }
                    this.i_Count += count;
                }
            }
            else
            {
                foreach (var item in collection)
                {
                    Insert(index++, item);
                }
            }
        }

        public unsafe void InsertRangeWithoutChecks(int index, ReadOnlySpan<T> span)
        {
            int count = span.Length;
            ensureCapacity(this.i_Count + count);
            var ptr = GetUnsafePtr();
            int sizeOfT = UnsafeUtility.SizeOf<T>();

            if (index < this.i_Count)
            {
                UnsafeUtility.MemMove(ptr + index + count, ptr + index, (long)(i_Count - index) * sizeOfT);
            }

            fixed (void* srcPtr = span)
            {
                UnsafeUtility.MemCpy(ptr + index, srcPtr, (long)count * sizeOfT);
            }
            i_Count += count;
        }

        public unsafe void RemoveAt(int index)
        {
            if (index < 0 || index >= i_Count) throw new ArgumentOutOfRangeException(nameof(index));

            if (index < --i_Count)
            {
                var ptr = GetUnsafePtr();
                UnsafeUtility.MemMove(ptr + index, ptr + index + 1, (long)(i_Count - index) * UnsafeUtility.SizeOf<T>());
            }
        }

        public unsafe void RemoveRange(int index, int count)
        {
            if (index < 0 || count < 0) throw new ArgumentOutOfRangeException(index < 0 ? nameof(index) : nameof(count));
            if (i_Count - index < count) throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");

            if (count > 0)
            {
                int remainCount = i_Count - count - index;
                if (remainCount > 0)
                {
                    var ptr = GetUnsafePtr();
                    UnsafeUtility.MemMove(ptr + index, ptr + index + count, (long)remainCount * UnsafeUtility.SizeOf<T>());
                }
                i_Count -= count;
            }
        }

        public void Dispose()
        {
            if (i_Items.IsCreated)
            {
                i_Items.Dispose();
            }
            i_Count = 0;
        }

        public NativeArray<T> AsArray()
        {
            return i_Items.GetSubArray(0, i_Count);
        }

        public NativeArray<T>.Enumerator GetEnumerator()
        {
            return i_Items.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Span<T> AsSpan()
        {
            return AsArray();
        }

        public Span<T> AsSpan(int count)
        {
            ensureCapacity(count);
            if (count > i_Count) i_Count = count;
            return i_Items.GetSubArray(0, count);
        }

        public void CopyFrom(NNativeList<T> src)
        {
            ensureCapacity(src.i_Count);
            i_Count = src.i_Count;
            if (i_Count > 0)
            {
                NativeArray<T>.Copy(src.i_Items, 0, i_Items, 0, i_Count);
            }
        }

        public unsafe void CopyFrom(IEnumerable<T> src)
        {
            if (src is NNativeList<T> nativeListSrc)
            {
                CopyFrom(nativeListSrc);
            }
            else if (src is ICollection<T> collection)
            {
                ensureCapacity(collection.Count);
                i_Count = collection.Count;
                if (i_Count > 0)
                {
                    if (src is T[] arr)
                    {
                        i_Items.CopyFrom(arr);
                    }
                    else if (src is List<T> list)
                    {
                        var span = list.AsSpan();
                        fixed (void* srcPtr = span)
                        {
                            UnsafeUtility.MemCpy(GetUnsafePtr(), srcPtr, (long)i_Count * UnsafeUtility.SizeOf<T>());
                        }
                    }
                    else
                    {
                        var span = new Span<T>(GetUnsafePtr(), i_Count);
                        int i = 0;
                        foreach (var item in collection)
                        {
                            span[i++] = item;
                        }
                    }
                }
            }
            else
            {
                i_Count = 0;
                foreach (var item in src)
                {
                    Add(item);
                }
            }
        }
    }
}
