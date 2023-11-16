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
    public class InstancesPool<T> : ISerializationCallbackReceiver
        where T : Object
    {
        private static Exception NOT_SUPPORT_EXCEPTION(Type type) => new Exception($"Not support InstancesPool for {type}");

        [SerializeField] private T _prefab;
        [SerializeField] private bool _isUniquePool;
        [SerializeField] private int _startupInstanceCount = 0;

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
                Id = getGameObject(_prefab).GetInstanceID();
            }
        }

        private readonly static bool IS_GENERIC_OF_GAMEOBJECT = typeof(T).Equals(typeof(GameObject));

        public uint MaxPoolInstanceCount { get; set; }
        public int Id {get; private set;}

#if UNITY_EDITOR
        private T _copiedPrefabInEditor;
#endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T getPrefab()
        {
#if UNITY_EDITOR
            if (!_copiedPrefabInEditor)
            {
                _copiedPrefabInEditor = Object.Instantiate(_prefab, InstancesPoolContainer.EditorPrefabContainer, true);
                _copiedPrefabInEditor.name = _prefab.name;
            }
            return _copiedPrefabInEditor;
#else
            return _prefab;
#endif
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
                }
                _origin.setPoolId(_prefab.GetInstanceID(), _isUniquePool);
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
            if (prefab is GameObject go)
            {
                return go;
            }
            else if (prefab is Component com)
            {
                return com.gameObject;
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
                if (go.TryGetComponent<T>(out var com))
                {
                    return com;
                }
                throw NOT_SUPPORT_EXCEPTION(typeof(T));
            }
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
            return getInstanceFromGO(SharedPool.get(parent, worldPositionStays));
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
#if UNITY_EDITOR
            if (_copiedPrefabInEditor)
            {
                NUtils.destroyObject(_copiedPrefabInEditor);
                _copiedPrefabInEditor = null;
            }
#endif
        }

        public void OnBeforeSerialize()
        {

        }

        public void OnAfterDeserialize()
        {
#if !UNITY_EDITOR
            (new NWaitUntil(() => NStartRunner.IsPlaying)).startWaitable().addCompletedEvent(updateStartupInstances);
#endif
        }
    }
    internal static class SharedInstancesPool
    {
        [EditorQuittingMethod]
        static void reset()
        {
            foreach (var sharedPool in _sharedPools.Values)
            {
                sharedPool.clearPool();
            }
            _sharedPools.Clear();
        }
        private static Dictionary<int, InstancesPool<GameObject>> _sharedPools = new();
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
    internal class InstancesPoolContainer : MonoBehaviour
    {
#if UNITY_EDITOR
        private static Transform _editorPrefabContainer;
        public static Transform EditorPrefabContainer
        {
            get
            {
                InternalCheck.checkEditorMode();
                if (!_editorPrefabContainer)
                {
                    _editorPrefabContainer = new GameObject("___EditorPrefabContainer").transform;
                    _editorPrefabContainer.SetParent(Container);
                }
                return _editorPrefabContainer;
            }
        }
#endif
        private static Transform _container;
        public static Transform Container
        {
            get
            {
                InternalCheck.checkEditorMode();
                if (!_container)
                {
                    _container = new GameObject("[InstancesPool.Container]").transform;
                    _container.setActive(false);
                    DontDestroyOnLoad(_container.gameObject);
                }
                return _container;
            }
        }
    }
}