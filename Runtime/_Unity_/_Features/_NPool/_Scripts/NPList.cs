using System;
using System.Collections.Generic;

namespace Nextension
{
    public sealed class NPList<T> : NCollectionPool<NPList<T>, List<T>, T>, IList<T>, IReadOnlyList<T>
    {
        public static NPList<T> get()
        {
            var collectionPool = Pool.get();
            collectionPool.startTracking();
            return collectionPool;
        }
        public static NPList<T> get(IEnumerable<T> collection)
        {
            var poolList = get();
            poolList.AddRange(collection);
            return poolList;
        }
        public static NPList<T> getWithoutTracking()
        {
            return Pool.get();
        }
        public static NPList<T> getWithoutTracking(IEnumerable<T> collection)
        {
            var poolList = getWithoutTracking();
            poolList.AddRange(collection);
            return poolList;
        }
        private NPList() : base() { }
        public T this[int index]
        {
            get { return _collection[index]; }
            set { _collection[index] = value; }
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
        public Span<T> AsSpan()
        {
            onAccessed();
            return _collection.AsSpan();
        }
        public List<T>.Enumerator GetEnumerator()
        {
            onAccessed();
            return _collection.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return _collection.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            _collection.Insert(index, item);
        }
    }
}