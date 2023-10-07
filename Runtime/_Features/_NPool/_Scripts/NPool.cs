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
    public class NPool<T> where T : class, IPoolable, new()
    {
        private readonly NBListCompareHashCode<T> _pool;

        public int CountAll { get; private set; }
        public int PoolCount => _pool.Count;
        public uint MaxPoolItemCount { get; set; }
        public NPool()
        {
            _pool = new NBListCompareHashCode<T>();
            MaxPoolItemCount = IPoolable.DEFAULT_MAX_POOL_ITEM_COUNT;
        }
        public T get()
        {
            T item;
            if (_pool.Count > 0)
            {
                item = _pool.takeAndRemoveLast();
            }
            else
            {
                item = new();
                CountAll++;
            }
            item.onSpawned();
            return item;
        }
        public bool release(T item)
        {
            var isExist = _pool.bFindInsertIndex(item, out var insertIndex);
            if (isExist)
            {
                return false;
            }

            if (_pool.Count >= MaxPoolItemCount)
            {
                item.onDestroyed();
                return true;
            }
            else
            {
                item.onDespawned();
                _pool.insert(insertIndex, item);
                return true;
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
                CountAll -= _pool.Count;
                foreach (var item in _pool.asEnumerable())
                {
                    item.onDestroyed();
                }
                _pool.clear();
            }
        }
    }
}
