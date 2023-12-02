using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Nextension
{
    public interface IBList
    {
        public void sort();
        public int Count { get; }
        public void removeAt(int index);
    }
    public interface IBList<TValue> : IBList
    {
        public TValue this[int index] { get; set; }
        public int bIndexOf(TValue item);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TValue">Value Type of List</typeparam>
    /// <typeparam name="TCompareKey">Key Type to compare Value in List</typeparam>
    public abstract class AbsBList<TValue, TCompareKey> : IBList<TValue>, IEnumerable<TValue>
    {
        protected abstract int compareKey(TCompareKey k1, TCompareKey k2);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool equalValue(TValue item1, TValue item2)
        {
            return item1.equals(item2);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int exeCompareKey(TCompareKey k1, TCompareKey k2)
        {
            try
            {
                return compareKey(k1, k2);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
                return -1;
            }
        }
        private int exeCompareValue(TValue a, TValue b)
        {
            return exeCompareKey(getCompareKeyFromValue(a), getCompareKeyFromValue(b));
        }
        private int exeFindIndexOf(TValue value, int centerIndex)
        {
            int listCount = _list.Count;
            var span = _list.asSpan();
            var searchKey = getCompareKeyFromValue(value);
            for (int i = centerIndex; i >= 0; i--)
            {
                if (exeCompareKey(searchKey, getCompareKeyFromValue(span[i])) != 0)
                {
                    break;
                }
                if (equalValue(span[i], value))
                {
                    return i;
                }
            }
            for (int i = centerIndex + 1; i < listCount; i++)
            {
                if (exeCompareKey(searchKey, getCompareKeyFromValue(span[i])) != 0)
                {
                    break;
                }
                if (equalValue(span[i], value))
                {
                    return i;
                }
            }
            return -1;
        }

        [SerializeField] protected List<TValue> _list;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract TCompareKey getCompareKeyFromValue(TValue item);

        public AbsBList()
        {
            _list = new List<TValue>();
        }
        public AbsBList(int capacity)
        {
            _list = new List<TValue>(capacity);
        }
        public AbsBList(IEnumerable<TValue> collection)
        {
            _list = new List<TValue>(collection);
            sort();
        }

        public int Count => _list.Count;
        public TValue this[int index]
        {
            get => _list[index];
            set
            {
                var oldKey = getCompareKeyFromValue(_list[index]);
                var newKey = getCompareKeyFromValue(value);
                _list[index] = value;
                if (exeCompareKey(oldKey, newKey) != 0)
                {
                    sort();
                }
            }
        }

        #region Query from TKey

        public TValue bFind(TCompareKey searchKey)
        {
            var index = bFindIndex(searchKey);
            if (index >= 0)
            {
                return _list[index];
            }
            throw new KeyNotFoundException();
        }
        public bool bContains(TCompareKey searchKey)
        {
            return bFindIndex(searchKey) >= 0;
        }
        public int bFindIndex(TCompareKey searchKey)
        {
            int listCount = _list.Count;
            if (listCount == 0)
            {
                return -1;
            }

            int startIndex = 0;
            int endIndex = listCount - 1;
            int midIndex;
            int compareResult;
            var span = _list.asSpan();

            while (startIndex < endIndex)
            {
                midIndex = (startIndex + endIndex) >> 1;
                compareResult = exeCompareKey(searchKey, getCompareKeyFromValue(span[midIndex]));
                if (compareResult == 0)
                {
                    return midIndex;
                }
                else if (compareResult > 0)
                {
                    startIndex = midIndex + 1;
                }
                else
                {
                    endIndex = midIndex - 1;
                }
            }

            compareResult = exeCompareKey(searchKey, getCompareKeyFromValue(span[startIndex]));
            if (compareResult == 0)
            {
                return startIndex;
            }
            else
            {
                return -1;
            }
        }
        public int bFindInsertIndex(TCompareKey searchKey)
        {
            var span = _list.asSpan();
            int listCount = span.Length;
            switch (listCount)
            {
                case 0:
                    return 0;
                case 1:
                    if (exeCompareKey(searchKey, getCompareKeyFromValue(span[0])) >= 0)
                    {
                        return listCount;
                    }
                    else
                    {
                        return 0;
                    }
            }

            if (exeCompareKey(searchKey, getCompareKeyFromValue(span[0])) < 0)
            {
                return 0;
            }
            if (exeCompareKey(searchKey, getCompareKeyFromValue(span[^1])) >= 0)
            {
                return listCount;
            }

            int startIndex = 0;
            int endIndex = listCount - 1;
            int midIndex;
            int midIndexPlus1;
            int compareResult0;
            int compareResult1;

            while (startIndex < endIndex)
            {
                midIndex = (startIndex + endIndex) >> 1;
                midIndexPlus1 = midIndex + 1;
                compareResult0 = exeCompareKey(searchKey, getCompareKeyFromValue(span[midIndex]));
                switch (compareResult0)
                {
                    case 0:
                        return midIndexPlus1;
                    case 1:
                        compareResult1 = exeCompareKey(searchKey, getCompareKeyFromValue(span[midIndexPlus1]));
                        switch (compareResult1)
                        {
                            case 0:
                            case -1:
                                return midIndexPlus1;
                            default:
                                startIndex = midIndexPlus1;
                                break;
                        }
                        break;
                    default:
                        endIndex = midIndex;
                        break;
                }
            }
            return ++startIndex;
        }
        public Span<int> bFindIndices(TCompareKey searchKey)
        {
            var fIndex = bFindIndex(searchKey);

            if (fIndex == -1)
            {
                return new Span<int>();
            }

            List<int> indices = new();
            var span = _list.asSpan();
            int listCount = span.Length;

            for (int i = fIndex; i >= 0; i--)
            {
                if (exeCompareKey(getCompareKeyFromValue(span[i]), searchKey) == 0)
                {
                    indices.Add(i);
                }
                else
                {
                    break;
                }
            }

            indices.Reverse();

            for (int i = fIndex + 1; i < listCount; ++i)
            {
                if (exeCompareKey(getCompareKeyFromValue(span[i]), searchKey) == 0)
                {
                    indices.Add(i);
                }
                else
                {
                    break;
                }
            }
            return indices.asSpan();
        }
        /// <summary>
        /// Enumerable indices without sort
        /// </summary>
        public IEnumerable<int> bFindIndicesWithoutSort(TCompareKey searchKey)
        {
            var fIndex = bFindIndex(searchKey);
            if (fIndex == -1)
            {
                yield break;
            }
            int listCount = _list.Count;
            for (int i = fIndex; i >= 0; i--)
            {
                if (exeCompareKey(getCompareKeyFromValue(_list[i]), searchKey) == 0)
                {
                    yield return i;
                }
                else
                {
                    break;
                }
            }
            for (int i = fIndex + 1; i < listCount; ++i)
            {
                if (exeCompareKey(getCompareKeyFromValue(_list[i]), searchKey) == 0)
                {
                    yield return i;
                }
                else
                {
                    break;
                }
            }
        }

        public int bTryGetValue(TCompareKey searchKey, out TValue item)
        {
            var index = bFindIndex(searchKey);
            if (index >= 0)
            {
                item = _list[index];
            }
            else
            {
                item = default;
            }
            return index;
        }

        public bool removeKey(TCompareKey key)
        {
            var index = bFindIndex(key);
            if (index >= 0)
            {
                _list.RemoveAt(index);
                return true;
            }
            return false;
        }
        public int removeAllKey(TCompareKey key)
        {
            var indices = bFindIndices(key);
            for (int i = indices.Length - 1; i >= 0; i--)
            {
                _list.RemoveAt(indices[i]);
            }
            return indices.Length;
        }

        #endregion

        #region Query from TValue

        /// <summary>
        /// Return true if item already exists, otherwise return false
        /// </summary>
        public bool bFindInsertIndex(TValue value, out int insertIndex)
        {
            int listCount = _list.Count;
            var span = _list.asSpan();
            var searchKey = getCompareKeyFromValue(value);
            switch (listCount)
            {
                case 0:
                    insertIndex = 0;
                    return false;
                case 1:
                    var compareFist = exeCompareKey(searchKey, getCompareKeyFromValue(span[0]));
                    if (compareFist >= 0)
                    {
                        insertIndex = listCount;
                        return equalValue(span[0], value);
                    }
                    else
                    {
                        insertIndex = 0;
                        return false;
                    }
            }

            if (exeCompareKey(searchKey, getCompareKeyFromValue(span[0])) < 0)
            {
                insertIndex = 0;
                return false;
            }
            if (exeCompareKey(searchKey, getCompareKeyFromValue(span[^1])) >= 0)
            {
                insertIndex = listCount;
                return equalValue(span[^1], value);
            }

            int startIndex = 0;
            int endIndex = listCount - 1;
            int midIndex;
            int midIndexPlus1;
            int compareResult0;
            int compareResult1;

            while (startIndex < endIndex)
            {
                midIndex = (startIndex + endIndex) >> 1;
                midIndexPlus1 = midIndex + 1;
                compareResult0 = exeCompareKey(searchKey, getCompareKeyFromValue(span[midIndex]));
                switch (compareResult0)
                {
                    case 0:
                        insertIndex = midIndexPlus1;
                        return exeFindIndexOf(value, midIndexPlus1) >= 0;
                    case 1:
                        compareResult1 = exeCompareKey(searchKey, getCompareKeyFromValue(span[midIndexPlus1]));
                        switch (compareResult1)
                        {
                            case 0:
                                insertIndex = midIndexPlus1;
                                return exeFindIndexOf(value, midIndexPlus1) >= 0;
                            case -1:
                                insertIndex = midIndexPlus1;
                                return false;
                            default:
                                startIndex = midIndexPlus1;
                                break;
                        }
                        break;
                    default:
                        endIndex = midIndex;
                        break;
                }
            }
            insertIndex = ++startIndex;
            return false;
        }
        public int bIndexOf(TValue item)
        {
            var index = bFindIndex(getCompareKeyFromValue(item));
            if (index >= 0)
            {
                return exeFindIndexOf(item, index);
            }
            return -1;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool bContains(TValue item)
        {
            return bIndexOf(item) >= 0;
        }

        /// <summary>
        /// Returns sorted index list
        /// </summary>
        public int findIndex(Predicate<TValue> predicate)
        {
            return _list.FindIndex(predicate);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void add(TValue item)
        {
            _list.Add(item);
        }
        /// <summary>
        /// return false if item already exists in the list, otherwise return true
        /// </summary>
        public bool addAndSortIfNotExist(TValue item)
        {
            if (bFindInsertIndex(item, out var insertIndex))
            {
                _list.Insert(insertIndex, item);
            }
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void insert(int index, TValue item)
        {
            _list.Insert(index, item);
        }
        public int addAndSort(TValue item)
        {
            var insertIndex = bFindInsertIndex(getCompareKeyFromValue(item));
            _list.Insert(insertIndex, item);
            return insertIndex;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void addRange(ICollection<TValue> collection)
        {
            _list.AddRange(collection);
        }
        public void addRangeAndSort(ICollection<TValue> collection)
        {
            addRange(collection);
            sort();
        }

        public bool removeValue(TValue item)
        {
            var index = bIndexOf(item);
            if (index >= 0)
            {
                _list.RemoveAt(index);
                return true;
            }
            return false;
        }
        public int removeAllValue(TValue item)
        {
            int count = 0;
            var indices = bFindIndices(getCompareKeyFromValue(item));
            var span = _list.asSpan();
            for (int i = indices.Length - 1; i >= 0; i--)
            {
                var index = indices[i];
                if (equalValue(item, span[index]))
                {
                    _list.RemoveAt(index);
                    ++count;
                }
            }
            return count;
        }
        #endregion

        #region Others

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void sort()
        {
            _list.Sort(exeCompareValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<TValue> toList()
        {
            return new List<TValue>(_list);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue[] toArray()
        {
            return _list.ToArray();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<TValue> asSpan()
        {
            return _list.asSpan();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void copyTo(TValue[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void removeAt(int index)
        {
            _list.RemoveAt(index);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void removeRange(int index, int count)
        {
            _list.RemoveRange(index, count);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void clear()
        {
            _list.Clear();
        }

        public List<TValue>.Enumerator GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        #endregion
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TValue">Value Type of List</typeparam>
    /// <typeparam name="TCompareKey">Key Type to compare Value in List</typeparam>
    public abstract class AbsBListGenericComparable<TValue, TCompareKey> : AbsBList<TValue, TCompareKey> where TCompareKey : IComparable<TCompareKey>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected sealed override int compareKey(TCompareKey k1, TCompareKey k2)
        {
            return k1.CompareTo(k2);
        }
        public AbsBListGenericComparable() : base() { }
        public AbsBListGenericComparable(int capacity) : base(capacity) { }
        public AbsBListGenericComparable(IEnumerable<TValue> collection) : base(collection) { }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TValue">Value Type of List</typeparam>
    /// <typeparam name="K">Key Type to compare Value in List</typeparam>
    public abstract class AbsBListComparable<TValue, TCompareKey> : AbsBList<TValue, TCompareKey> where TCompareKey : IComparable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected sealed override int compareKey(TCompareKey k1, TCompareKey k2)
        {
            return k1.compareTo(k2);
        }
        public AbsBListComparable() : base() { }
        public AbsBListComparable(int capacity) : base(capacity) { }
        public AbsBListComparable(IEnumerable<TValue> collection) : base(collection) { }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="V">Value Type of List</typeparam>
    /// <typeparam name="K">Key Type to compare Value in List</typeparam>
    public sealed class NBList<TValue, TCompareKey> : AbsBList<TValue, TCompareKey>
    {
        private Func<TValue, TCompareKey> _getCompareKeyFromValueFunc;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override TCompareKey getCompareKeyFromValue(TValue item)
        {
            return _getCompareKeyFromValueFunc(item);
        }

        protected override int compareKey(TCompareKey k1, TCompareKey k2)
        {
            return k1.compareTo(k2);
        }

        public NBList(Func<TValue, TCompareKey> getCompareKeyFromValueFunc) : base()
        {
            _getCompareKeyFromValueFunc = getCompareKeyFromValueFunc;
        }
        public NBList(int capacity, Func<TValue, TCompareKey> getCompareKeyFromValueFunc) : base(capacity)
        {
            _getCompareKeyFromValueFunc = getCompareKeyFromValueFunc;
        }
        public NBList(IEnumerable<TValue> collection, Func<TValue, TCompareKey> getCompareKeyFromValueFunc) : base(collection)
        {
            _getCompareKeyFromValueFunc = getCompareKeyFromValueFunc;
        }
    }

    [Serializable]
    public sealed class NBListCompareHashCode<TValue> : AbsBListGenericComparable<TValue, int>
    {
        public NBListCompareHashCode() : base() { }
        public NBListCompareHashCode(int capacity) : base(capacity) { }
        protected override int getCompareKeyFromValue(TValue item)
        {
            return item.GetHashCode();
        }
    }
}
