using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nextension
{
    public struct ArrayEnumerator<T> : IEnumerator<T>
    {
        public ArrayEnumerator(T[] array)
        {
            this.array = array;
            _index = startIndex = 0;
            maxIndex = (uint)array.Length;
            _current = default;
        }
        public ArrayEnumerator(T[] array, uint startIndex)
        {
            this.array = array;
            _index = this.startIndex = startIndex;
            maxIndex = (uint)array.Length;
            _current = default;
        }
        public ArrayEnumerator(T[] array, uint startIndex, uint count)
        {
            this.array = array;
            _index = this.startIndex = startIndex;
            maxIndex = startIndex + count;

            if (maxIndex > array.Length)
            {
                throw new ArgumentOutOfRangeException();
            }
            _current = default;
        }

        internal readonly T[] array;
        internal readonly uint startIndex;
        internal readonly uint maxIndex;

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
        public static unsafe UnsafeArrayEnumerator<T> createFrom<TSrc>(ArrayEnumerator<TSrc> src) where TSrc : unmanaged
        {
            var sizeOfSrc = sizeof(TSrc);
#if UNITY_EDITOR
            var sizeOfT = NUtils.sizeOf<T>();
            if (sizeOfSrc != sizeOfT)
            {
                Debug.LogWarning($"SizeOfSrc is different from SizeOfT ({sizeOfSrc} != {sizeOfT})");
            }
#endif
            fixed (TSrc* srcPtr = &src.array[src.startIndex])
            {
                return new UnsafeArrayEnumerator<T>(srcPtr, src.maxIndex - src.startIndex);
            }
        }
        public static unsafe UnsafeArrayEnumerator<T> createFrom<TSrc>(TSrc[] src, uint startIndex = 0) where TSrc : unmanaged
        {
            return createFrom(new ArrayEnumerator<TSrc>(src, startIndex));
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
