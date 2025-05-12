using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Nextension
{
    public static class InstancesPool
    {
        public static bool checkInPool(GameObject gameObject)
        {
            if (!gameObject.TryGetComponent<OriginInstance>(out var origin))
            {
                Debug.LogWarning($"{gameObject.name} is not from pool");
                return false;
            }
            else
            {
                return origin.IsInPool;
            }
        }
        public static void release(GameObject gameObject)
        {
            if (!gameObject.TryGetComponent<OriginInstance>(out var origin))
            {
                Debug.LogError($"{gameObject.name} is not from pool");
            }
            else
            {
                origin.release();
            }
        }
        public static void release(Component component)
        {
            release(component.gameObject);
        }
        public static void releaseOrDestroy(GameObject gameObject)
        {
            if (!gameObject.TryGetComponent<OriginInstance>(out var origin))
            {
                NUtils.destroy(gameObject);
            }
            else
            {
                origin.release();
            }
        }
        public static void releaseOrDestroy(Component component)
        {
            releaseOrDestroy(component.gameObject);
        }
        public static void release(this OriginInstance origin)
        {
            if (!origin.IsInPool)
            {
                origin.Pool.release(origin);
            }
        }

        public static SharedPoolWrapper<T> getSharedPool<T>(this T prefab) where T : Object
        {
            var go = InstancesPoolUtil.getGameObject(prefab);
            if (go.TryGetComponent<OriginInstance>(out var originInstance))
            {
                return new SharedPoolWrapper<T>(originInstance.Id);
            }
            else
            {
                var poolId = InstancesPoolUtil.computePoolId(prefab);
                SharedInstancesPool.getOrCreatePool(poolId, go, 0);
                return new SharedPoolWrapper<T>(poolId);
            }
        }
        public static SharedPoolWrapper<T> getSharedPoolWithoutChecks<T>(this T prefab) where T : Object
        {
            var go = InstancesPoolUtil.getGameObject(prefab);
            var poolId = InstancesPoolUtil.computePoolId(prefab);
            SharedInstancesPool.getOrCreatePool(poolId, go, 0);
            return new SharedPoolWrapper<T>(poolId);
        }
    }

    public interface IInstancePool
    {
        public int Id { get; }
        void release(OriginInstance origin);
    }

    public interface IInstancePool<T> where T : Object
    {
        T get(Transform parent = null, bool worldPositionStays = true);
        T getAndRelease(IWaitable releaseWaitable, Transform parent = null, bool worldPositionStays = true);
        T getAndDelayRelease(float delaySeconds, Transform parent = null, bool worldPositionStays = true);
        IEnumerable<T> getInstances(int count, Transform parent, bool worldPositionStays = true);
        void release(T instance);
        void clearPool();
        bool poolContains(T instance);
    }

    [Serializable]
    public class InstancesPool<T> : ISerializationCallbackReceiver, IInstancePool, IInstancePool<T>
        where T : Object
    {
        [SerializeField] private T _prefab;
        [SerializeField] private bool _disableSharedPool;
        [SerializeField] private int _startupInstanceCount = 0;
        [SerializeField] private bool _useOriginPrefab;

        private int _currentStartupInstanceCount;
        private uint _clonedCount;

        [NonSerialized] private InstancesPool<GameObject> _sharedPool;
        [NonSerialized] private List<T> _instancePool;

        private void requireCall()
        {
            EditorCheck.checkEditorMode();
            if (_instancePool == null)
            {
                _instancePool = new List<T>(_startupInstanceCount);
                Id = InstancesPoolUtil.computePoolId(_prefab);
                if (_useOriginPrefab)
                {
                    getGameObject(_prefab).getOrAddComponent<OriginInstance>().setPool(this);
                }
                else
                {
                    _copiedPrefab = Object.Instantiate(_prefab, InstancesPoolContainer.CopiedPrefabContainer, true);
                    _copiedPrefab.name = _prefab.name;
                    getGameObject(_copiedPrefab).getOrAddComponent<OriginInstance>().setPool(this);
                }
                if (MaxPoolInstanceCount == 0)
                {
                    MaxPoolInstanceCount = IPoolable.DEFAULT_MAX_POOL_ITEM_COUNT;
                }
            }
        }

        public uint MaxPoolInstanceCount { get; set; }
        public int Id { get; private set; }

        private T _copiedPrefab;
        public T Prefab => getPrefab();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T getPrefab()
        {
            EditorCheck.checkEditorMode();
#if UNITY_EDITOR
            if (_prefab == null)
            {
                throw new NullReferenceException("Prefab is null");
            }
#endif
            if (_useOriginPrefab)
            {
                return _prefab;
            }
            return _copiedPrefab;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void updateStartupInstances()
        {
            if (_disableSharedPool)
            {
                updateStartupInstances(null);
            }
            else
            {
                SharedPool.updateStartupInstances(null);
            }
        }
        internal void updateStartupInstances(int? startupCount)
        {
            EditorCheck.checkEditorMode();
            if (startupCount.HasValue && _startupInstanceCount < startupCount.Value)
            {
                _startupInstanceCount = startupCount.Value;
            }
            requireCall();
            if (_currentStartupInstanceCount < _startupInstanceCount && _startupInstanceCount > 0)
            {
                _currentStartupInstanceCount = _startupInstanceCount;
                createStartupInstances();
            }
        }
        private InstancesPool<GameObject> SharedPool => _disableSharedPool ? 
            throw new Exception($"Not support when {nameof(_disableSharedPool)} is true") : 
            _sharedPool ??= SharedInstancesPool.getOrCreatePool(getGameObject(_prefab), _startupInstanceCount);
        private T createNewInstance()
        {
            var ins = Object.Instantiate(getPrefab(), InstancesPoolContainer.Container, true);
            ins.name = $"{_prefab.name} [PoolClone_{_clonedCount++}]";
            ins.getOrAddComponent<OriginInstance>().setPool(this);
            return ins;
        }
        private void createStartupInstances()
        {
            while (_instancePool.Count < _startupInstanceCount)
            {
                _instancePool.Add(createNewInstance());
            }
        }
        private static GameObject getGameObject(T instance)
        {
            return InstancesPoolUtil.getGameObject(instance);
        }
        public InstancesPool(T prefab, int startupInstanceCount = 0, bool disableSharedPool = false)
        {
            EditorCheck.checkEditorMode();
            _prefab = prefab;
            _startupInstanceCount = startupInstanceCount;
            _disableSharedPool = disableSharedPool;

            updateStartupInstances();
        }
        public T get(Transform parent = null, bool worldPositionStays = true)
        {
            requireCall();
            if (_disableSharedPool)
            {
                T ins;
                if (_instancePool.Count > 0)
                {
                    ins = _instancePool.takeAndRemoveLast();
                }
                else
                {
                    ins = createNewInstance();
                }

                var go = getGameObject(ins);
                go.transform.setParent(parent, worldPositionStays);

                using var poolableArray = go.getComponentsInChildren_CachedList<IPoolable>();
                foreach (var poolable in poolableArray)
                {
                    poolable.onSpawn();
                }
                return ins;
            }
            return InstancesPoolUtil.getInstanceFromGO<T>(SharedPool.get(parent, worldPositionStays));
        }
        public T getAndRelease(IWaitable releaseWaitable, Transform parent = null, bool worldPositionStays = true)
        {
            T item = get(parent, worldPositionStays);
            releaseWaitable.startWaitable().addCompletedEvent(() =>
            {
                release(item);
            });
            return item;
        }
        public T getAndDelayRelease(float delaySeconds, Transform parent = null, bool worldPositionStays = true)
        {
            T item = get(parent, worldPositionStays);
            new NWaitSecond(delaySeconds).startWaitable().addCompletedEvent(() =>
            {
                release(item);
            });
            return item;
        }
        public IEnumerable<T> getInstances(int count, Transform parent, bool worldPositionStays = true)
        {
            for (int i = 0; i < count; ++i)
            {
                yield return get(parent, worldPositionStays);
            }
        }
        public void clearPool()
        {
            clearPool(true);
        }
        public void clearPool(bool isClearSharedPool)
        {
            EditorCheck.checkEditorMode();
            if (_disableSharedPool)
            {
                if (_instancePool != null && _instancePool.Count > 0)
                {
                    foreach (var item in _instancePool)
                    {
                        var go = getGameObject(item);
                        using var poolableArray = go.getComponentsInChildren_CachedList<IPoolable>();
                        foreach (var poolable in poolableArray)
                        {
                            poolable.onDestroy();
                        }
                        NUtils.destroy(go);
                    }
                    _instancePool.Clear();
                }
            }
            else
            {
                if (isClearSharedPool)
                {
                    if (_sharedPool != null)
                    {
                        SharedInstancesPool.clearSharedPool(_sharedPool.Id);
                        _sharedPool = null;
                    }
                }
            }
        }
        public bool poolContains(T instance)
        {
            if (_disableSharedPool)
            {
                return _instancePool.Contains(instance);
            }
            else
            {
                return SharedPool.poolContains(getGameObject(instance));
            }
        }

        public void OnBeforeSerialize()
        {

        }
        public void OnAfterDeserialize()
        {
#if UNITY_EDITOR
            new NWaitFrame_Editor(1).startWaitable().addCompletedEvent(() =>
            {
                if (NStartRunner.IsPlaying && _prefab && getGameObject(_prefab))
                {
                    updateStartupInstances();
                }
            });
#else
            new NWaitFrame(1).startWaitable().addCompletedEvent(() =>
            {
                if (_prefab && getGameObject(_prefab))
                {
                    updateStartupInstances();
                }
            });
#endif
        }

        public void release(OriginInstance origin)
        {
            release(InstancesPoolUtil.getInstanceFromGO<T>(origin.gameObject));
        }

        public void release(T instance)
        {
            using var otherOrigins = getGameObject(instance).getComponentsInChildren_CachedList<OriginInstance>(true);
            int otherCount = otherOrigins.Count;
            if (otherCount > 1)
            {
                for (int i = 0; i < otherCount; ++i)
                {
                    var tempGO = otherOrigins[i].gameObject;
                    tempGO.transform.setParent(InstancesPoolContainer.Container);
                }
                for (int i = 0; i < otherCount; ++i)
                {
                    otherOrigins[i].release();
                }
                return;
            }

            var origin = otherOrigins[0];
            var go = origin.gameObject;

            if (_disableSharedPool)
            {
                if (origin.IsInPool)
                {
                    Debug.LogWarning($"Instance has been in pool", instance);
                    return;
                }
                if (origin.Id != Id)
                {
                    Debug.LogWarning($"OriginId [{origin.Id}], [{Id}] not match, try release by SharedInstancesPool", go);
                    SharedInstancesPool.getPool(origin.Id)?.release(go);
                    return;
                }

                using var poolableArray = go.getComponentsInChildren_CachedList<IPoolable>();
                if (_instancePool.Count >= MaxPoolInstanceCount)
                {
                    foreach (var poolable in poolableArray)
                    {
                        poolable.onDespawn();
                        poolable.onDestroy();
                    }
                    NUtils.destroy(go);
                    return;
                }
                else
                {
                    _instancePool.Add(instance);
                }

                go.transform.setParent(InstancesPoolContainer.Container);
                foreach (var poolable in poolableArray)
                {
                    poolable.onDespawn();
                }
            }
            else
            {
                SharedPool.release(getGameObject(instance));
            }
        }
    }

    public readonly struct SharedPoolWrapper<T> : IInstancePool<T> where T : Object
    {
        private readonly int _poolId;
        public readonly int Id => _poolId;

        public SharedPoolWrapper(int poolId)
        {
            this._poolId = poolId;
        }
        public T get(Transform parent = null, bool worldPositionStays = true)
        {
            return InstancesPoolUtil.getInstanceFromGO<T>(SharedInstancesPool.getPool(_poolId).get(parent, worldPositionStays));
        }
        public T getAndRelease(IWaitable releaseWaitable, Transform parent = null, bool worldPositionStays = true)
        {
            var pool = SharedInstancesPool.getPool(_poolId);
            var go = pool.get(parent, worldPositionStays);
            releaseWaitable.startWaitable().addCompletedEvent(() =>
            {
                pool.release(go);
            });
            return InstancesPoolUtil.getInstanceFromGO<T>(go);
        }
        public T getAndDelayRelease(float delaySeconds, Transform parent = null, bool worldPositionStays = true)
        {
            var pool = SharedInstancesPool.getPool(_poolId);
            var go = pool.get(parent, worldPositionStays);
            new NWaitSecond(delaySeconds).startWaitable().addCompletedEvent(() =>
            {
                pool.release(go);
            });
            return InstancesPoolUtil.getInstanceFromGO<T>(go);
        }
        public IEnumerable<T> getInstances(int count, Transform parent, bool worldPositionStays = true)
        {
            for (int i = 0; i < count; ++i)
            {
                yield return get(parent, worldPositionStays);
            }
        }
        public void release(T instance)
        {
            SharedInstancesPool.getPool(_poolId).release(InstancesPoolUtil.getGameObject(instance));
        }
        public void clearPool()
        {
            SharedInstancesPool.clearSharedPool(_poolId);
        }
        public bool poolContains(T instance)
        {
            return SharedInstancesPool.getPool(_poolId).poolContains(InstancesPoolUtil.getGameObject(instance));
        }
    }
}