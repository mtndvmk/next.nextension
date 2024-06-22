using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Nextension
{
    public class PoolableComponentPool<T> where T : Component, IPoolable
    {
        [SerializeField] private T _prefab;
        [SerializeField] private int _startupInstanceCount = 0;
        [SerializeField] private bool _useOriginPrefab;

        private int _currentStartupInstanceCount;
        private uint _clonedCount;

        private OriginInstance _origin;
        private HashSet<T> _instancePool;
        private T _copiedPrefab;

        public uint MaxPoolInstanceCount { get; set; }
        public int Id { get; private set; }

        public T Prefab => getPrefab();

        private int computePoolId()
        {
            return _prefab.GetHashCode();
        }
        private void requireCall()
        {
            InternalCheck.checkEditorMode();
            if (_instancePool == null)
            {
                _instancePool = new HashSet<T>(_startupInstanceCount);
                Id = computePoolId();
            }
        }

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
            updateStartupInstances(null);
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
                    _origin.setPoolId(Id, false);
                }
                if (MaxPoolInstanceCount == 0)
                {
                    MaxPoolInstanceCount = IPoolable.DEFAULT_MAX_POOL_ITEM_COUNT;
                }
            }
            var ins = Object.Instantiate(getPrefab(), InstancesPoolContainer.Container, true);
            ins.name = $"{_origin.name} [PoolClone_{++_clonedCount}]";
            ins.onCreate();
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
            return prefab.gameObject;
        }
        public PoolableComponentPool(T prefab, int startupInstanceCount = 0)
        {
            InternalCheck.checkEditorMode();
            _prefab = prefab;
            _startupInstanceCount = startupInstanceCount;

            updateStartupInstances();
        }
        public T get(Transform parent = null, bool worldPositionStays = true)
        {
            requireCall();
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
            ins.onSpawn();
            return ins;
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T getAndDelayRelease(float delaySeconds, Transform parent = null, bool worldPositionStays = true)
        {
            return getAndRelease(new NWaitSecond(delaySeconds), parent, worldPositionStays);
        }
        public IEnumerable<T> getInstances(int count, Transform parent, bool worldPositionStays = true)
        {
            for (int i = 0; i < count; ++i)
            {
                yield return get(parent, worldPositionStays);
            }
        }
        internal void release(T instance, GameObject go, OriginInstance origin)
        {
            if (origin.Id != _origin.Id)
            {
                Debug.LogError($"OriginId [{origin.Id}], [{_origin.Id}] not match", go);
                return;
            }
            instance.onDespawn();
            if (_instancePool.Count >= MaxPoolInstanceCount)
            {
                NUtils.destroy(go);
                instance.onDestroy();
                return;
            }

            go.transform.setParent(InstancesPoolContainer.Container);
            _instancePool.Add(instance);
        }
        public void release(T instance)
        {
            InternalCheck.checkEditorMode();
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
                Debug.LogError($"Missing OriginInstance on object: {instance.name}", instance);
                return;
            }

            if (origin.Id != _origin.Id)
            {
                Debug.LogError($"OriginId [{origin.Id}], [{_origin.Id}] not match", instance);
                return;
            }

            instance.onDespawn();

            if (_instancePool.Count >= MaxPoolInstanceCount)
            {
                instance.onDestroy();
                NUtils.destroy(go);
                return;
            }

            go.transform.setParent(InstancesPoolContainer.Container);
            _instancePool.Add(instance);
        }
        public void clearPool()
        {
            InternalCheck.checkEditorMode();
            if (_instancePool != null && _instancePool.Count > 0)
            {
                foreach (var item in _instancePool)
                {
                    item.onDestroy();
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
