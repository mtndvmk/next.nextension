using System;
using UnityEngine;

namespace Nextension
{
    public interface IGameObjectInstantiate
    {
        GameObject getGameObject(Transform parent = null, bool worldPositionStays = true);
        GameObject getGameObject();
        void release(GameObject target);
    }
    public interface IComponentInstantiate<T> : IGameObjectInstantiate where T : UnityEngine.Object
    {
        T getComponent(Transform parent = null, bool worldPositionStays = true);
        T getComponent();
    }
    public readonly struct GameObjectInstantiate : IGameObjectInstantiate
    {
        internal readonly Func<Transform, bool, GameObject> instantiateFunc;
        internal readonly Action<GameObject> destroyFunc;

        public bool IsCreated => instantiateFunc != null && destroyFunc != null;

        public GameObjectInstantiate(Func<Transform, bool, GameObject> instantiateFunc, Action<GameObject> destroyFunc)
        {
            this.instantiateFunc = instantiateFunc;
            this.destroyFunc = destroyFunc;
        }
        public GameObjectInstantiate(Func<GameObject> instantiateFunc, Action<GameObject> destroyFunc)
        {
            this.instantiateFunc = (_, _) => instantiateFunc();
            this.destroyFunc = destroyFunc;
        }
        public GameObjectInstantiate(IGameObjectInstantiate gameObjectInstantiate)
        {
            this.instantiateFunc = gameObjectInstantiate.getGameObject;
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
    public readonly struct ComponentInstantiate<T> : IComponentInstantiate<T> where T : UnityEngine.Object
    {
        internal readonly Func<Transform, bool, GameObject> instantiateFunc;
        internal readonly Action<GameObject> destroyFunc;

        public bool IsCreated => instantiateFunc != null && destroyFunc != null;

        public ComponentInstantiate(Func<Transform, bool, GameObject> instantiateFunc, Action<GameObject> destroyFunc)
        {
            this.instantiateFunc = instantiateFunc;
            this.destroyFunc = destroyFunc;
        }
        public ComponentInstantiate(Func<GameObject> instantiateFunc, Action<GameObject> destroyFunc)
        {
            this.instantiateFunc = (_, _) => instantiateFunc();
            this.destroyFunc = destroyFunc;
        }

        public ComponentInstantiate(Func<T> instantiateFunc, Action<GameObject> destroyFunc)
        {
            if (instantiateFunc != null)
            {
                if (UnityGeneric<T>.IsComponent)
                {
                    this.instantiateFunc = (_, _) =>
                    {
                        return (instantiateFunc() as Component).gameObject;
                    };
                }
                else if (UnityGeneric<T>.IsGameObject)
                {
                    this.instantiateFunc = (_, _) =>
                    {
                        return instantiateFunc() as GameObject;
                    };
                }
                else
                {
                    throw new ArgumentException($"Type {typeof(T)} is not Component or GameObject");
                }
            }
            else
            {
                this.instantiateFunc = null;
            }
            this.destroyFunc = destroyFunc;
        }
        public ComponentInstantiate(Func<Transform, bool, T> instantiateFunc, Action<GameObject> destroyFunc)
        {
            if (instantiateFunc != null)
            {
                if (UnityGeneric<T>.IsComponent)
                {
                    this.instantiateFunc = (parent, worldPositionStays) =>
                    {
                        return (instantiateFunc(parent, worldPositionStays) as Component).gameObject;
                    };
                }
                else if (UnityGeneric<T>.IsGameObject)
                {
                    this.instantiateFunc = (parent, worldPositionStays) =>
                    {
                        return instantiateFunc(parent, worldPositionStays) as GameObject;
                    };
                }
                else
                {
                    throw new ArgumentException($"Type {typeof(T)} is not Component or GameObject");
                }
            }
            else
            {
                this.instantiateFunc = null;
            }
            this.destroyFunc = destroyFunc;
        }

        public ComponentInstantiate(GameObjectInstantiate gameObjectInstantiate)
        {
            this.instantiateFunc = gameObjectInstantiate.instantiateFunc;
            this.destroyFunc = gameObjectInstantiate.destroyFunc;
        }
        public ComponentInstantiate(IGameObjectInstantiate gameObjectInstantiate)
        {
            this.instantiateFunc = gameObjectInstantiate.getGameObject;
            this.destroyFunc = gameObjectInstantiate.release;
        }
        public ComponentInstantiate(IComponentInstantiate<T> componentInstantiate)
        {
            this.instantiateFunc = componentInstantiate.getGameObject;
            this.destroyFunc = componentInstantiate.release;
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
            return instantiateFunc(parent, worldPositionStays);
        }
        public GameObject getGameObject()
        {
            return getGameObject(null, true);
        }

        public T getComponent(Transform parent = null, bool worldPositionStays = true)
        {
            var gameObject = getGameObject(parent, worldPositionStays);
            if (gameObject is T goAsT)
            {
                return goAsT;
            }
            if (gameObject.TryGetComponent(out T com))
            {
                return com;
            }
            Debug.LogError($"Cannot get component of type {typeof(T)}", gameObject);
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