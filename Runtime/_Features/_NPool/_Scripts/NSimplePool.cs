//#define ENABLE_DEBUG_LOG
using System.Collections.Generic;
using UnityEngine;

namespace Nextension
{
    public static class NSimplePool<T> where T : class
    {
        private static readonly List<T> _pool = new();
        public static int PoolCount => _pool.Count;
        public static int CreatedCount { get; private set; }

        public static T get()
        {
            T item;
#if ENABLE_DEBUG_LOG
            if (CreatedCount == 0)
            {
                Debug.Log("[NSimplePool] START " + typeof(T));
            }
            Debug.Log("[NSimplePool] CREATED " + typeof(T));
#endif
            if (_pool.Count > 0)
            {
                item = _pool.takeAndRemoveLast();
            }
            else
            {
                item = NUtils.createInstance<T>();
                CreatedCount++;
            }


            return item;
        }

        public static void release(T item)
        {
            _pool.Add(item);
            if (PoolCount > CreatedCount)
            {
                Debug.LogError($"[NSimplePool] Pool count {PoolCount} exceeded created count {CreatedCount} for type {typeof(T).Name}.");
            }
#if ENABLE_DEBUG_LOG
            Debug.Log("[NSimplePool] RELEASED " + typeof(T));
            if (PoolCount == CreatedCount)
            {
                Debug.Log("[NSimplePool] END " + typeof(T));
            }
#endif
        }
    }
}
