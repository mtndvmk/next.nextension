using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Nextension
{

    public static class InsPool
    {
        internal static uint DEFAULT_MAX_POOL_ITEM_COUNT = 100;
        public static void setDefaultMaxPoolCount(uint maxPoolSize) { DEFAULT_MAX_POOL_ITEM_COUNT = maxPoolSize; }
        public static bool isReleaseable(this GameObject gameObject)
        {
            if (!gameObject || !gameObject.TryGetComponent<OriginInstance>(out var origin))
            {
                return false;
            }
            else
            {
                return !origin.IsInPool;
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

        public static SharedInsPool<T> getSharedPool<T>(this T prefab) where T : Object
        {
            var go = InsPoolUtil.getGameObject(prefab);
            if (go.TryGetComponent<OriginInstance>(out var originInstance))
            {
                return new SharedInsPool<T>(originInstance.Id);
            }
            else
            {
                var poolId = InsPoolUtil.computePoolId(prefab);
                SharedInsPoolUtil.getOrCreatePool(poolId, go, 0);
                return new SharedInsPool<T>(poolId);
            }
        }
        public static SharedInsPool<T> getSharedPoolWithoutChecks<T>(this T prefab) where T : Object
        {
            var go = InsPoolUtil.getGameObject(prefab);
            var poolId = InsPoolUtil.computePoolId(prefab);
            SharedInsPoolUtil.getOrCreatePool(poolId, go, 0);
            return new SharedInsPool<T>(poolId);
        }
    }

    [Serializable]
    public class InsPool<T> : AbsInsPool<T>, ISerializationCallbackReceiver
        where T : Object
    {
        [SerializeField] private T _prefab;
        [SerializeField] private bool _disableSharedPool;
        [SerializeField] private int _startupInstanceCount = 0;
        [SerializeField] private bool _useOriginPrefab;

        [NonSerialized] private InsPool<GameObject> _sharedPool;
        [NonSerialized] private List<T> _instancePool;

        private int _currentStartupInstanceCount;
        private uint _clonedCount;
        private int _id;
        private T _copiedPrefab;
        public override int Id => _id;
        public T Prefab => getPrefab();
        public uint MaxPoolInstanceCount { get; set; }

        private void requireCall()
        {
            EditorCheck.checkEditorMode();
            if (_instancePool == null)
            {
                _instancePool = new List<T>(_startupInstanceCount);
                _id = InsPoolUtil.computePoolId(_prefab);
                if (_useOriginPrefab)
                {
                    getGameObjectFromT(_prefab).getOrAddComponent<OriginInstance>().setPool(this, true);
                }
                else
                {
                    _copiedPrefab = Object.Instantiate(_prefab, InsPoolContainer.CopiedPrefabContainer, true);
                    _copiedPrefab.name = _prefab.name;
                    getGameObjectFromT(_copiedPrefab).getOrAddComponent<OriginInstance>().setPool(this, true);
                }
                if (MaxPoolInstanceCount == 0)
                {
                    MaxPoolInstanceCount = InsPool.DEFAULT_MAX_POOL_ITEM_COUNT;
                }
            }
        }
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
        private InsPool<GameObject> SharedPool => _disableSharedPool ?
            throw new Exception($"Not support when {nameof(_disableSharedPool)} is true") :
            _sharedPool ??= SharedInsPoolUtil.getOrCreatePool(getGameObjectFromT(_prefab), _startupInstanceCount);
        private T createNewInstance()
        {
            var ins = Object.Instantiate(getPrefab(), InsPoolContainer.Container, true);
            ins.name = $"{_prefab.name} [PoolClone_{_clonedCount++}]";
            ins.getOrAddComponent<OriginInstance>().setPool(this, false);
            return ins;
        }
        private void createStartupInstances()
        {
            initializeInstancesInPool(_startupInstanceCount);
        }
        public void initializeInstancesInPool(int count)
        {
            EditorCheck.checkEditorMode();
            while (_instancePool.Count < count)
            {
                _instancePool.Add(createNewInstance());
            }
        }
        private static GameObject getGameObjectFromT(T instance)
        {
            return InsPoolUtil.getGameObject(instance);
        }
        public InsPool(T prefab, int startupInstanceCount = 0, bool disableSharedPool = false)
        {
            EditorCheck.checkEditorMode();
            _prefab = prefab;
            _startupInstanceCount = startupInstanceCount;
            _disableSharedPool = disableSharedPool;

            updateStartupInstances();
        }
        public override T get(Transform parent = null, bool worldPositionStays = true)
        {
            requireCall();
            if (_disableSharedPool)
            {
                T ins;
                bool isNewInstance = false;
                if (_instancePool.Count > 0)
                {
                    ins = _instancePool.takeAndRemoveLast();
                }
                else
                {
                    ins = createNewInstance();
                    isNewInstance = true;
                }

                var go = getGameObjectFromT(ins);
                go.transform.setParent(parent, worldPositionStays);

                using var poolableArray = go.getComponentsInChildren_CachedList<IInsPoolable>();
                if (isNewInstance)
                {
                    if (poolableArray.Count > 0)
                    {
                        bool firstIsRoot = (poolableArray[0] as Component).gameObject == go;
                        poolableArray[0].onCreated(firstIsRoot);
                        for (int i = 1; i < poolableArray.Count; ++i)
                        {
                            poolableArray[i].onCreated(false);
                        }
                    }
                }
                if (poolableArray.Count > 0)
                {
                    bool firstIsRoot = (poolableArray[0] as Component).gameObject == go;
                    poolableArray[0].onSpawn(firstIsRoot);
                    for (int i = 1; i < poolableArray.Count; ++i)
                    {
                        poolableArray[i].onSpawn(false);
                    }
                }
                return ins;
            }
            return InsPoolUtil.getInstanceFromGO<T>(SharedPool.get(parent, worldPositionStays));
        }

        public override void clearPool()
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
                        var go = getGameObjectFromT(item);
                        using var poolableArray = go.getComponentsInChildren_CachedList<IInsPoolable>();
                        if (poolableArray.Count > 0)
                        {
                            bool firstIsRoot = (poolableArray[0] as Component).gameObject == go;
                            poolableArray[0].onDestroy(firstIsRoot);
                            for (int i = 1; i < poolableArray.Count; ++i)
                            {
                                poolableArray[i].onDestroy(false);
                            }
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
                        SharedInsPoolUtil.clearSharedPool(_sharedPool.Id);
                        _sharedPool = null;
                    }
                }
            }
        }
        public override bool poolContains(T instance)
        {
            if (_disableSharedPool)
            {
                return _instancePool.Contains(instance);
            }
            else
            {
                return SharedPool.poolContains(getGameObjectFromT(instance));
            }
        }

        public void OnBeforeSerialize()
        {

        }
        public async void OnAfterDeserialize()
        {
#if UNITY_EDITOR
            await TaskEditor.waitFrame(1);
            if (NStartRunner.IsPlaying && _prefab && getGameObjectFromT(_prefab))
            {
                updateStartupInstances();
            }
#else
            await new NWaitFrame(1);
            if (_prefab && getGameObjectFromT(_prefab))
            {
                updateStartupInstances();
            }
#endif
        }

        public override void release(T instance)
        {
            using var otherOrigins = getGameObjectFromT(instance).getComponentsInChildren_CachedList<OriginInstance>(true);
            int otherCount = otherOrigins.Count;
            if (otherCount > 1)
            {
                for (int i = 0; i < otherCount; ++i)
                {
                    var tempGO = otherOrigins[i].gameObject;
                    tempGO.transform.setParent(InsPoolContainer.Container);
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
                    Debug.LogWarning($"OriginId [{origin.Id}], [{Id}] not match, try release by SharedInsPool", go);
                    SharedInsPoolUtil.getPool(origin.Id)?.release(go);
                    return;
                }

                using var poolableArray = go.getComponentsInChildren_CachedList<IInsPoolable>();
                if (_instancePool.Count >= MaxPoolInstanceCount)
                {
                    if (poolableArray.Count > 0)
                    {
                        bool firstIsRoot = (poolableArray[0] as Component).gameObject == go;
                        poolableArray[0].onDespawn(firstIsRoot);
                        poolableArray[0].onDestroy(firstIsRoot);
                        for (int i = 1; i < poolableArray.Count; ++i)
                        {
                            poolableArray[i].onDespawn(false);
                            poolableArray[i].onDestroy(false);
                        }
                    }
                    NUtils.destroy(go);
                    return;
                }
                else
                {
                    _instancePool.Add(instance);
                }

                go.transform.setParent(InsPoolContainer.Container);
                if (poolableArray.Count > 0)
                {
                    bool firstIsRoot = (poolableArray[0] as Component).gameObject == go;
                    poolableArray[0].onDespawn(firstIsRoot);
                    for (int i = 1; i < poolableArray.Count; ++i)
                    {
                        poolableArray[i].onDespawn(false);
                    }
                }
            }
            else
            {
                SharedPool.release(getGameObjectFromT(instance));
            }
        }
    }
}