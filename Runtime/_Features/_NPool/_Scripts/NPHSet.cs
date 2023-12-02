using System.Collections.Generic;

namespace Nextension
{
    public sealed class NPHSet<T> : NCollectionPool<NPHSet<T>, HashSet<T>, T>
    {
        public static NPHSet<T> get()
        {
            return (_pool ??= new()).get();
        }
        public static NPHSet<T> get(IEnumerable<T> collection)
        {
            var poolList = get();
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