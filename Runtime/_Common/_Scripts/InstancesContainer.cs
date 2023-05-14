using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Nextension
{
    [Serializable]
    public class InstancesContainer<T> where T : Object
    {
        private static Exception NOT_SUPPORT_EXCEPTION(Type type) => new Exception($"Not support InstancesContainer for {type}");
        public InstancesContainer(T prefab, Transform container = null, int startInstanceCount = 0)
        {
            if (prefab == null) throw new ArgumentNullException("InstancesContainer.prefab");
            if (!(prefab is Component || prefab is GameObject))
            {
                throw NOT_SUPPORT_EXCEPTION(typeof(T));
            }
            _prefab = prefab;
            _container = container;
            if (startInstanceCount > 0)
            {
                createInstances(startInstanceCount);
            }
        }
        [SerializeField] private T _prefab;
        [SerializeField] private Transform _container;
        private List<T> _instances = new List<T>();
        private int _index;

        public void beginGetInstance()
        {
            _index = 0;
        }
        public void endGetInstance()
        {
            for (int i = _index; i < _instances.Count; i++)
            {
                _instances[i].setActive(false);
            }
            _index = 0;
        }
        public T getNext()
        {
            if (_index >= _instances.Count)
            {
                createInstances(_index + 1);
            }
            var ins = _instances[_index++];
            ins.setActive(true);
            return ins;
        }


        private void createInstances(int count, bool isActive = false)
        {
            if (_instances == null)
            {
                _instances = new List<T>();
            }
            while (_instances.Count < count)
            {
                var ins = GameObject.Instantiate(_prefab, _container);
                ins.setActive(isActive);
                _instances.Add(ins);
            }
        }

        public IEnumerable<T> getInstances(int count)
        {
            createInstances(count);
            for (int i = 0; i < count; i++)
            {
                _instances[i].setActive(true);
                yield return _instances[i];
            }
            for (int i = count; i < _instances.Count; i++)
            {
                _instances[i].setActive(false);
            }
            _index = 0;
        }
        public T getOneInstance()
        {
            return getInstances(1).First();
        }
        public void clearInstances()
        {
            foreach (var ins in _instances)
            {
                NUtils.destroyObject(ins);
            }
            _instances.Clear();
        }
    }
}
