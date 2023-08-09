using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Nextension
{
    /// <summary>
    /// Create a container for the InstanceList from the prefab.
    /// Call [beginGetInstance] to restart and begin getting the InstanceList like a pool, then call [getNext] to get one active instance, finally call [endGetInstance] to stop getting instance and other instances in the container will be deactivated
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class InstancesContainer<T> where T : Object
    {
        private static Exception NOT_SUPPORT_EXCEPTION(Type type) => new Exception($"Not support InstancesContainer for {type}");

        private static void checkPrefab(Object prefab)
        {
            if (prefab == null) throw new ArgumentNullException("InstancesContainer.prefab");
            if (!(prefab is Component || prefab is GameObject))
            {
                throw NOT_SUPPORT_EXCEPTION(typeof(T));
            }
        }

        public InstancesContainer(T prefab, Transform container = null)
        {
            checkPrefab(prefab);
            _prefab = prefab;
            _container = container;
        }
        [SerializeField] private T _prefab;
        [SerializeField] private Transform _container;

        private List<T> _instanceList;
        private List<T> InstanceList
        {
            get
            {
                if (_instanceList == null)
                {
                    createInstanceList();
                }
                return _instanceList;
            }
        }

        private int _activedCount;

        public Transform Container => _container;
        public int ActivedCount => _activedCount;
        public int TotalCount => _instanceList.Count;

        public void beginGetInstance()
        {
            _activedCount = 0;
        }
        public void endGetInstance()
        {
            for (int i = _activedCount; i < InstanceList.Count; ++i)
            {
                InstanceList[i].setActive(false);
            }
        }
        public T getNext()
        {
            if (_activedCount >= InstanceList.Count)
            {
                createInstances(_activedCount + 1);
            }
            var ins = InstanceList[_activedCount++];
            ins.setActive(true);
            return ins;
        }
        public IEnumerable<T> getNext(int count)
        {
            for (int i = 0; i < count; ++i)
            {
                yield return getNext();
            }
        }
        public IEnumerable<T> enumerateActived()
        {
            for (int i = 0; i < _activedCount; ++i)
            {
                yield return InstanceList[i];
            }
        }
        public IEnumerable<T> enumerateAll()
        {
            return InstanceList.AsEnumerable();
        }
        public T getAt(int index)
        {
            return InstanceList[index];
        }
        public T this[int index] => getAt(index);

        private void createInstanceList()
        {
            _instanceList = new List<T>();
        }
        private void createInstances(int count, bool isActive = false)
        {
            while (InstanceList.Count < count)
            {
                var ins = Object.Instantiate(_prefab, _container);
                ins.setActive(isActive);
                InstanceList.Add(ins);
            }
        }

        public void clearInstances()
        {
            foreach (var ins in InstanceList)
            {
                NUtils.destroyObject(ins);
            }
            InstanceList.Clear();
        }
        public void clearInstances(int keepCount)
        {
            var removeCount = InstanceList.Count - keepCount;
            if (removeCount > 0)
            {
                for (int i = keepCount; i < InstanceList.Count; ++i)
                {
                    NUtils.destroyObject(InstanceList[i]);
                }
                InstanceList.RemoveRange(keepCount, removeCount);
            }
        }
        public void clearInstancesFitIn()
        {
            clearInstances(_activedCount);
        }
        public void clearInstancesPrefer(int count)
        {
            clearInstances(count < _activedCount ? _activedCount : count);
        }
    }
}
