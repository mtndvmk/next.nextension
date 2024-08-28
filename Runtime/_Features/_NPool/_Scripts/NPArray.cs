using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Nextension
{
    public class NPArray<T> : NCollectionPool<NPArray<T>, NArray<T>, T>
    {
        public static int PoolCount => _pool == null ? 0 : _pool.PoolCount;
        public static NPArray<T> get()
        {
            var collectionPool = (_pool ??= new()).get();
            collectionPool.startTracking();
            return collectionPool;
        }
        public static NPArray<T> get(IEnumerable<T> collection)
        {
            var collectionPool = get();
            collectionPool._collection.copyFrom(collection);
            return collectionPool;
        }
        public static NPArray<T> get(Span<T> span)
        {
            var collectionPool = get();
            collectionPool._collection.copyFrom(span);
            return collectionPool;
        }
        public static NPArray<T> getWithoutTracking()
        {
            var collectionPool = (_pool ??= new()).get();
            return collectionPool;
        }
        public static NPArray<T> getWithoutTracking(IEnumerable<T> collection)
        {
            var collectionPool = getWithoutTracking();
            collectionPool._collection.copyFrom(collection);
            return collectionPool;
        }
        public static NPArray<T> getWithoutTracking(Span<T> span)
        {
            var collectionPool = getWithoutTracking();
            collectionPool._collection.copyFrom(span);
            return collectionPool;
        }
        private NPArray() : base() { }
        
        public int Capacity => _collection.Capacity;

        public T this[int index]
        {
            get { onAccessed(); return _collection[index]; }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T getAtWithoutChecks(int index)
        {
            onAccessed();
            return _collection.getAtWithoutChecks(index);
        }
        public void AddRange(IEnumerable<T> collection)
        {
            onAccessed();
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
            onAccessed();
            _collection.ensureCapacity(capacity);
        }
        public void copyFrom(IEnumerable<T> collection)
        {
            onAccessed();
            _collection.copyFrom(collection);
        }
        public void copyTo(NPArray<T> dst)
        {
            onAccessed();
            dst.onAccessed();
            dst._collection.copyFrom(this);
        }
        public ArrayEnumerator<T> GetEnumerator()
        {
            onAccessed();
            return _collection.GetEnumerator();
        }
        public T[] toArray()
        {
            onAccessed();
            return asSpan().ToArray();
        }
        public int IndexOf(T item)
        {
            return Collection.IndexOf(item);
        }
    }
}
