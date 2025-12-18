using System.Collections.Generic;
using UnityEngine;

namespace Nextension
{
    internal static class SharedInsPoolUtil
    {
#if UNITY_EDITOR
        [EditorQuittingMethod]
        static void reset()
        {
            foreach (var sharedPool in _sharedPools.Values)
            {
                sharedPool.clearPool();
            }
            _sharedPools.Clear();
        }
#endif
        private readonly static Dictionary<int, InsPool<GameObject>> _sharedPools = new();
        public static InsPool<GameObject> getOrCreatePool<T>(T component, int startupInstanceCount) where T : Object
        {
            return getOrCreatePool(InsPoolUtil.getGameObject(component), startupInstanceCount);
        }
        public static InsPool<GameObject> getOrCreatePool(GameObject prefab, int startupInstanceCount)
        {
            return getOrCreatePool(InsPoolUtil.computePoolId(prefab), prefab, startupInstanceCount);
        }
        internal static InsPool<GameObject> getOrCreatePool(int poolId, GameObject prefab, int startupInstanceCount)
        {
            if (!_sharedPools.TryGetValue(poolId, out var pool))
            {
                pool = new InsPool<GameObject>(prefab, startupInstanceCount, true);
                _sharedPools.Add(poolId, pool);
            }
            pool.updateStartupInstances(startupInstanceCount);
            return pool;
        }
        public static InsPool<GameObject> getPool<T>(T component) where T : Object
        {
            return getPool(InsPoolUtil.getGameObject(component));
        }
        public static InsPool<GameObject> getPool(GameObject prefab)
        {
            return getPool(InsPoolUtil.computePoolId(prefab));
        }
        public static InsPool<GameObject> getPool(int id)
        {
            if (_sharedPools.TryGetValue(id, out var pool)) return pool;
            return null;
        }
        public static bool exists(int id)
        {
            return _sharedPools.ContainsKey(id);
        }
        public static void clearSharedPool(int id)
        {
            if (_sharedPools.TryGetValue(id, out var pool))
            {
                pool.clearPool();
                _sharedPools.Remove(id);
            }
        }
    }
}