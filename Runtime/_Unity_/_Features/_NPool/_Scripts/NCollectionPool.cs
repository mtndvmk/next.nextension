#if !UNITY_EDITOR
#define NNEXT_DISABLE_NPOOL_TRACKING
#endif
using System;
using System.Collections;
using System.Collections.Generic;

namespace Nextension
{
    public abstract class NCollectionPool<TPool, TCollection, T> : IDisposable, IPoolable, ICollection<T>
        where TPool : NCollectionPool<TPool, TCollection, T>
        where TCollection : ICollection<T>, new()
    {
        protected TCollection _collection;

        internal static NPool<TPool> Pool => NStaticPool<TPool>.getPool();

        public TCollection Collection => _collection;

        public static int PoolCount => Pool.PoolCount;

        public bool IsDisposed => Pool.contains(this as TPool);

        protected NCollectionPool()
        {
            _collection = NUtils.createInstance<TCollection>();
#if !NNEXT_DISABLE_NPOOL_TRACKING
            string id = $"{GetType()}, {Pool.CountAll}";
            _poolTrackable = new PoolTracker(id);
            NDebug.Log($"Created {id}");
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

        internal static void release(TPool pool)
        {
            if (!pool.IsDisposed && Pool.release(pool))
            {
                pool.Clear();
            }
        }

        public static void setMaxItemInPool(int count)
        {
            Pool.maxPoolItemCount = count;
            Pool.clearFitIn();
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
