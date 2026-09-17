using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Nextension
{
    public class NList<T> : IList<T>, IReadOnlyList<T>
    {
        public NList()
        {
            i_Items = Array.Empty<T>();
        }

        public NList(int capacity)
        {
            i_Items = new T[capacity];
        }

        public NList(IEnumerable<T> collection)
        {
            i_Items = collection.ToArray();
            i_Count = i_Items.Length;
        }

        public NList(ReadOnlySpan<T> span)
        {
            i_Items = span.ToArray();
            i_Count = i_Items.Length;
        }

        [NonSerialized] internal T[] i_Items;

        [NonSerialized] internal int i_Count;

        public int Capacity => i_Items.Length;

        public int Count => i_Count;

        public bool IsReadOnly => false;

        public T this[int index]
        {
            get => (uint)index < i_Count ? i_Items[index] : throw new IndexOutOfRangeException();
            set
            {
                if ((uint)index >= i_Count) throw new IndexOutOfRangeException();
                i_Items[index] = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetAtWithoutChecks(int index)
        {
            return i_Items[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetAtWithoutChecks(uint index)
        {
            return i_Items[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetWithoutChecks(int index, T value)
        {
            i_Items[index] = value;
        }

        public void EnsureCapacity(int capacity)
        {
            var oldCapacity = Capacity;
            if (oldCapacity < capacity)
            {
                var newItems = new T[capacity];
                Array.Copy(i_Items, newItems, i_Count);
                i_Items = newItems;
            }
        }

        public Span<T> AsSpan()
        {
            return new Span<T>(i_Items, 0, i_Count);
        }

        public Memory<T> AsMemory()
        {
            return new Memory<T>(i_Items, 0, i_Count);
        }

        public Span<T> AsSpan(int start, int length)
        {
            return new Span<T>(i_Items, start, length);
        }

        public void CopyFrom<TCollection>(TCollection collection) where TCollection : IEnumerable<T>
        {
            if (collection is ICollection<T> collection2)
            {
                var cCount = collection2.Count;
                EnsureCapacity(cCount);
                collection2.CopyTo(i_Items, 0);
                i_Count = cCount;
                Array.Clear(i_Items, i_Count, i_Items.Length - i_Count);
            }
            else
            {
                i_Count = 0;
                foreach (var item in collection)
                {
                    Add(item);
                }
                Array.Clear(i_Items, i_Count, i_Items.Length - i_Count);
            }
        }

        public void CopyFrom(Span<T> span)
        {
            EnsureCapacity(span.Length);
            span.CopyTo(i_Items.AsSpan());
            i_Count = span.Length;
        }

        public void Add(T item)
        {
            if (i_Count == i_Items.Length)
            {
                var newItems = new T[i_Count == 0 ? 4 : i_Count << 1];
                Array.Copy(i_Items, newItems, i_Count);
                i_Items = newItems;
            }
            i_Items[i_Count++] = item;
        }

        public void AddRange<TCollection>(TCollection collection) where TCollection : IEnumerable<T>
        {
            InsertRange(i_Count, collection);
        }

        public void AddRange(ReadOnlySpan<T> span)
        {
            InsertRangeWithoutChecks(i_Count, span);
        }

        public void Clear()
        {
            if (i_Count > 0)
            {
                Array.Clear(i_Items, 0, i_Count);
                i_Count = 0;
            }
        }

        public bool Contains(T item)
        {
            for (int i = 0; i < i_Count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(item, i_Items[i]))
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (i_Count > 0)
            {
                Array.Copy(i_Items, 0, array, arrayIndex, i_Count);
            }
        }

        public bool Remove(T item)
        {
            for (int i = 0; i < i_Count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(item, i_Items[i]))
                {
                    RemoveAtWithoutChecks(i);
                    return true;
                }
            }
            return false;
        }

        public void RemoveAt(int index)
        {
            if ((uint)index >= i_Count)
            {
                throw new IndexOutOfRangeException();
            }
            if (index < --i_Count)
            {
                Array.Copy(i_Items, index + 1, i_Items, index, i_Count - index);
            }
            i_Items[i_Count] = default;
        }

        public void RemoveLast()
        {
            if (i_Count > 0)
            {
                i_Items[--i_Count] = default;
            }
        }

        public void RemoveAtWithoutChecks(int index)
        {
            if (index < --i_Count)
            {
                Array.Copy(i_Items, index + 1, i_Items, index, i_Count - index);
            }
            i_Items[i_Count] = default;
        }

        public void RemoveAtSwapBackWithoutChecks(int index)
        {
            i_Items[index] = i_Items[--i_Count];
            i_Items[i_Count] = default;
        }

        public int IndexOf(T item)
        {
            for (int i = 0; i < i_Count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(item, i_Items[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        public void Insert(int index, T item)
        {
            if (i_Count == i_Items.Length)
            {
                var newItems = new T[i_Count == 0 ? 4 : i_Count << 1];
                Array.Copy(i_Items, newItems, i_Count);
                i_Items = newItems;
            }
            if (index < i_Count)
            {
                Array.Copy(i_Items, index, i_Items, index + 1, i_Count - index);
            }
            i_Items[index] = item;
            i_Count++;
        }

        public void InsertRange<TCollection>(int index, TCollection collection) where TCollection : IEnumerable<T>
        {
            if (index > i_Count)
            {
                throw new IndexOutOfRangeException();
            }

            if (collection is ICollection<T> collection2)
            {
                int count = collection2.Count;
                if (count > 0)
                {
                    EnsureCapacity(this.i_Count + count);
                    if (index < this.i_Count)
                    {
                        Array.Copy(i_Items, index, i_Items, index + count, this.i_Count - index);
                    }

                    if (this == collection2)
                    {
                        Array.Copy(i_Items, 0, i_Items, index, index);
                        Array.Copy(i_Items, index + count, i_Items, index * 2, this.i_Count - index);
                    }
                    else
                    {
                        var array = new NList<T>();
                        array.CopyFrom(collection2);
                        array.AsSpan().CopyTo(i_Items.AsSpan(index));
                    }
                    this.i_Count += count;
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

        public void InsertRangeWithoutChecks<TCollection>(int index, TCollection collection) where TCollection : IEnumerable<T>
        {
            if (collection is ICollection<T> collection2)
            {
                int count = collection2.Count;
                if (count > 0)
                {
                    EnsureCapacity(this.i_Count + count);
                    if (index < this.i_Count)
                    {
                        Array.Copy(i_Items, index, i_Items, index + count, this.i_Count - index);
                    }

                    if (this == collection2)
                    {
                        Array.Copy(i_Items, 0, i_Items, index, index);
                        Array.Copy(i_Items, index + count, i_Items, index * 2, this.i_Count - index);
                    }
                    else
                    {
                        var array = new NList<T>();
                        array.CopyFrom(collection2);
                        array.AsSpan().CopyTo(i_Items.AsSpan(index));
                    }
                    this.i_Count += count;
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

        public void InsertRangeWithoutChecks(int index, ReadOnlySpan<T> span)
        {
            int count = span.Length;
            EnsureCapacity(this.i_Count + count);
            if (index < this.i_Count)
            {
                Array.Copy(i_Items, index, i_Items, index + count, this.i_Count - index);
            }
            span.CopyTo(i_Items.AsSpan(index));
            i_Count += count;
        }

        public void SetCount(int count)
        {
            if (i_Count == count) return;
            if (i_Count < count)
            {
                EnsureCapacity(count);
                i_Count = count;
            }
            else
            {
                Array.Clear(i_Items, count, i_Count - count);
                i_Count = count;
            }
        }

        public void Sort()
        {
            Array.Sort(i_Items, 0, i_Count);
        }

        public void Sort(Comparison<T> comparison)
        {
            SortUtil.quickSort(AsSpan(), comparison);
        }

        public ArrayEnumerator<T> GetEnumerator()
        {
            return new ArrayEnumerator<T>(i_Items, 0, (uint)i_Count);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new ArrayEnumerator<T>(i_Items, 0, (uint)i_Count);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new ArrayEnumerator<T>(i_Items, 0, (uint)i_Count);
        }
    }
}