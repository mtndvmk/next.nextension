using UnityEngine;
using Object = UnityEngine.Object;

namespace Nextension
{
    public ref struct SharedInsPool<T> where T : Object
    {
        private readonly int _poolId;
        public readonly int Id => _poolId;
        public readonly uint MaxPoolInstanceCount
        {
            get => SharedInsPoolUtil.getPool(_poolId).MaxPoolInstanceCount;
            set => SharedInsPoolUtil.getPool(_poolId).MaxPoolInstanceCount = value;
        }
        public SharedInsPool(int poolId)
        {
            this._poolId = poolId;
        }

        public readonly InsPool<GameObject> Pool => SharedInsPoolUtil.getPool(_poolId);

        public readonly T get(Transform parent = null, bool worldPositionStays = true)
        {
            return InsPoolUtil.getInstanceFromGO<T>(SharedInsPoolUtil.getPool(_poolId).get(parent, worldPositionStays));
        }
        public readonly T getAndRelease(IWaitable waitable, Transform parent = null, bool worldPositionStays = true)
        {
            var pool = SharedInsPoolUtil.getPool(_poolId);
            var go = pool.get(parent, worldPositionStays);
            pool.waitAndRelease(waitable, go);
            return InsPoolUtil.getInstanceFromGO<T>(go);
        }
        public readonly T getAndDelayRelease(float delaySeconds, Transform parent = null, bool worldPositionStays = true)
        {
            var pool = SharedInsPoolUtil.getPool(_poolId);
            var go = pool.get(parent, worldPositionStays);
            pool.waitAndRelease(new NWaitSecond(delaySeconds), go);
            return InsPoolUtil.getInstanceFromGO<T>(go);
        }
        public readonly void release(T instance)
        {
            SharedInsPoolUtil.getPool(_poolId).release(InsPoolUtil.getGameObject(instance));
        }
        public readonly void clearPool()
        {
            SharedInsPoolUtil.clearSharedPool(_poolId);
        }
        public readonly bool poolContains(T instance)
        {
            return SharedInsPoolUtil.getPool(_poolId).poolContains(InsPoolUtil.getGameObject(instance));
        }
        public readonly void initializeInstancesInPool(int count)
        {
            SharedInsPoolUtil.getPool(_poolId).initializeInstancesInPool(count);
        }

        public readonly GameObjectInstantiate getGameObjectInstantiate()
        {
            return Pool.getGameObjectInstantiate();
        }

        public readonly ComponentInstantiate<T> getComponentInstantiate()
        {
            return new ComponentInstantiate<T>(getGameObjectInstantiate());
        }
    }
}