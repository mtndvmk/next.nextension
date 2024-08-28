#if !UNITY_EDITOR
#define NNEXT_DISABLE_NPOOL_TRACKING
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nextension
{
    public abstract class NCollectionPool<TPool, TCollection, T> : IDisposable, IPoolable, ICollection<T>
        where TPool : NCollectionPool<TPool, TCollection, T>
        where TCollection : ICollection<T>, new()
    {
        protected static NPool<TPool> _pool;
        protected TCollection _collection;

        public TCollection Collection => _collection;

        protected NCollectionPool()
        {
            _collection = NUtils.createInstance<TCollection>();
#if !NNEXT_DISABLE_NPOOL_TRACKING
            string id = $"{GetType()}, {_pool.CountAll}";
            _poolTrackable = new(id);
            Debug.Log($"Created {id}");
#endif
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        protected void startTracking()
        {
#if !NNEXT_DISABLE_NPOOL_TRACKING
            _poolTrackable.start();
#endif
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        protected void onAccessed()
        {
#if !NNEXT_DISABLE_NPOOL_TRACKING
            _poolTrackable.updateAccessInfo();
#endif
        }

        public static void release(TPool pool)
        {
            if (!_pool.release(pool))
            {
                Debug.LogWarning("Can't find TPool in usingPool");
            }
            pool.Clear();
        }

#if !NNEXT_DISABLE_NPOOL_TRACKING
        private PoolTracker _poolTrackable;
        void IPoolable.onDespawn()
        {
            _poolTrackable.stop();
        }
#endif
        public void stopTracking()
        {
#if !NNEXT_DISABLE_NPOOL_TRACKING
            _poolTrackable.stop();
#endif
        }

        public void Dispose()
        {
            release(this as TPool);
        }

        public int Count => _collection.Count;
        public bool IsReadOnly => false;

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            onAccessed();
            return _collection.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            onAccessed();
            return _collection.GetEnumerator();
        }

        public void Add(T item)
        {
            onAccessed();
            _collection.Add(item);
        }

        public void Clear()
        {
            onAccessed();
            _collection.Clear();
        }

        public bool Contains(T item)
        {
            onAccessed();
            return _collection.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _collection.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            onAccessed();
            return _collection.Remove(item);
        }
    }
}