using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Nextension
{
    public interface IPoolable
    {
        public const uint DEFAULT_MAX_POOL_ITEM_COUNT = 100;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] void onCreate() { }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] void onSpawn() { }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] void onDespawn() { }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] void onDestroy() { }
    }
    public class NPool<T> where T : class, IPoolable
    {
        [NonSerialized] private readonly HashSet<T> _pool;
        [NonSerialized] private int _countAll;

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
                item.onCreate();
                _countAll++;
            }
            item.onSpawn();
            return item;
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
                    item.onDestroy();
                }
                _pool.Clear();
            }
        }
    }

    public static class NStaticPool<T> where T : class, IPoolable
    {
        private static NPool<T> _pool = new();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T get() => _pool.get();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool release(T item) => _pool.release(item);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T getAndRelease(IWaitable releaseWaitable) => _pool.getAndRelease(releaseWaitable);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T getAndRelease(float time) => _pool.getAndRelease(time);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void clear() => _pool.clear();
    }
}
