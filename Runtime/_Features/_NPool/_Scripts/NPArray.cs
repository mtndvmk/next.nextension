using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Nextension
{
    public class NPArray<T> : NCollectionPool<NPArray<T>, NArray<T>, T>
    {
        public static NPArray<T> get()
        {
            return (_pool ??= new()).get();
        }
        public static NPArray<T> get(IEnumerable<T> collection)
        {
            var poolList = get();
            poolList._collection.copyFrom(collection);
            return poolList;
        }
        public static NPArray<T> get(Span<T> span)
        {
            var poolList = get();
            poolList._collection.copyFrom(span);
            return poolList;
        }
        private NPArray() : base() { }
        
        public T this[int index]
        {
            get { return _collection[index]; }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T getWithoutChecks(int index)
        {
            return _collection.getWithoutChecks(index);
        }
        public void AddRange(IEnumerable<T> collection)
        {
            _collection.AddRange(collection);
        }
        public void RemoveAt(int index)
        {
            onAccessed();
            _collection.RemoveAt(index);
        }
        public Span<T> asSpan()
        {
            onAccessed();
            return _collection.asSpan();
        }
        public void ensureCapacity(int capacity)
        {
            _collection.ensureCapacity(capacity);
        }
        public void copyFrom(IEnumerable<T> collection)
        {
            _collection.copyFrom(collection);
        }
        public ArrayEnumerator<T> GetEnumerator()
        {
            onAccessed();
            return _collection.GetEnumerator();
        }
    }
}
