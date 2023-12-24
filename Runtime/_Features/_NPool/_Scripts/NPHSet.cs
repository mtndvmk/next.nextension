using System.Collections.Generic;

namespace Nextension
{
    public sealed class NPHSet<T> : NCollectionPool<NPHSet<T>, HashSet<T>, T>
    {
        public static NPHSet<T> get()
        {
            var collectionPool = (_pool ??= new()).get();
            collectionPool.startTracking();
            return collectionPool;
        }
        public static NPHSet<T> get(IEnumerable<T> collection)
        {
            var poolList = get();
            poolList.UnionWith(collection);
            return poolList;
        }
        public static NPHSet<T> getWithoutTracking()
        {
            return (_pool ??= new()).get();
        }
        public static NPHSet<T> getWithoutTracking(IEnumerable<T> collection)
        {
            var poolList = getWithoutTracking();
            poolList.UnionWith(collection);
            return poolList;
        }
        private NPHSet() : base() { }
        public new bool Add(T item)
        {
            onAccessed();
            return _collection.Add(item);
        }

        public void UnionWith(IEnumerable<T> collection)
        {
            onAccessed();
            _collection.UnionWith(collection);
        }
        public void ExceptWith(IEnumerable<T> collection)
        {
            onAccessed();
            _collection.ExceptWith(collection);
        }
        public HashSet<T>.Enumerator GetEnumerator()
        {
            onAccessed();
            return _collection.GetEnumerator();
        }
    }
}