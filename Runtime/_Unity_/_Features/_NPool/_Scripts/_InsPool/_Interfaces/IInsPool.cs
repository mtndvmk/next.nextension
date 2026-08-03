using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Nextension
{
    public interface IInsPool : IGameObjectInstantiator
    {
        public ulong Id { get; }
        void clearPool();
        void release(OriginInstance origin);
        IGameObjectInstantiator getGameObjectInstantiator();
    }
    public interface IInsPool<T> : IInsPool, IComponentInstantiator<T> where T : Object
    {
        T get(Transform parent = null, bool worldPositionStays = true);
        T getAndRelease(IWaitable releaseWaitable, Transform parent = null, bool worldPositionStays = true);
        T getAndDelayRelease(float delaySeconds, Transform parent = null, bool worldPositionStays = true);
        IEnumerable<T> getInstances(int count, Transform parent, bool worldPositionStays = true);
        void release(T instance);
        bool poolContains(T instance);
        IComponentInstantiator<T> getComponentInstantiator();
    }
    
    [System.Serializable]
    public abstract class AbsInsPool<T> : IInsPool<T> where T : Object
    {
        public abstract ulong Id { get; }

        public abstract void clearPool();

        public abstract T get(Transform parent = null, bool worldPositionStays = true);

        public abstract bool poolContains(T instance);

        public abstract void release(T instance);

        public GameObject getGameObject(Transform parent = null, bool worldPositionStays = true)
        {
            return InsPoolUtil.getGameObject(get(parent, worldPositionStays));
        }
        public GameObject getGameObject()
        {
            return getGameObject(null, true);
        }

        public T getComponent(Transform parent = null, bool worldPositionStays = true)
        {
            return get(parent, worldPositionStays);
        }

        public T getComponent()
        {
            return get();
        }

        public IGameObjectInstantiator getGameObjectInstantiator()
        {
            return this;
        }

        public IComponentInstantiator<T> getComponentInstantiator()
        {
            return this;
        }

        public IComponentInstantiator<T2> getComponentInstantiator<T2>() where T2 : Object
        {
            return new ComponentInstantiator<T2>(this);
        }

        public IEnumerable<T> getInstances(int count, Transform parent, bool worldPositionStays = true)
        {
            for (int i = 0; i < count; ++i)
            {
                yield return get(parent, worldPositionStays);
            }
        }

        public void release(GameObject target)
        {
            InsPool.release(target);
        }
        public void release(OriginInstance origin)
        {
            release(InsPoolUtil.getInstanceFromGO<T>(origin.gameObject));
        }
        public void waitAndRelease<TWaitable>(TWaitable waitable, T instance) where TWaitable : IWaitable
        {
            __waitAndRelease(waitable, instance).forget();
        }
        public void waitAndRelease<TWaitable>(TWaitable waitable, GameObject gameObject) where TWaitable : IWaitable
        {
            __waitAndRelease(waitable, gameObject).forget();
        }

        private async NTaskVoid __waitAndRelease<TWaitable>(TWaitable waitable, T instance) where TWaitable : IWaitable
        {
            await waitable;
            release(instance);
        }
        private async NTaskVoid __waitAndRelease<TWaitable>(TWaitable waitable, GameObject gameObject) where TWaitable : IWaitable
        {
            await waitable;
            release(gameObject);
        }

        public T getAndRelease(IWaitable waitable, Transform parent = null, bool worldPositionStays = true)
        {
            T item = get(parent, worldPositionStays);
            waitAndRelease(waitable, item);
            return item;
        }
        public T getAndDelayRelease(float delaySeconds, Transform parent = null, bool worldPositionStays = true)
        {
            T item = get(parent, worldPositionStays);
            waitAndRelease(new NWaitSecond(delaySeconds), item);
            return item;
        }
    }
}