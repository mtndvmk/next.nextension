using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
    public class InsContainer<T>
        where T : Object
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

        public InsContainer(T prefab, Transform container = null)
        {
            checkPrefab(prefab);
            _prefab = prefab;
            _container = container;
        }
        public InsContainer(T prefab, int startupInstanceCount, Transform container = null)
        {
            checkPrefab(prefab);
            _prefab = prefab;
            _container = container;
            _startupInstanceCount = startupInstanceCount;
        }

        [SerializeField] private T _prefab;
        [SerializeField] private Transform _container;
        [SerializeField] private int _startupInstanceCount = 0;
        [SerializeField] private bool _useSharedInsPool;

        private List<T> _instanceList;

        public T Prefab => _prefab;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void requireCall()
        {
            EditorCheck.checkEditorMode();
            if (_instanceList == null)
            {
                createInstanceList();
            }
        }
        private void createInstanceList()
        {
            _instanceList = new(_startupInstanceCount);
            if (_startupInstanceCount > 0)
            {
                createInstances(_startupInstanceCount);
            }
        }
        private void createInstances(int count, bool isActive = false)
        {
            while (_instanceList.Count < count)
            {
                T ins;
                if (_useSharedInsPool)
                {
                    var pool = SharedInsPoolUtil.getOrCreatePool(_prefab, _startupInstanceCount);
                    var go = pool.get(_container);
                    go.transform.resetTransform();
                    ins = InsPoolUtil.getInstanceFromGO<T>(go);
                }
                else
                {
                    ins = Object.Instantiate(_prefab, _container);
                }
#if UNITY_EDITOR
                if (!_useSharedInsPool)
                {
                    ins.name = _prefab.name + $" [Clone_{_instanceList.Count}]";
                }
#endif
                ins.setActive(isActive);
                _instanceList.Add(ins);
            }
        }

        private int _activatedCount;

        public Transform Container => _container;
        public int ActivatedCount => _activatedCount;
        public int TotalCount => _instanceList == null ? 0 : _instanceList.Count;

        public void beginGetInstance()
        {
            beginGetInstanceFrom(0);
        }
        public Span<T> getInstances(int count, int fromIndex = 0)
        {
            beginGetInstanceFrom(fromIndex);
            var result = getNext(count);
            endGetInstance();
            return result;
        }
        public void beginGetInstanceFrom(int fromIndex)
        {
            requireCall();
            _activatedCount = fromIndex;
            if (_useSharedInsPool && _instanceList != null)
            {
                for (int i = _instanceList.Count - 1; i >= 0; i--)
                {
                    var ins = _instanceList[i];
                    var go = InsPoolUtil.getGameObject(ins);
                    var parent = go.transform.parent;
                    if (parent == _container)
                    {
                        continue;
                    }
                    _instanceList.removeAt(i);
                }
            }
        }
        public void endGetInstance()
        {
            requireCall();
            var span = _instanceList.asSpan();
            int instanceCount = span.Length;
            if (_useSharedInsPool)
            {
                releaseSharedInsPool(_activatedCount);
            }
            else
            {
                for (int i = _activatedCount; i < instanceCount; ++i)
                {
                    span[i].setActive(false);
                }
            }
        }
        private void releaseSharedInsPool(int fromIndex)
        {
            var span = _instanceList.asSpan();
            int instanceCount = span.Length;
            var pool = SharedInsPoolUtil.getPool(_prefab);
            for (int i = fromIndex; i < instanceCount; ++i)
            {
                pool.release(InsPoolUtil.getGameObject(span[i]));
            }
            _instanceList.RemoveRange(fromIndex, instanceCount - fromIndex);
        }
        public T getNext()
        {
            requireCall();
            if (_activatedCount >= _instanceList.Count)
            {
                createInstances(_activatedCount + 1, true);
            }
            var ins = _instanceList[_activatedCount++];
            if (_useSharedInsPool)
            {
                var go = InsPoolUtil.getGameObject(ins);
                var parent = go.transform.parent;
                go.transform.resetTransform(_container);
            }
            ins.setActive(true);
            return ins;
        }
        public Span<T> getNext(int count)
        {
            int start = _activatedCount;
            for (int i = 0; i < count; ++i)
            {
                getNext();
            }
            return _instanceList.asSpan()[start.._activatedCount];
        }
        public Span<T> asActivatedItemSpan()
        {
            requireCall();
            return _instanceList.asSpan()[.._activatedCount];
        }
        public Span<T> asAllItemSpan()
        {
            requireCall();
            return _instanceList.asSpan();
        }
        public T getInstanceAt(int index)
        {
            requireCall();
            return _instanceList[index];
        }
        public T this[int index] => getInstanceAt(index);

        public void deactiveAll()
        {
            beginGetInstance();
            endGetInstance();
        }
        public void clearInstances()
        {
            if (_instanceList == null || _instanceList.Count == 0)
            {
                return;
            }
            if (_useSharedInsPool)
            {
                releaseSharedInsPool(0);
            }
            else
            {
                foreach (var ins in _instanceList.asSpan())
                {
                    NUtils.destroyObject(ins);
                }
            }
            _instanceList.Clear();
        }
        public void clearInstances(int keepCount)
        {
            if (_instanceList == null)
            {
                return;
            }
            var listCount = _instanceList.Count;
            var removeCount = listCount - keepCount;
            if (removeCount > 0)
            {
                var span = _instanceList.asSpan();
                if (_useSharedInsPool)
                {
                    releaseSharedInsPool(keepCount);
                }
                else
                {
                    for (int i = keepCount; i < listCount; ++i)
                    {
                        NUtils.destroyObject(span[i]);
                    }
                }
                _instanceList.RemoveRange(keepCount, removeCount);
            }
        }
        public void clearInstancesFitIn()
        {
            clearInstances(_activatedCount);
        }
        public void clearInstancesPrefer(int count)
        {
            clearInstances(count < _activatedCount ? _activatedCount : count);
        }
    }
}