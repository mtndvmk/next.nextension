using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Nextension
{
    public class NArray<T> : IList<T>
    {
        private readonly static T[] _emptyArray = new T[0];
        public NArray()
        {
            _items = _emptyArray;
        }
        public NArray(int capacity)
        {
            _items = new T[capacity];
        }
        public NArray(IEnumerable<T> collection)
        {
            _items = collection.ToArray();
            _count = _items.Length;
        }
        private T[] _items;
        private int _count;

        public int Capacity => _items.Length;
        public int Count => _count;

        public bool IsReadOnly => false;

        public T this[int index]
        {
            get => index < _count ? _items[index] : throw new IndexOutOfRangeException();
            set
            {
                if (index >= _count) throw new IndexOutOfRangeException();
                _items[index] = value;
            }
        }

        public T getWithoutChecks(int index)
        {
            return _items[index];
        }
        public void setWithoutChecks(int index, T value)
        {
            _items[index] = value;
        }
        public void ensureCapacity(int capacity)
        {
            var oldCapacity = Capacity;
            if (oldCapacity < capacity)
            {
                var newItems = new T[capacity];
                Array.Copy(_items, newItems, _count);
                _items = newItems;
            }
        }
        public Span<T> asSpan()
        {
            return new Span<T>(_items, 0, _count);
        }
        public void copyFrom(IEnumerable<T> collection)
        {
            if (collection is ICollection<T> collection2)
            {
                copyFrom(collection2);
            }
            else
            {
                _count = 0;
                foreach (var item in collection)
                {
                    Add(item);
                }
                Array.Clear(_items, _count, _items.Length - _count);
            }
        }
        public void copyFrom(ICollection<T> collection)
        {
            var cCount = collection.Count;
            ensureCapacity(cCount);
            collection.CopyTo(_items, 0);
            _count = cCount;
            Array.Clear(_items, _count, _items.Length - _count);
        }
        public void copyFrom(Span<T> span)
        {
            ensureCapacity(span.Length);
            span.CopyTo(_items.AsSpan());
            _count = span.Length;
        }

        public void Add(T item)
        {
            if (_count == _items.Length)
            {
                var newItems = new T[_count == 0 ? 4 : _count << 1];
                Array.Copy(_items, newItems, _count);
                _items = newItems;
            }
            _items[_count++] = item;
        }
        public void AddRange(IEnumerable<T> collection)
        {
            InsertRange(_count, collection);
        }
        public void Clear()
        {
            if (_count > 0)
            {
                Array.Clear(_items, 0, _count);
                _count = 0;
            }
        }
        public bool Contains(T item)
        {
            for (int i = 0; i < _count; i++)
            {
                if (item.equals(_items[i]))
                {
                    return true;
                }
            }
            return false;
        }
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (_count > 0)
            {
                Array.Copy(_items, 0, array, arrayIndex, _count);
            }
        }

        public bool Remove(T item)
        {
            for (int i = 0; i < _count; i++)
            {
                if (item.equals(_items[i]))
                {
                    RemoveAt(i);
                    return true;
                }
            }
            return false;
        }
        public void RemoveAt(int index)
        {
            if (index >= _count)
            {
                throw new IndexOutOfRangeException();
            }
            if (index < --_count)
            {
                Array.Copy(_items, index + 1, _items, index, _count - index);
            }
            _items[_count] = default;
        }

        public int IndexOf(T item)
        {
            for (int i = 0; i < _count; i++)
            {
                if (item.equals(_items[i]))
                {
                    return i;
                }
            }
            return -1;
        }
        public void Insert(int index, T item)
        {
            if (_count == _items.Length)
            {
                var newItems = new T[_count == 0 ? 4 : _count << 1];
                Array.Copy(_items, newItems, _count);
                _items = newItems;
            }
            if (index < _count)
            {
                Array.Copy(_items, index, _items, index + 1, _count - index);
            }
            _items[index] = item;
            _count++;
        }
        public void InsertRange(int index, IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException();
            }

            if (index > _count)
            {
                throw new IndexOutOfRangeException();
            }

            if (collection is ICollection<T> collection2)
            {
                int count = collection2.Count;
                if (count > 0)
                {
                    ensureCapacity(_count + count);
                    if (index < _count)
                    {
                        Array.Copy(_items, index, _items, index + count, _count - index);
                    }

                    if (this == collection2)
                    {
                        Array.Copy(_items, 0, _items, index, index);
                        Array.Copy(_items, index + count, _items, index * 2, _count - index);
                    }
                    else
                    {
                        using var poolArray = NPArray<T>.get();
                        poolArray.copyFrom(collection2);
                        poolArray.asSpan().CopyTo(_items.AsSpan(index));
                    }
                    _count += count;
                }
            }
            else
            {
                foreach (var item in collection)
                {
                    Insert(index++, item);
                }
            }
        }

        public ArrayEnumerator<T> GetEnumerator()
        {
            return new ArrayEnumerator<T>(_items, 0, (uint)_count);
        }
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new ArrayEnumerator<T>(_items, 0, (uint)_count);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new ArrayEnumerator<T>(_items, 0, (uint)_count);
        }
    }
}