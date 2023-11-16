using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Nextension
{
    public interface IPoolable
    {
        public const uint DEFAULT_MAX_POOL_ITEM_COUNT = 100;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void onSpawned() { }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void onDespawned() { }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void onDestroyed() { }
    }
    public class NPool<T> where T : class, IPoolable
    {
        private readonly HashSet<T> _pool;
        private int _countAll;

        [NonSerialized] public uint maxPoolItemCount;

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
                _countAll++;
            }
            item.onSpawned();
            return item;
        }
        public bool release(T item)
        {
            if (_pool.Count >= maxPoolItemCount)
            {
                item.onDestroyed();
                return true;
            }
            else
            {
                if (_pool.Add(item))
                {
                    item.onDespawned();
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T getAndRelease(float time)
        {
            return getAndRelease(new NWaitSecond(time));
        }

        public void clear()
        {
            if (_pool.Count > 0)
            {
                _countAll -= _pool.Count;
                foreach (var item in _pool)
                {
                    item.onDestroyed();
                }
                _pool.Clear();
            }
        }
    }
}
