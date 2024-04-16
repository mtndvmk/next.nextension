using System.Collections.Generic;
using UnityEngine;

namespace Nextension
{
    internal static class SharedInstancesPool
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
        private static Dictionary<int, InstancesPool<GameObject>> _sharedPools = new();
        public static InstancesPool<GameObject> getOrCreatePool<T>(T component, int startupInstanceCount) where T : Object
        {
            return getOrCreatePool(InstancesPoolUtil.getGameObject(component), startupInstanceCount);
        }
        public static InstancesPool<GameObject> getOrCreatePool(GameObject prefab, int startupInstanceCount)
        {
            var insId = prefab.GetInstanceID();
            InstancesPool<GameObject> pool;
            if (!_sharedPools.ContainsKey(insId))
            {
                pool = new InstancesPool<GameObject>(prefab, startupInstanceCount, true);
                _sharedPools.Add(insId, pool);
            }
            else
            {
                pool = _sharedPools[insId];
            }
            pool.updateStartupInstances(startupInstanceCount);
            return pool;
        }
        public static InstancesPool<GameObject> getPool(int id)
        {
            if (_sharedPools.TryGetValue(id, out var pool)) return pool;
            return null;
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