using System;
using System.Collections.Generic;
using Unity.Collections;

namespace Nextension
{
    public sealed class NPNativeList<T> : NCollectionPool<NPNativeList<T>, NNativeList<T>, T>, IList<T>, IReadOnlyList<T>, IPoolable where T : unmanaged
    {
        public static NPNativeList<T> get()
        {
            var collectionPool = Pool.get();
            collectionPool.startTracking();
            return collectionPool;
        }

        public static NPNativeList<T> get(int initialCapacity)
        {
            var collectionPool = get();
            collectionPool._collection.ensureCapacity(initialCapacity);
            return collectionPool;
        }

        public static NPNativeList<T> get(IEnumerable<T> collection)
        {
            var collectionPool = get();
            collectionPool.AddRange(collection);
            return collectionPool;
        }

        public static NPNativeList<T> getWithoutTracking()
        {
            var collectionPool = Pool.get();
            return collectionPool;
        }

        public static NPNativeList<T> getWithoutTracking(int initialCapacity)
        {
            var collectionPool = getWithoutTracking();
            collectionPool._collection.ensureCapacity(initialCapacity);
            return collectionPool;
        }

        public static NPNativeList<T> getWithoutTracking(IEnumerable<T> collection)
        {
            var collectionPool = getWithoutTracking();
            collectionPool.AddRange(collection);
            return collectionPool;
        }

        public int Capacity => _collection.Capacity;

        private NPNativeList() : base()
        {
            DisposeManager.add(Pool);
        }

        public T this[int index]
        {
            get => _collection[index];
            set
            {
                onAccessed();
                _collection[index] = value;
            }
        }

        public void AddRange(IEnumerable<T> collection)
        {
            onAccessed();
            foreach (var item in collection)
            {
                _collection.Add(item);
            }
        }

        public void RemoveAt(int index)
        {
            onAccessed();
            _collection.RemoveAt(index);
        }

        public int IndexOf(T item)
        {
            onAccessed();
            return _collection.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            onAccessed();
            _collection.Insert(index, item);
        }

        public void InsertRangeWithoutChecks(int index, ReadOnlySpan<T> span)
        {
            onAccessed();
            _collection.InsertRangeWithoutChecks(index, span);
        }

        public NativeArray<T> AsArray()
        {
            onAccessed();
            return _collection.AsArray();
        }

        void IPoolable.onDestroy()
        {
            _collection.Dispose();
        }

        public void CopyFrom(NPNativeList<T> src)
        {
            onAccessed();
            _collection.CopyFrom(src._collection);
        }
        public void CopyFrom(IEnumerable<T> collection)
        {
            onAccessed();
            _collection.CopyFrom(collection);
        }
        public Span<T> AsSpan()
        {
            onAccessed();
            return _collection.AsSpan();
        }
        public NativeArray<T>.Enumerator GetEnumerator()
        {
            onAccessed();
            return _collection.GetEnumerator();
        }
    }
}
