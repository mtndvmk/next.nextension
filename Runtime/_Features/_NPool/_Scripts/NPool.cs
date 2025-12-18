using System;
using System.Collections.Generic;

namespace Nextension
{
    public interface IPoolable
    {
        public const int DEFAULT_MAX_POOL_ITEM_COUNT = 100;
        void onCreated() { }
        void onSpawn() { }
        void onDespawn() { }
        void onDestroy() { }
    }
    public class NPool<T> where T : class, IPoolable
    {
        [NonSerialized] private readonly HashSet<T> _pool;
        [NonSerialized] private int _countAll;

        [NonSerialized] public int maxPoolItemCount;

        public int CountAll => _countAll;
        public int PoolCount => _pool.Count;

        public NPool()
        {
            _pool = new();
            maxPoolItemCount = IPoolable.DEFAULT_MAX_POOL_ITEM_COUNT;
        }
        public T get()
        {
            T item;
            if (_pool.Count > 0)
            {
                item = _pool.takeAndRemoveFirst();
            }
            else
            {
                item = NUtils.createInstance<T>();
                item.onCreated();
                _countAll++;
            }
            item.onSpawn();
            return item;
        }
        public bool contains(T item)
        {
            return _pool.Contains(item);
        }
        public bool release(T item)
        {
            if (_pool.Count >= maxPoolItemCount)
            {
                item.onDestroy();
                return true;
            }
            else
            {
                if (_pool.Add(item))
                {
                    item.onDespawn();
                    return true;
                }
                return false;
            }
        }
        public T getAndRelease(IWaitable releaseWaitable)
        {
            T item = get();
            releaseWaitable.startWaitable().addCompletedEvent(() =>
            {
                release(item);
            });
            return item;
        }
        public T getAndRelease(float time)
        {
            return getAndRelease(new NWaitSecond(time));
        }
        public void clear(int keepCount = 0)
        {
            if (_pool.Count > keepCount)
            {
                _countAll -= _pool.Count;
                foreach (var item in _pool)
                {
                    item.onDestroy();
                }
                _pool.Clear();
            }
        }
        public void clearFitIn()
        {
            clear(maxPoolItemCount);
        }
    }

    public static class NStaticPool<T> where T : class, IPoolable
    {
        private static NPool<T> _pool = new();
        public static T get() => _pool.get();
        public static bool release(T item) => _pool.release(item);
        public static T getAndRelease(IWaitable releaseWaitable) => _pool.getAndRelease(releaseWaitable);
        public static T getAndRelease(float time) => _pool.getAndRelease(time);
        public static void clear() => _pool.clear();
    }
}
