using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Nextension
{
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
                    _container.gameObject.hideFlags = HideFlags.DontSave;
                    _container.setActive(false);
                    GameObject.DontDestroyOnLoad(_container.gameObject);
                }
                return _container;
            }
        }
    }
    internal static class SharedInstancesPool
    {
        private static Dictionary<int, InstancesPool<GameObject>> _sharedPool = new Dictionary<int, InstancesPool<GameObject>>();
        public static InstancesPool<GameObject> getOrCreatePool(GameObject prefab)
        {
            var insId = prefab.GetInstanceID();
            InstancesPool<GameObject> pool;
            if (!_sharedPool.ContainsKey(insId))
            {
                pool = new InstancesPool<GameObject>(prefab);
                pool.notUseSharedPool = true;
                _sharedPool.Add(insId, pool);
            }
            else
            {
                pool = _sharedPool[insId];
            }
            return pool;
        }
        public static InstancesPool<GameObject> getPool(int id)
        {
            if (_sharedPool.TryGetValue(id, out var pool)) return pool;
            return null;
        }
    }
    [Serializable]
    public class InstancesPool<T> : ISerializationCallbackReceiver where T : Object
    {
        public void OnBeforeSerialize()
        {

        }
        public void OnAfterDeserialize()
        {
            _instances = new Queue<T>();
        }
        private static Exception NOT_SUPPORT_EXCEPTION(Type type) => new Exception($"Not support InstancesPool for {type}");
        private Queue<T> _instances = new Queue<T>();
        [SerializeField] private T prefab;
        [SerializeField] internal bool notUseSharedPool;

        [NonSerialized] private InstancesPool<GameObject> _sharedPool;
        private OriginInstance _origin;

        private T _copiedPrefabOnEditor;
        private T getPrefab()
        {
#if UNITY_EDITOR
            if (!_copiedPrefabOnEditor)
            {
                _copiedPrefabOnEditor = Object.Instantiate(prefab);
            }
            return _copiedPrefabOnEditor;
#else
            return prefab;
#endif
        }

        private InstancesPool<GameObject> SharedPool
        {
            get
            {
                if (_sharedPool == null)
                {
                    _sharedPool = SharedInstancesPool.getOrCreatePool(getGameObject(getPrefab()));
                }
                return _sharedPool;
            }
        }

        private T createNew()
        {
            if (_origin == null)
            {
                var go = getGameObject(getPrefab());
                _origin = go.getOrAddComponent<OriginInstance>();
                _origin.Id = go.GetInstanceID();
            }
            var ins = GameObject.Instantiate(getPrefab(), InstancesPoolContainer.Container, true);
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
            var typeOfT = typeof(T);
            if (typeOfT.Equals(typeof(GameObject)))
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
            InternalUtils.checkEditorMode();
            this.prefab = prefab;
        }

        public T getOneInstance(Transform parent = null, bool worldPositionStays = true)
        {
            InternalUtils.checkEditorMode();
            if (notUseSharedPool)
            {
                T ins;
                if (_instances.Count > 0)
                {
                    ins = _instances.Dequeue();
                }
                else
                {
                    ins = createNew();
                }

                var go = getGameObject(ins);
                go.transform.setParent(parent, worldPositionStays);
                foreach (var poolable in go.GetComponentsInChildren<IPoolable>(true))
                {
                    poolable.onSpawned();
                }
                return ins;
            }
            return getInstanceFromGO(SharedPool.getOneInstance(parent, worldPositionStays));
        }
        public IEnumerable<T> getInstances(int count, Transform parent, bool worldPositionStays = true) 
        {
            for (int i = 0; i < count; i++)
            {
                yield return getOneInstance(parent, worldPositionStays);
            }
        }
        public void sendToPool(T instance)
        {
            InternalUtils.checkEditorMode();
            if (instance == null)
            {
                return;
            }
            if (notUseSharedPool)
            {
                if (_origin == null)
                {
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
                    SharedInstancesPool.getPool(origin.Id)?.sendToPool(go);
                    return;
                }
                go.transform.setParent(InstancesPoolContainer.Container);
                _instances.Enqueue(instance);
                foreach (var poolable in go.GetComponentsInChildren<IPoolable>(true))
                {
                    poolable.onDespawned();
                }
                return;
            }
            SharedPool.sendToPool(getGameObject(instance));
        }
        public void cleanPool(bool isCleanSharedPool = false)
        {
            InternalUtils.checkEditorMode();
            if (isCleanSharedPool && !notUseSharedPool)
            {
                SharedPool.cleanPool();
                return;
            }
            while (_instances.Count > 0)
            {
                NUtils.destroyObject(_instances.Dequeue());
            }
        }
    }

    public class OriginInstance : MonoBehaviour
    {
        [field: SerializeField] public int Id { get; internal set; }
    }
    public interface IPoolable
    {
        public void onSpawned();
        public void onDespawned();
    }
}
