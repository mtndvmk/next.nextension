using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Nextension
{
    public static class InstancesPool
    {
        public static void release(GameObject gameObject, bool invokeIPoolableEvent = false)
        {
            var origin = gameObject.GetComponent<OriginInstance>();
            if (origin == null)
            {
                Debug.LogError($"{gameObject.name} is not from pool");
            }
            else
            {
                if (origin.IsUniquePool)
                {
                    var pool = SharedInstancesPool.getPool(origin.Id);
                    if (pool != null)
                    {
                        pool.release(gameObject, gameObject, origin, invokeIPoolableEvent);
                    }
                    else
                    {
                        Debug.LogError($"Can't find pool - {origin.Id}");
                    }
                }
                else
                {
                    Debug.LogError("Only support unique pool, destroy gameObject");
                }
            }
        }
        public static void release(Component component, bool invokeIPoolableEvent = false)
        {
            release(component.gameObject, invokeIPoolableEvent);
        }
    }

    [Serializable]
    public class InstancesPool<T> : ISerializationCallbackReceiver
        where T : Object
    {
        [SerializeField] private T _prefab;
        [SerializeField] private bool _isUniquePool;
        [SerializeField] private int _startupInstanceCount = 0;
        [SerializeField] private bool _useOriginPrefab; 

        private int _currentStartupInstanceCount;
        private uint _clonedCount;

        private OriginInstance _origin;
        [NonSerialized] private InstancesPool<GameObject> _sharedPool;
        [NonSerialized] private HashSet<T> _instancePool;

        private void requireCall()
        {
            InternalCheck.checkEditorMode();
            if (_instancePool == null)
            {
                _instancePool = new HashSet<T>(_startupInstanceCount);
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
            if (_isUniquePool)
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
        private InstancesPool<GameObject> SharedPool => _isUniquePool ? throw new Exception("SharedPool is not valid for UniquePool") : _sharedPool ??= SharedInstancesPool.getOrCreatePool(getGameObject(_prefab), _startupInstanceCount);
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
                    _origin.setPoolId(Id, _isUniquePool);
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
        private static GameObject getGameObject(T prefab)
        {
            return InstancesPoolUtil.getGameObject(prefab);
        }
        public InstancesPool(T prefab, int startupInstanceCount = 0, bool isUniquePool = false)
        {
            InternalCheck.checkEditorMode();
            _prefab = prefab;
            _startupInstanceCount = startupInstanceCount;
            _isUniquePool = isUniquePool;

            updateStartupInstances();
        }
        public T get(Transform parent = null, bool worldPositionStays = true, bool invokeIPoolableEvent = false)
        {
            requireCall();
            if (_isUniquePool)
            {
                T ins;
                if (_instancePool.Count > 0)
                {
                    ins = _instancePool.takeAndRemoveFirst();
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
            return InstancesPoolUtil.getInstanceFromGO<T>(SharedPool.get(parent, worldPositionStays));
        }
        public T getAndRelease(IWaitable releaseWaitable, Transform parent = null, bool worldPositionStays = true, bool invokeIPoolableEvent = false)
        {
            T item = get(parent, worldPositionStays, invokeIPoolableEvent);
            releaseWaitable.startWaitable().addCompletedEvent(() =>
            {
                release(item, invokeIPoolableEvent);
            });
            return item;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T getAndDelayRelease(float delaySeconds, Transform parent = null, bool worldPositionStays = true, bool invokeIPoolableEvent = false)
        {
            return getAndRelease(new NWaitSecond(delaySeconds), parent, worldPositionStays, invokeIPoolableEvent);
        }
        public IEnumerable<T> getInstances(int count, Transform parent, bool worldPositionStays = true, bool invokeIPoolableEvent = false)
        {
            for (int i = 0; i < count; ++i)
            {
                yield return get(parent, worldPositionStays, invokeIPoolableEvent);
            }
        }
        internal void release(T instance, GameObject go, OriginInstance origin, bool invokeIPoolableEvent = false)
        {
            if (_isUniquePool)
            {
                if (origin.Id != _origin.Id)
                {
                    Debug.LogWarning($"OriginId [{origin.Id}], [{_origin.Id}] not match, try release by SharedInstancesPool", go);
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
                if (_instancePool.Count >= MaxPoolInstanceCount)
                {
                    NUtils.destroy(go);
                    if (invokeIPoolableEvent)
                    {
                        foreach (var poolable in go.GetComponentsInChildren<IPoolable>(true))
                        {
                            poolable.onDestroyed();
                        }
                    }
                    return;
                }

                go.transform.setParent(InstancesPoolContainer.Container);
                _instancePool.Add(instance);
            }
            else
            {
                SharedPool.release(getGameObject(instance), go, origin, invokeIPoolableEvent);
            }
        }
        public void release(T instance, bool invokeIPoolableEvent = false)
        {
            InternalCheck.checkEditorMode();
            if (_isUniquePool)
            {
                if (_origin == null)
                {
                    Debug.LogWarning($"Origin of pool is null?", instance);
                    return;
                }

                var isExist = _instancePool.Contains(instance);
                if (isExist)
                {
                    Debug.LogWarning($"Instance has been in pool", instance);
                    return;
                }

                var go = getGameObject(instance);
                if (!go.TryGetComponent<OriginInstance>(out var origin))
                {
                    Debug.LogWarning($"Missing OriginInstance on object: {instance.name}", instance);
                    return;
                }

                if (origin.Id != _origin.Id)
                {
                    Debug.LogWarning($"OriginId [{origin.Id}], [{_origin.Id}] not match, try release by SharedInstancesPool", instance);
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

                if (_instancePool.Count >= MaxPoolInstanceCount)
                {
                    NUtils.destroy(go);
                    if (invokeIPoolableEvent)
                    {
                        foreach (var poolable in go.GetComponentsInChildren<IPoolable>(true))
                        {
                            poolable.onDestroyed();
                        }
                    }
                    return;
                }

                go.transform.setParent(InstancesPoolContainer.Container);
                _instancePool.Add(instance);
            }
            else
            {
                SharedPool.release(getGameObject(instance), invokeIPoolableEvent);
            }
        }
        public void clearPool(bool isClearSharedPool = true)
        {
            InternalCheck.checkEditorMode();
            if (!_isUniquePool)
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
            if (_instancePool != null && _instancePool.Count > 0)
            {
                foreach (var item in _instancePool)
                {
                    NUtils.destroyObject(item);
                }
                _instancePool.Clear();
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
}