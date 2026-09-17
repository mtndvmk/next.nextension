using System;
using System.Runtime.CompilerServices;

namespace Nextension
{
    public struct UArrayEnumerator<T> where T : unmanaged
    {
        static UArrayEnumerator()
        {
        }

        public static readonly UArrayEnumerator<T> Empty = new(Array.Empty<byte>(), 0);

        public UArrayEnumerator(byte[] items) : this(items, items.Length)
        {
        }

        public UArrayEnumerator(byte[] items, int byteCount)
        {
            if ((uint)byteCount > (uint)items.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(byteCount));
            }
            _items = items;
            _byteCount = byteCount;
            _byteIndex = 0;
            _current = default;
        }

        private readonly byte[] _items;
        private readonly int _byteCount;
        private int _byteIndex;
        private T _current;

        public readonly T Current => _current;

        public readonly int Count => _byteCount / Unsafe.SizeOf<T>();

        public bool MoveNext()
        {
            if (_byteIndex < _byteCount)
            {
                _current = Unsafe.ReadUnaligned<T>(ref _items[_byteIndex]);
                _byteIndex += Unsafe.SizeOf<T>();
                return true;
            }
            return false;
        }

        public void Reset()
        {
            _byteIndex = 0;
            _current = default;
        }

        public readonly void Dispose()
        {
        }

        public readonly UArrayEnumerator<T> GetEnumerator()
        {
            return this;
        }
    }
}
