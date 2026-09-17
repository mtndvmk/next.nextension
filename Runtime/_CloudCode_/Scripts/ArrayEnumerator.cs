using System;
using System.Collections;
using System.Collections.Generic;

namespace Nextension
{
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
}

