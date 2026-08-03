using System;
using System.Collections;
using System.Collections.Generic;

namespace Nextension
{
    public struct ListEnumerator<T> : IEnumerator<T>
    {
        private readonly IReadOnlyList<T> _list;
        private readonly int _count;
        private readonly int _start;
        private int _index;
            
        public readonly T Current => _list[_index];

        readonly object IEnumerator.Current => Current;

        public ListEnumerator(IReadOnlyList<T> list)
        {
            _list = list;
            _count = list?.Count ?? 0;
            _index = -1;
            _start = 0;
        }

        public ListEnumerator(IReadOnlyList<T> list, int start, int length)
        {
            _list = list;
            _start = start;
            _count = start + length;
            _index = start - 1;
        }

        public readonly void Dispose()
        {
            
        }

        public bool MoveNext()
        {
            _index++;
            return _index < _count;
        }

        public void Reset()
        {
            _index = _start - 1;
        }
    }

    public struct ArrayEnumerator<T> : IEnumerator<T>
    {
        private enum Mode
        {
            SingleItem,
            Array
        }
        public ArrayEnumerator(T item)
        {
            _mode = Mode.SingleItem;
            _current = item;
            _index = startIndex = 0;
            
            array = default;
            maxIndex = 1;
        }
        public ArrayEnumerator(T[] array)
        {
            _mode = Mode.Array;
            _index = startIndex = 0;
            _current = default;

            this.array = array;
            maxIndex = (uint)array.Length;
        }
        public ArrayEnumerator(T[] array, uint startIndex)
        {
            _mode = Mode.Array;
            _index = this.startIndex = startIndex;
            _current = default;

            this.array = array;
            maxIndex = (uint)array.Length;
        }
        public ArrayEnumerator(T[] array, uint startIndex, uint count)
        {
            _mode = Mode.Array;
            _index = this.startIndex = startIndex;
            maxIndex = startIndex + count;
            
            this.array = array;
            _current = default;

            if (maxIndex > array.Length)
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        internal readonly T[] array;
        internal readonly uint startIndex;
        internal readonly uint maxIndex;

        private Mode _mode;
        private uint _index;
        private T _current;

        public readonly T Current => _current;
        readonly object IEnumerator.Current
        {
            get
            {
                if (_index == startIndex || _index == maxIndex)
                {
                    throw new InvalidOperationException("InvalidOperation_EnumOpCantHappen");
                }
                return _current;
            }
        }

        public readonly void Dispose()
        {
        }
        public bool MoveNext()
        {
            if (_index < maxIndex)
            {
                if (_mode == Mode.SingleItem)
                {
                    _index++;
                    return true;
                }
                _current = array[_index++];
                return true;
            }
            return false;
        }
        public void Reset()
        {
            _index = startIndex;
            if (_mode != Mode.SingleItem) _current = default;
        }

        public readonly ArrayEnumerator<T> GetEnumerator()
        {
            return this;
        }
    }
    
    public unsafe struct UnsafeArrayEnumerator<T> : IEnumerator<T> where T : unmanaged
    {
        public UnsafeArrayEnumerator(void* array, uint itemCount)
        {
            this.array = (T*)array;
            this.itemCount = itemCount;

            _current = default;
            _index = startIndex = 0;
        }
        public static UnsafeArrayEnumerator<T> createFrom<TSrc>(ArrayEnumerator<TSrc> src) where TSrc : unmanaged
        {
            var sizeOfSrc = sizeof(TSrc);
#if UNITY_EDITOR
            var sizeOfT = NUtils.sizeOf<T>();
            if (sizeOfSrc != sizeOfT)
            {
                NDebug.LogWarning($"SizeOfSrc is different from SizeOfT ({sizeOfSrc} != {sizeOfT})");
            }
#endif
            fixed (TSrc* srcPtr = &src.array[src.startIndex])
            {
                return new UnsafeArrayEnumerator<T>(srcPtr, src.maxIndex - src.startIndex);
            }
        }

        internal readonly T* array;
        internal readonly uint itemCount;
        internal readonly uint startIndex;

        private uint _index;
        private T _current;

        public readonly T Current => _current;
        readonly object IEnumerator.Current
        {
            get
            {
                if (_index == startIndex || _index == itemCount)
                {
                    throw new InvalidOperationException("InvalidOperation_EnumOpCantHappen");
                }
                return _current;
            }
        }

        public readonly void Dispose()
        {
        }
        public bool MoveNext()
        {
            if (_index < itemCount)
            {
                _current = array[_index++];
                return true;
            }
            return false;
        }
        public void Reset()
        {
            _index = startIndex;
            _current = default;
        }

        public readonly UnsafeArrayEnumerator<T> GetEnumerator()
        {
            return this;
        }
    }
    
    public struct RandomEnumerator<T> : IEnumerator<T> where T : unmanaged
    {
        private readonly NPUArray<T> _values;
        private T _current;
        private int _remainingCount;
        private Unity.Mathematics.Random _rand;
        private bool _isDisposable;

        public RandomEnumerator(ReadOnlySpan<T> values, uint seed = 0)
        {
            this._values = NPUArray<T>.getWithoutTracking();
            this._values.AddRange(values);

            this._remainingCount = _values.Count;
            this._rand = NUtils.getRandom(seed);

            _isDisposable = true;
            _current = default;
        }
        public RandomEnumerator(ref NPUArray<T> values, uint seed = 0)
        {
            this._values = values;
            this._remainingCount = _values.Count;
            this._rand = NUtils.getRandom(seed);

            _isDisposable = false;
            _current = default;
        }
        public RandomEnumerator(IEnumerable<T> values, uint seed = 0)
        {
            this._values = NPUArray<T>.getWithoutTracking();
            this._values.AddRange(values);

            this._remainingCount = _values.Count;
            this._rand = NUtils.getRandom(seed);

            _isDisposable = true;
            _current = default;
        }
        public RandomEnumerator(ReadOnlySpan<T> values, ref Unity.Mathematics.Random rand)
        {
            this._values = NPUArray<T>.getWithoutTracking();
            this._values.AddRange(values);

            this._remainingCount = _values.Count;
            this._rand = rand;

            _isDisposable = true;
            _current = default;
        }
        public RandomEnumerator(IEnumerable<T> values, ref Unity.Mathematics.Random rand)
        {
            this._values = NPUArray<T>.getWithoutTracking();
            this._values.AddRange(values);

            this._remainingCount = _values.Count;
            this._rand = rand;

            _isDisposable = true;
            _current = default;
        }
        public readonly T Current => _current;

        readonly object IEnumerator.Current => _current;

        public readonly void Dispose()
        {
            if (_isDisposable)
                _values.Dispose();
        }

        public bool MoveNext()
        {
            if (_remainingCount > 0)
            {
                _remainingCount--;
                var randIndex = _rand.NextInt(_remainingCount);
                _current = _values[randIndex];
                (_values[randIndex], _values[_remainingCount]) = (_values[_remainingCount], _values[randIndex]);
                return true;
            }
            return false;
        }

        public void Reset()
        {
            _remainingCount = _values.Count;
        }

        public readonly RandomEnumerator<T> GetEnumerator()
        {
            return this;
        }
    }
}

