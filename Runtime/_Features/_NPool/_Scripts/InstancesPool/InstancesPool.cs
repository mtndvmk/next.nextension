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
            var origin = gameObject.GetComponent<OriginInstance>();
            if (origin == null)
            {
                Debug.LogWarning($"{gameObject.name} is not from pool");
                return false;
            }
            else
            {
                if (origin.IsSharedPool)
                {
                    var pool = SharedInstancesPool.getPool(origin.Id);
                    if (pool != null)
                    {
                        return pool.poolContains(gameObject);
                    }
                    else
                    {
                        Debug.LogError($"Can't find pool - {origin.Id}");
                    }
                }
                else
                {
                    Debug.LogError("Only support unique pool");
                }
            }
            return false;
        }
        public static void release(GameObject gameObject)
        {
            var origin = gameObject.GetComponent<OriginInstance>();
            if (origin == null)
            {
                Debug.LogError($"{gameObject.name} is not from pool");
            }
            else
            {
                if (origin.IsSharedPool)
                {
                    var pool = SharedInstancesPool.getPool(origin.Id);
                    if (pool != null)
                    {
                        pool.release(gameObject, gameObject, origin);
                    }
                    else
                    {
                        Debug.LogError($"Can't find pool - {origin.Id}");
                    }
                }
                else
                {
                    Debug.LogError("Only support unique pool");
                }
            }
        }
        public static void release(Component component)
        {
            release(component.gameObject);
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

    public interface IInstancePool<T> where T : Object
    {
        T get(Transform parent = null, bool worldPositionStays = true);
        T getAndRelease(IWaitable releaseWaitable, Transform parent = null, bool worldPositionStays = true)
        {
            T item = get(parent, worldPositionStays);
            releaseWaitable.startWaitable().addCompletedEvent(() =>
            {
                release(item);
            });
            return item;
        }
        T getAndDelayRelease(float delaySeconds, Transform parent = null, bool worldPositionStays = true)
        {
            T item = get(parent, worldPositionStays);
            new NWaitSecond(delaySeconds).startWaitable().addCompletedEvent(() =>
            {
                release(item);
            });
            return item;
        }
        IEnumerable<T> getInstances(int count, Transform parent, bool worldPositionStays = true)
        {
            for (int i = 0; i < count; ++i)
            {
                yield return get(parent, worldPositionStays);
            }
        }
        void release(T instance);
        void clearPool();
        bool poolContains(T instance);
    }

    [Serializable]
    public class InstancesPool<T> : ISerializationCallbackReceiver, IInstancePool<T>
        where T : Object
    {
        [SerializeField] private T _prefab;
        [SerializeField] private bool _isSharedPool;
        [SerializeField] private int _startupInstanceCount = 0;
        [SerializeField] private bool _useOriginPrefab;

        private int _currentStartupInstanceCount;
        private uint _clonedCount;

        private OriginInstance _origin;
        [NonSerialized] private InstancesPool<GameObject> _sharedPool;
        [NonSerialized] private List<T> _instancePool;

        private void requireCall()
        {
            InternalCheck.checkEditorMode();
            if (_instancePool == null)
            {
                _instancePool = new List<T>(_startupInstanceCount);
                Id = InstancesPoolUtil.computePoolId(_prefab);
            }
        }

        internal readonly static bool IS_GENERIC_OF_GAMEOBJECT = typeof(T).Equals(typeof(GameObject));

        public uint MaxPoolInstanceCount { get; set; }
        public int Id { get; private set; }

        private T _copiedPrefab;
        public T Prefab => getPrefab();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T getPrefab()
        {
            InternalCheck.checkEditorMode();
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
            if (!_copiedPrefab)
            {
                _copiedPrefab = Object.Instantiate(_prefab, InstancesPoolContainer.CopiedPrefabContainer, true);
                _copiedPrefab.name = _prefab.name;
            }
            return _copiedPrefab;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void updateStartupInstances()
        {
            if (_isSharedPool)
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
            InternalCheck.checkEditorMode();
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
        private InstancesPool<GameObject> SharedPool => _isSharedPool ? 
            throw new Exception($"Not support when {nameof(_isSharedPool)} is true") : 
            _sharedPool ??= SharedInstancesPool.getOrCreatePool(getGameObject(_prefab), _startupInstanceCount);
        private T createNewInstance()
        {
            if (_origin == null)
            {
                var go = getGameObject(getPrefab());
                if (go.TryGetComponent(out _origin))
                {
                    Debug.LogWarning($"Already OriginInstance component in {go.name}");
                }
                else
                {
                    _origin = go.AddComponent<OriginInstance>();
                    _origin.setPoolId(Id);
                }
                if (MaxPoolInstanceCount == 0)
                {
                    MaxPoolInstanceCount = IPoolable.DEFAULT_MAX_POOL_ITEM_COUNT;
                }
            }
            var ins = Object.Instantiate(getPrefab(), InstancesPoolContainer.Container, true);
            ins.name = $"{_origin.name} [PoolClone_{++_clonedCount}]";
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
        public InstancesPool(T prefab, int startupInstanceCount = 0, bool isSharedPool = false)
        {
            InternalCheck.checkEditorMode();
            _prefab = prefab;
            _startupInstanceCount = startupInstanceCount;
            _isSharedPool = isSharedPool;

            updateStartupInstances();
        }
        public T get(Transform parent = null, bool worldPositionStays = true)
        {
            requireCall();
            if (_isSharedPool)
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
                using var poolableArray = NPList<IPoolable>.get();
                go.GetComponentsInChildren(true, poolableArray.Collection);
                foreach (var poolable in poolableArray)
                {
                    poolable.onSpawn();
                }
                return ins;
            }
            return InstancesPoolUtil.getInstanceFromGO<T>(SharedPool.get(parent, worldPositionStays));
        }
        internal void release(T instance, GameObject go, OriginInstance origin)
        {
            if (_isSharedPool)
            {
                if (origin.IsInPool)
                {
                    Debug.LogWarning($"Instance has been in pool", instance);
                    return;
                }
                if (origin.Id != _origin.Id)
                {
                    Debug.LogWarning($"OriginId [{origin.Id}], [{_origin.Id}] not match, try release by SharedInstancesPool", go);
                    SharedInstancesPool.getPool(origin.Id)?.release(go);
                    return;
                }

                using var poolableArray = NPList<IPoolable>.get();
                go.GetComponentsInChildren(true, poolableArray.Collection);

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

                go.transform.setParent(InstancesPoolContainer.Container);
                foreach (var poolable in poolableArray)
                {
                    poolable.onDespawn();
                }
            }
            else
            {
                SharedPool.release(getGameObject(instance), go, origin);
            }
        }
        public void release(T instance)
        {
            InternalCheck.checkEditorMode();
            if (_isSharedPool)
            {
                if (_origin == null)
                {
                    Debug.LogWarning($"Origin of pool is null?", instance);
                    return;
                }

                var go = getGameObject(instance);
                if (!go.TryGetComponent<OriginInstance>(out var origin))
                {
                    Debug.LogWarning($"Missing OriginInstance on object: {instance.name}", instance);
                    return;
                }

                release(instance, go, origin);
            }
            else
            {
                SharedPool.release(getGameObject(instance));
            }
        }
        public void clearPool()
        {
            clearPool(true);
        }
        public void clearPool(bool isClearSharedPool)
        {
            InternalCheck.checkEditorMode();
            if (_isSharedPool)
            {
                if (_instancePool != null && _instancePool.Count > 0)
                {
                    foreach (var item in _instancePool)
                    {
                        var go = getGameObject(item);
                        using var poolableArray = NPList<IPoolable>.get();
                        go.GetComponentsInChildren(true, poolableArray.Collection);
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
            if (_isSharedPool)
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
    }

    public struct SharedPoolWrapper<T> : IInstancePool<T> where T : Object
    {
        public readonly int poolId;
        public SharedPoolWrapper(int poolId)
        {
            this.poolId = poolId;
        }
        public T get(Transform parent = null, bool worldPositionStays = true)
        {
            return InstancesPoolUtil.getInstanceFromGO<T>(SharedInstancesPool.getPool(poolId).get(parent, worldPositionStays));
        }
        public void release(T instance)
        {
            SharedInstancesPool.getPool(poolId).release(InstancesPoolUtil.getGameObject(instance));
        }
        public void clearPool()
        {
            SharedInstancesPool.clearSharedPool(poolId);
        }
        public bool poolContains(T instance)
        {
            return SharedInstancesPool.getPool(poolId).poolContains(InstancesPoolUtil.getGameObject(instance));
        }
    }
}