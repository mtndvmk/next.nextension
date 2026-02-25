using System;
using System.Collections;
using System.Collections.Generic;

namespace Nextension
{
    public readonly struct NPUHSet32<T> : IDisposable, ICollection<T> where T : unmanaged
    {
        public struct Enumerator : IEnumerator<T>
        {
            internal Enumerator(HashSet<int>.Enumerator hashSetEnumerator)
            {
                _enumerator = hashSetEnumerator;
            }
            private HashSet<int>.Enumerator _enumerator;
            public T Current => NConverter.bitConvertDiffSize<int, T>(_enumerator.Current);
            object IEnumerator.Current => _enumerator.Current;

            public void Dispose()
            {
                _enumerator.Dispose();
            }
            public bool MoveNext()
            {
                return _enumerator.MoveNext();
            }
            public void Reset()
            {
                throw new NotSupportedException();
            }
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private static void checkSize()
        {
            if (NUtils.sizeOf<T>() > NUtils.sizeOf<int>())
            {
                throw new Exception("T size must be less than Int32 size");
            }
        }
        public static NPUHSet32<T> get()
        {
            checkSize();
            var newSet32 = new NPUHSet32<T>(NPHSet<int>.get());
            return newSet32;
        }
        public static NPUHSet32<T> getWithoutTracking()
        {
            checkSize();
            var newSet32 = new NPUHSet32<T>(NPHSet<int>.getWithoutTracking());
            return newSet32;
        }

        private readonly NPHSet<int> _hashset;
        public readonly int Count => _hashset.Count;

        public readonly bool IsCreated => _hashset != null;
        public readonly bool IsReadOnly => _hashset.IsReadOnly;

        private NPUHSet32(NPHSet<int> hashset)
        {
            _hashset = hashset;
        }

        public readonly void stopTracking()
        {
            _hashset.stopTracking();
        }

        public readonly bool Add(T item)
        {
            return _hashset.Add(NConverter.bitConvertDiffSize<T, int>(item));
        }
        public readonly void Clear()
        {
            _hashset.Clear();
        }
        public readonly bool Contains(T item)
        {
            return _hashset.Contains(NConverter.bitConvertDiffSize<T, int>(item));
        }
        public readonly void CopyTo(T[] array, int arrayIndex)
        {
            foreach (var item in _hashset)
            {
                array[arrayIndex++] = NConverter.bitConvertDiffSize<int, T>(item);
            }
        }
        public readonly bool Remove(T item)
        {
            return _hashset.Remove(NConverter.bitConvertDiffSize<T, int>(item));
        }
        public readonly void Dispose()
        {
            _hashset.Dispose();
        }

        readonly IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }
        readonly IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public readonly Enumerator GetEnumerator()
        {
            return new Enumerator(_hashset.GetEnumerator());
        }

        readonly void ICollection<T>.Add(T item)
        {
            Add(item);
        }
    }
}
