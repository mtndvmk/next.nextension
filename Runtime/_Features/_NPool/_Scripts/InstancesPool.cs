using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Nextension
{
    public static class InstancesPool
    {
        /// <summary>
        /// Release the game object to the pool if gameObject was spawned by the shared pool, otherwise destroy it
        /// </summary>
        public static void releaseOrDestroy(GameObject gameObject)
        {
            var origin = gameObject.GetComponent<OriginInstance>();
            if (origin == null)
            {
                Debug.LogWarning("Destroyed " + gameObject.name);
                NUtils.destroy(gameObject);
            }
            else
            {
                if (origin.IsUniquePool)
                {
                    var pool = SharedInstancesPool.getPool(origin.Id);
                    if (pool != null)
                    {
                        pool.release(gameObject);
                    }
                    else
                    {
                        NUtils.destroy(gameObject);
                    }
                }
                else
                {
                    Debug.LogWarning("Not support unique pool, destroy gameObject");
                    NUtils.destroy(gameObject);
                }
            }
        }
        public static void releaseOrDestroy(Component component)
        {
            releaseOrDestroy(component.gameObject);
        }
    }

    [Serializable]
    public class InstancesPool<T> where T : Object
    {
        private static Exception NOT_SUPPORT_EXCEPTION(Type type) => new Exception($"Not support InstancesPool for {type}");

        [SerializeField] private T prefab;
        [SerializeField] internal bool isUniquePool;

        private uint _maxCapacity = 0;
        private OriginInstance _origin;
        [NonSerialized] private InstancesPool<GameObject> _sharedPool;
        [NonSerialized] private NBListUseHashCode<T> _instancePool;
        private NBListUseHashCode<T> InstancePool
        {
            get
            {
                if (_instancePool == null)
                {
                    createInstancePool();
                }
                return _instancePool;
            }
        }

        private readonly static bool IS_GENERIC_OF_GAMEOBJECT = typeof(T).Equals(typeof(GameObject));
        private const uint MAX_CAPACITY_DEFAULT = 100;

#if UNITY_EDITOR
        private T _copiedPrefabOnEditor;
#endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T getPrefab()
        {
#if UNITY_EDITOR
            if (!_copiedPrefabOnEditor)
            {
                _copiedPrefabOnEditor = Object.Instantiate(prefab, InstancesPoolContainer.Container, true);
                _copiedPrefabOnEditor.name = prefab.name;
            }
            return _copiedPrefabOnEditor;
#else
            return prefab;
#endif
        }
        private void createInstancePool()
        {
            _instancePool = new NBListUseHashCode<T>();
        }
        private InstancesPool<GameObject> SharedPool
        {
            get
            {
                if (_sharedPool == null)
                {
                    _sharedPool = SharedInstancesPool.getOrCreatePool(getGameObject(prefab));
                }
                return _sharedPool;
            }
        }
        private T createNewInstance()
        {
            if (_origin == null)
            {
                var go = getGameObject(getPrefab());
                _origin = go.getOrAddComponent<OriginInstance>();
                _origin.setPoolId(prefab.GetInstanceID(), isUniquePool);
                if (_maxCapacity == 0)
                {
                    _maxCapacity = MAX_CAPACITY_DEFAULT;
                }
            }
            var ins = Object.Instantiate(getPrefab(), InstancesPoolContainer.Container, true);
            return ins;
        }
        private static GameObject getGameObject(T prefab)
        {
            if (prefab == null) throw new ArgumentNullException("InstancesPool.prefab");
            if (prefab is GameObject)
            {
                return prefab as GameObject;
            }
            else if (prefab is Component)
            {
                return (prefab as Component).gameObject;
            }
            else
            {
                throw NOT_SUPPORT_EXCEPTION(typeof(T));
            }
        } 
        private static T getInstanceFromGO(GameObject go)
        {
            if (IS_GENERIC_OF_GAMEOBJECT)
            {
                return go as T;
            }
            else
            {
                return go.GetComponent<T>();
            }
        }

        public InstancesPool(T prefab)
        {
            InternalCheck.checkEditorMode();
            this.prefab = prefab;
        }
        public T get(Transform parent = null, bool worldPositionStays = true, bool invokeIPoolableEvent = false)
        {
            InternalCheck.checkEditorMode();
            if (isUniquePool)
            {
                T ins;
                if (InstancePool.Count > 0)
                {
                    ins = InstancePool.takeAndRemoveAt(0);
                }
                else
                {
                    ins = createNewInstance();
                }

                var go = getGameObject(ins);
                go.transform.setParent(parent, worldPositionStays);
                if (invokeIPoolableEvent)
                {
                    foreach (var poolable in go.GetComponentsInChildren<IPoolable>(true))
                    {
                        poolable.onSpawned();
                    }
                }
                return ins;
            }
            return getInstanceFromGO(SharedPool.get(parent, worldPositionStays));
        }
        public IEnumerable<T> getInstances(int count, Transform parent, bool worldPositionStays = true, bool invokeIPoolableEvent = false) 
        {
            for (int i = 0; i < count; ++i)
            {
                yield return get(parent, worldPositionStays, invokeIPoolableEvent);
            }
        }
        public void release(T instance, bool invokeIPoolableEvent = false)
        {
            InternalCheck.checkEditorMode();
            if (instance == null)
            {
                return;
            }
            if (isUniquePool)
            {
                if (_origin == null)
                {
                    Debug.LogWarning($"Origin of pool is null?", instance);
                    return;
                }
                if (InstancePool.bContains(instance))
                {
                    Debug.LogWarning($"Instance has been in pool", instance);
                    return;
                }
                var go = getGameObject(instance);
                var origin = go.GetComponent<OriginInstance>();
                if (origin == null)
                {
                    Debug.LogWarning($"Missing OriginInstance on object: {instance.name}", instance);
                    return;
                }

                if (origin.Id != _origin.Id)
                {
                    Debug.LogWarning($"OriginId [{origin.Id}] not match, try release by SharedInstancesPool", instance);
                    SharedInstancesPool.getPool(origin.Id)?.release(go);
                    return;
                }

                if (invokeIPoolableEvent)
                {
                    foreach (var poolable in go.GetComponentsInChildren<IPoolable>(true))
                    {
                        poolable.onDespawned();
                    }
                }

                if (InstancePool.Count >= _maxCapacity)
                {
                    NUtils.destroy(go);
                    return;
                }

                go.transform.setParent(InstancesPoolContainer.Container);
                InstancePool.addAndSort(instance);
            }
            else
            {
                SharedPool.release(getGameObject(instance));
            }
        }
        public void clearPool(bool isClearSharedPool = false)
        {
            InternalCheck.checkEditorMode();
            if (!isUniquePool)
            {
                if (isClearSharedPool)
                {
                    SharedPool.clearPool();
                }
                return;
            }
            if (InstancePool.Count > 0)
            {
                foreach (var item in InstancePool.asEnumerable())
                {
                    NUtils.destroyObject(item);
                }
                InstancePool.clear();
            }
        }
    }
    internal static class SharedInstancesPool
    {
        [StartupMethod]
        static void reset()
        {
            _sharedPools = new Dictionary<int, InstancesPool<GameObject>>();
        }
        private static Dictionary<int, InstancesPool<GameObject>> _sharedPools;
        public static InstancesPool<GameObject> getOrCreatePool(GameObject prefab)
        {
            var insId = prefab.GetInstanceID();
            InstancesPool<GameObject> pool;
            if (!_sharedPools.ContainsKey(insId))
            {
                pool = new InstancesPool<GameObject>(prefab);
                pool.isUniquePool = true;
                _sharedPools.Add(insId, pool);
            }
            else
            {
                pool = _sharedPools[insId];
            }
            return pool;
        }
        public static InstancesPool<GameObject> getPool(int id)
        {
            if (_sharedPools.TryGetValue(id, out var pool)) return pool;
            return null;
        }
    }
    internal class InstancesPoolContainer : MonoBehaviour
    {
        private static Transform _container;
        public static Transform Container
        {
            get
            {
                if (!_container)
                {
                    _container = new GameObject("InstancesPool.Container").transform;
                    _container.gameObject.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
                    _container.setActive(false);
                    DontDestroyOnLoad(_container.gameObject);
                }
                return _container;
            }
        }
    }
}
