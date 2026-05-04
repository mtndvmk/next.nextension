using System;
using Unity.Collections;

namespace Nextension
{
    public struct NativeSegmentTree : IDisposable
    {
        private NativeArray<float> _tree;
        private int _capacity;
        private int _size;
        private Allocator _allocator;

        public int Size => _size;
        public float TotalSum => _tree.IsCreated && _tree.Length > 1 ? _tree[1] : 0;
        public bool IsCreated => _tree.IsCreated;

        public NativeSegmentTree(int capacity, Allocator allocator)
        {
            _allocator = allocator;
            _size = 0;
            _capacity = NUtils.nextPOT(capacity);
            // Array must be 2 * _capacity to store both leaves and internal nodes
            _tree = new NativeArray<float>(_capacity << 1, allocator);
        }

        public void EnsureCapacity(int newCapacity)
        {
            if (newCapacity <= _capacity) return;
            int oldCapacity = _capacity;
            _capacity = NUtils.nextPOT(newCapacity);

            // Segment tree requires exactly 2 * capacity to fit parents + leaves
            var newTree = new NativeArray<float>(_capacity << 1, _allocator);

            if (_size > 0)
            {
                NativeArray<float>.Copy(_tree, oldCapacity, newTree, _capacity, _size);

                _tree.Dispose();
                _tree = newTree;

                int lastLeafParent = (_capacity + _size - 1) >> 1;
                for (int i = lastLeafParent; i > 0; i--)
                {
                    _tree[i] = _tree[i << 1] + _tree[(i << 1) | 1];
                }
            }
            else
            {
                _tree.Dispose();
                _tree = newTree;
            }
        }

        public void SetSize(int size)
        {
            if (size > _capacity)
            {
                EnsureCapacity(size);
            }

            if (size == 0)
            {
                Clear();
                return;
            }
            
            if (size < _size)
            {
                for (int i = size; i < _size; i++)
                {
                    Set(i, 0);
                }
            }

            _size = size;
        }

        public void Clear()
        {
            if (_size > 0)
            {
                _tree.fill(0, _size);
                _size = 0;
            }
        }

        public void Set(int index, float value)
        {
            if (index < 0 || index >= _size) return;
            index += _capacity;
            _tree[index] = value;
            while (index > 1)
            {
                index >>= 1;
                _tree[index] = _tree[index << 1] + _tree[(index << 1) | 1];
            }
        }
        
        public float Get(int index)
        {
            if (index < 0 || index >= _size) return 0f;
            return _tree[index + _capacity];
        }

        public float GetSum(int count)
        {
            if (count <= 0) return 0;
            if (count >= _size) return TotalSum;

            float sum = 0;
            int l = _capacity;
            int r = _capacity + count;
            while (l < r)
            {
                if ((l & 1) == 1) sum += _tree[l++];
                if ((r & 1) == 1) sum += _tree[--r];
                l >>= 1;
                r >>= 1;
            }
            return sum;
        }

        public float GetSum(int from, int to)
        {
            if (from > to) return 0;
            if (from <= 0) return GetSum(to + 1);
            return GetSum(to + 1) - GetSum(from);
        }

        public int FindIndex(float targetSum)
        {
            if (targetSum <= 0) return 0;
            if (targetSum >= TotalSum) return _size - 1;

            int i = 1;
            while (i < _capacity)
            {
                float leftVal = _tree[i << 1];
                if (leftVal >= targetSum)
                {
                    i <<= 1;
                }
                else
                {
                    targetSum -= leftVal;
                    i = (i << 1) | 1;
                }
            }
            int index = i - _capacity;
            return index < _size ? index : _size - 1;
        }
        
        public void Dispose()
        {
            if (_tree.IsCreated)
            {
                _tree.Dispose();
            }
        }
    }
}
