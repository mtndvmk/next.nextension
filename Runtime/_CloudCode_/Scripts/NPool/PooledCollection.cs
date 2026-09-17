
using System;
using System.Collections;
using System.Collections.Generic;

namespace Nextension
{
    public abstract class PooledCollection<T, TCollection, TPoolable> : AbsPoolable, IDisposable
        where TCollection : class, ICollection<T>
        where TPoolable : PooledCollection<T, TCollection, TPoolable>
    {
        public const int DEFAULT_CAPACITY_THRESHOLD = 4096;

        private static int _capacityThreshold = DEFAULT_CAPACITY_THRESHOLD;

        public static int CapacityThreshold
        {
            get => _capacityThreshold;
            set => _capacityThreshold = value < 0 ? 0 : value;
        }

        private static NPool<TCollection> _valuePool => NPool<TCollection>.Shared;

        private static NPool<TPoolable> _pPool => NPool<TPoolable>.Shared;

        protected static TPoolable __get()
        {
            var p = _pPool.Rent().value;
            p.Collection = _valuePool.Rent().value;
            return p;
        }

        public TCollection Collection { get; protected set; }

        public int Count => Collection.Count;

        protected PooledCollection() { }

        protected virtual int __getCapacity() => Collection.Count;

        protected override void onDespawn()
        {
            base.onDespawn();

            var collection = Collection;
            var isOversized = __getCapacity() > _capacityThreshold;
            Collection = null;

            if (isOversized)
            {
                return;
            }

            collection.Clear();
            _valuePool.Return(collection);
        }

        public void Dispose()
        {
            if (IsRented)
            {
                _pPool.Return(this as TPoolable);
            }
        }

        public void SafeReturn(long returnToken)
        {
            if (IsRented)
            {
                _pPool.SafeReturn(this as TPoolable, returnToken);
            }
        }
    }

    public abstract class PooledList<T, TCollection, TPoolable> : PooledCollection<T, TCollection, TPoolable>, IList<T>, IReadOnlyList<T>
        where TCollection : class, ICollection<T>, IList<T>
        where TPoolable : PooledCollection<T, TCollection, TPoolable>
    {

        public T this[int index] { get => Collection[index]; set => Collection[index] = value; }

        public bool IsReadOnly => Collection.IsReadOnly;

        public void Add(T item)
        {
            Collection.Add(item);
        }

        public void Clear()
        {
            Collection.Clear();
        }

        public bool Contains(T item)
        {
            return Collection.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Collection.CopyTo(array, arrayIndex);
        }

        public int IndexOf(T item)
        {
            return Collection.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            Collection.Insert(index, item);
        }

        public bool Remove(T item)
        {
            return Collection.Remove(item);
        }

        public void RemoveAt(int index)
        {
            Collection.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Collection.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return Collection.GetEnumerator();
        }
    }

    public abstract class PooledSet<T, TCollection, TPoolable> : PooledCollection<T, TCollection, TPoolable>, ISet<T>, IReadOnlyCollection<T>
        where TCollection : class, ICollection<T>, ISet<T>
        where TPoolable : PooledCollection<T, TCollection, TPoolable>
    {
        public bool IsReadOnly => Collection.IsReadOnly;

        public void Add(T item) => Collection.Add(item);
        public bool AddIfNotPresent(T item) => Collection.Add(item);
        public void Clear() => Collection.Clear();
        public bool Contains(T item) => Collection.Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => Collection.CopyTo(array, arrayIndex);
        public bool Remove(T item) => Collection.Remove(item);

        public void ExceptWith(IEnumerable<T> other) => Collection.ExceptWith(other);
        public void IntersectWith(IEnumerable<T> other) => Collection.IntersectWith(other);
        public bool IsProperSubsetOf(IEnumerable<T> other) => Collection.IsProperSubsetOf(other);
        public bool IsProperSupersetOf(IEnumerable<T> other) => Collection.IsProperSupersetOf(other);
        public bool IsSubsetOf(IEnumerable<T> other) => Collection.IsSubsetOf(other);
        public bool IsSupersetOf(IEnumerable<T> other) => Collection.IsSupersetOf(other);
        public bool Overlaps(IEnumerable<T> other) => Collection.Overlaps(other);
        public bool SetEquals(IEnumerable<T> other) => Collection.SetEquals(other);
        public void SymmetricExceptWith(IEnumerable<T> other) => Collection.SymmetricExceptWith(other);
        public void UnionWith(IEnumerable<T> other) => Collection.UnionWith(other);
        bool ISet<T>.Add(T item) => Collection.Add(item);

        IEnumerator IEnumerable.GetEnumerator() => Collection.GetEnumerator();
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => Collection.GetEnumerator();
    }

    public abstract class PooledDictionary<TKey, TValue, TCollection, TPoolable> : PooledCollection<KeyValuePair<TKey, TValue>, TCollection, TPoolable>, IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
        where TCollection : class, ICollection<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>
        where TPoolable : PooledCollection<KeyValuePair<TKey, TValue>, TCollection, TPoolable>
    {
        public TValue this[TKey key] { get => Collection[key]; set => Collection[key] = value; }

        public ICollection<TKey> Keys => Collection.Keys;
        public ICollection<TValue> Values => Collection.Values;
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Collection.Keys;
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Collection.Values;

        public bool IsReadOnly => Collection.IsReadOnly;

        public void Add(TKey key, TValue value) => Collection.Add(key, value);
        public void Add(KeyValuePair<TKey, TValue> item) => Collection.Add(item);
        public void Clear() => Collection.Clear();
        public bool Contains(KeyValuePair<TKey, TValue> item) => Collection.Contains(item);
        public bool ContainsKey(TKey key) => Collection.ContainsKey(key);
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => Collection.CopyTo(array, arrayIndex);
        public bool Remove(TKey key) => Collection.Remove(key);
        public bool Remove(KeyValuePair<TKey, TValue> item) => Collection.Remove(item);
        public bool TryGetValue(TKey key, out TValue value) => Collection.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => Collection.GetEnumerator();
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => Collection.GetEnumerator();
    }
}