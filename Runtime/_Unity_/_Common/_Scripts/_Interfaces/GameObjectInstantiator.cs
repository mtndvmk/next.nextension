using System;
using UnityEngine;

namespace Nextension
{
    public interface IGameObjectInstantiator
    {
        GameObject getGameObject(Transform parent = null, bool worldPositionStays = true);
        GameObject getGameObject();
        void release(GameObject target);
    }
    
    public interface IComponentInstantiator<T> : IGameObjectInstantiator where T : UnityEngine.Object
    {
        T getComponent(Transform parent = null, bool worldPositionStays = true);
        T getComponent();
    }
    
    public readonly struct GameObjectInstantiator : IGameObjectInstantiator
    {
        internal readonly Func<Transform, bool, GameObject> instantiateFunc;
        internal readonly Action<GameObject> destroyFunc;

        public bool IsCreated => instantiateFunc != null && destroyFunc != null;

        public GameObjectInstantiator(Func<Transform, bool, GameObject> instantiateFunc, Action<GameObject> destroyFunc)
        {
            this.instantiateFunc = instantiateFunc;
            this.destroyFunc = destroyFunc;
        }
        
        public GameObjectInstantiator(Func<GameObject> instantiateFunc, Action<GameObject> destroyFunc)
        {
            this.instantiateFunc = (Transform parent, bool worldPositionStays) => instantiateFunc();
            this.destroyFunc = destroyFunc;
        }

        private void __throwNotCreatedException()
        {
#if UNITY_EDITOR
            if (!IsCreated)
            {
                throw new InvalidOperationException("ComponentInstantiate is not created properly");
            }
#endif
        }

        public GameObject getGameObject(Transform parent = null, bool worldPositionStays = true)
        {
            __throwNotCreatedException();
            return instantiateFunc(parent, worldPositionStays);
        }
        
        public GameObject getGameObject()
        {
            return getGameObject(null, true);
        }

        public void release(GameObject target)
        {
            __throwNotCreatedException();
            destroyFunc(target);
        }
    }
    
    public readonly struct ComponentInstantiator<T> : IComponentInstantiator<T> where T : UnityEngine.Object
    {
        internal readonly Func<Transform, bool, T> instantiateTFunc;
        internal readonly Func<Transform, bool, GameObject> instantiateGOFunc;
        internal readonly Action<GameObject> destroyFunc;

        public bool IsCreated => instantiateGOFunc != null && destroyFunc != null;

        public ComponentInstantiator(Func<Transform, bool, GameObject> instantiateFunc, Action<GameObject> destroyFunc)
        {
            this.instantiateTFunc = null;
            this.instantiateGOFunc = instantiateFunc;
            this.destroyFunc = destroyFunc;
        }
        
        public ComponentInstantiator(Func<GameObject> instantiateFunc, Action<GameObject> destroyFunc)
        {
            this.instantiateTFunc = null;
            this.instantiateGOFunc = (parent, worldPositionStays) => instantiateFunc();
            this.destroyFunc = destroyFunc;
        }

        public ComponentInstantiator(Func<T> instantiateFunc, Action<GameObject> destroyFunc)
        {
            if (UnityGeneric<T>.IsComponent)
            {
                this.instantiateTFunc = (parent, worldPositionStays) =>
                {
                    return instantiateFunc();
                };
                this.instantiateGOFunc = null;
            }
            else if (UnityGeneric<T>.IsGameObject)
            {
                this.instantiateTFunc = null;
                this.instantiateGOFunc = (parent, worldPositionStays) =>
                {
                    return instantiateFunc() as GameObject;
                };
            }
            else
            {
                throw new ArgumentException($"Type {typeof(T)} is not Component or GameObject");
            }
            this.destroyFunc = destroyFunc;
        }
        
        public ComponentInstantiator(Func<Transform, bool, T> instantiateFunc, Action<GameObject> destroyFunc)
        {
            if (UnityGeneric<T>.IsComponent)
            {
                this.instantiateTFunc = (parent, worldPositionStays) =>
                {
                    return instantiateFunc(parent, worldPositionStays);
                };
                this.instantiateGOFunc = null;
            }
            else if (UnityGeneric<T>.IsGameObject)
            {
                this.instantiateTFunc = null;
                this.instantiateGOFunc = (parent, worldPositionStays) =>
                {
                    return instantiateFunc(parent, worldPositionStays) as GameObject;
                };
            }
            else
            {
                throw new ArgumentException($"Type {typeof(T)} is not Component or GameObject");
            }
            this.destroyFunc = destroyFunc;
        }

        public ComponentInstantiator(GameObjectInstantiator gameObjectInstantiate)
        {
            this.instantiateTFunc = null;
            this.instantiateGOFunc = gameObjectInstantiate.instantiateFunc;
            this.destroyFunc = gameObjectInstantiate.destroyFunc;
        }
        
        public ComponentInstantiator(IGameObjectInstantiator gameObjectInstantiate)
        {
            this.instantiateTFunc = null;
            this.instantiateGOFunc = gameObjectInstantiate.getGameObject;
            this.destroyFunc = gameObjectInstantiate.release;
        }
        
        private void __throwNotCreatedException()
        {
#if UNITY_EDITOR
            if (!IsCreated)
            {
                throw new InvalidOperationException("ComponentInstantiate is not created properly");
            }
#endif
        }

        public GameObject getGameObject(Transform parent, bool worldPositionStays)
        {
            __throwNotCreatedException();
            if (instantiateTFunc != null)
            {
                return instantiateTFunc(parent, worldPositionStays).getGameObject();
            }
            return instantiateGOFunc(parent, worldPositionStays);
        }
        
        public GameObject getGameObject()
        {
            return getGameObject(null, true);
        }

        public T getComponent(Transform parent = null, bool worldPositionStays = true)
        {
            __throwNotCreatedException();
            if (instantiateTFunc != null)
            {
                return instantiateTFunc(parent, worldPositionStays);
            }
            var go = instantiateGOFunc(parent, worldPositionStays);
            if (go is T com)
            {
                return com;
            }
            if (go.TryGetComponent(out com))
            {
                return com;
            }
            NDebug.LogError($"Cannot get component of type {typeof(T)}", go);
            return null;
        }
        
        public T getComponent()
        {
            return getComponent(null, true);
        }

        public void release(GameObject target)
        {
            __throwNotCreatedException();
            destroyFunc(target);
        }
    }
}
