using System;
using System.Collections;
using System.Collections.Generic;

namespace Nextension
{
    public struct ArrayEnumerator<T> : IEnumerator<T>
    {
        public ArrayEnumerator(T[] array)
        {
            _array = array;
            _index = _startIndex = 0;
            _maxIndex = (uint)array.Length;
            _current = default;
        }
        public ArrayEnumerator(T[] array, uint startIndex)
        {
            _array = array;
            _index = _startIndex = startIndex;
            _maxIndex = (uint)array.Length;
            _current = default;
        }
        public ArrayEnumerator(T[] array, uint startIndex, uint count)
        {
            _array = array;
            _index = _startIndex = startIndex;
            _maxIndex = startIndex + count;

            if (_maxIndex > array.Length)
            {
                throw new ArgumentOutOfRangeException();
            }
            _current = default;
        }

        private readonly T[] _array;
        private readonly uint _startIndex;
        private readonly uint _maxIndex;

        private uint _index;
        private T _current;

        public T Current => _current;
        object IEnumerator.Current => _current;

        public void Dispose()
        {  
        }

        public bool MoveNext()
        {
            if (_index < _maxIndex)
            {
                _current = _array[_index++];
                return true;
            }
            return false;
        }

        public void Reset()
        {
            _index = _startIndex;
            _current = default;
        }
    }
}
