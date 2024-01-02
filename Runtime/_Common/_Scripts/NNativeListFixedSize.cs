using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Nextension
{
    public struct NNativeListFixedSize<T> : IDisposable where T : unmanaged
    {
        private NativeArray<T> _array;
        private int _count;

        public NNativeListFixedSize(int capacity, Allocator allocator = Allocator.Temp)
        {
            _array = new NativeArray<T>(capacity, allocator, NativeArrayOptions.UninitializedMemory);
            _count = 0;
        }
        public NNativeListFixedSize(T[] array, Allocator allocator = Allocator.Temp)
        {
            _array = new NativeArray<T>(array, allocator);
            _count = array.Length;
        }

        public T this[int index] { get => _array[index]; set => _array[index] = value; }
        public int Count => _count;
        public bool IsCreated => _array.IsCreated;

        public unsafe void AddRangeNoReSize(T[] array)
        {
            fixed (void* srcPtr = array)
            {
                int stride = NUtils.sizeOf<T>();
                var dstPtr = (byte*)_array.GetUnsafePtr() + stride * _count;
                UnsafeUtility.MemCpy(dstPtr, srcPtr, stride * array.Length);
            }
            _count += array.Length;
        }
        public void AddNoResize(T item)
        {
            _array[_count++] = item;
        }
        public void RemoveAtSwapback(int index)
        {
            _array[index] = _array[--_count];
        }
        public T TakeAndRemoveLast()
        {
            T item = _array[--_count];
            return item;
        }
        public void Clear()
        {
            _count = 0;
        }
        public void CopyTo(T[] array)
        {
            _array.CopyTo(array);
        }
        public void Dispose()
        {
            if (IsCreated)
            {
                _array.Dispose();
                _count = 0;
            }
        }
        public NativeSlice<T> Slice()
        {
            return _array.Slice(0, _count);
        }
        public NativeSlice<T>.Enumerator GetEnumerator()
        {
            var slice = Slice();
            return new NativeSlice<T>.Enumerator(ref slice);
        }
    }
}
