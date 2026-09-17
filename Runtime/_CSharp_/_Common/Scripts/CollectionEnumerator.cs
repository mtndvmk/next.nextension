using System;
using System.Collections;
using System.Collections.Generic;

namespace Nextension
{
    public unsafe struct UnsafeArrayEnumerator<T> : IEnumerator<T> where T : unmanaged
    {
        public UnsafeArrayEnumerator(void* array, uint itemCount)
        {
            this.array = (T*)array;
            this.itemCount = itemCount;

            _current = default;
            _index = startIndex = 0;
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
}

