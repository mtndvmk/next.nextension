using System;
using System.Collections;
using System.Collections.Generic;

namespace Nextension
{
    public struct NPUHSet64<T> : IDisposable, ICollection<T> where T : unmanaged
    {
        public struct Enumerator : IEnumerator<T>
        {
            internal Enumerator(HashSet<long>.Enumerator hashSetEnumerator)
            {
                _enumerator = hashSetEnumerator;
            }
            private HashSet<long>.Enumerator _enumerator;
            public T Current => NConverter.bitConvertDiffSize<long, T>(_enumerator.Current);
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
            if (NUtils.sizeOf<T>() > NUtils.sizeOf<long>())
            {
                throw new Exception("T size must be less than Int64 size");
            }
        }
        public static NPUHSet64<T> get()
        {
            checkSize();
            var newSet64 = new NPUHSet64<T>
            {
                _hashset = NPHSet<long>.get()
            };
            return newSet64;
        }
        public static NPUHSet64<T> getWithoutTracking()
        {
            checkSize();
            var newSet64 = new NPUHSet64<T>
            {
                _hashset = NPHSet<long>.getWithoutTracking()
            };
            return newSet64;
        }

        private NPHSet<long> _hashset;
        public int Count => _hashset.Count;
        public bool IsCreated => _hashset != null;
        public bool IsReadOnly => _hashset.IsReadOnly;

        public bool Add(T item)
        {
            return _hashset.Add(NConverter.bitConvertDiffSize<T, long>(item));
        }
        public void Clear()
        {
            _hashset.Clear();
        }
        public bool Contains(T item)
        {
            return _hashset.Contains(NConverter.bitConvertDiffSize<T, long>(item));
        }
        public void CopyTo(T[] array, int arrayIndex)
        {
            foreach (var item in _hashset)
            {
                array[arrayIndex++] = NConverter.bitConvertDiffSize<long, T>(item);
            }
        }
        public bool Remove(T item)
        {
            return _hashset.Remove(NConverter.bitConvertDiffSize<T, long>(item));
        }
        public void Dispose()
        {
            _hashset.Dispose();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public Enumerator GetEnumerator()
        {
            return new Enumerator(_hashset.GetEnumerator());
        }
        void ICollection<T>.Add(T item)
        {
            Add(item);
        }
    }
}
