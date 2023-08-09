using System;
using System.Collections.Generic;
using System.Linq;
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
    public interface IBList<V> : IBList
    {
        public V this[int index] { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="V">Value Type of List</typeparam>
    /// <typeparam name="K">Key Type to compare Value in List</typeparam>
    public abstract class AbsBList<V, K> : IBList<V>
    {
        protected abstract int compareKey(K k1, K k2);
        private int exeCompareKey(K k1, K k2)
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
        private int exeCompareValue(V a, V b)
        {
            return exeCompareKey(getCompareKeyFromValue(a), getCompareKeyFromValue(b));
        }
        [SerializeField] protected List<V> _list;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract K getCompareKeyFromValue(V item);

        public AbsBList()
        {
            _list = new List<V>();
        }
        public AbsBList(IEnumerable<V> collection)
        {
            _list = new List<V>(collection);
            sort();
        }

        public int Count => _list.Count;

        public V this[int index]
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

        public void sort()
        {
            _list.Sort(exeCompareValue);
        }
        public V bFind(K searchKey)
        {
            var index = bFindIndex(searchKey);
            if (index >= 0)
            {
                return _list[index];
            }
            return default;
        }
        public int bFindIndex(K searchKey)
        {
            int listCount = _list.Count;
            if (listCount == 0)
            {
                return -1;
            }

            int start = 0;
            int end = listCount - 1;
            int mid;
            int compareValue;

            while (start < end)
            {
                mid = (start + end) / 2;
                compareValue = exeCompareKey(searchKey, getCompareKeyFromValue(_list[mid]));
                if (compareValue == 0)
                {
                    return mid;
                }
                else if (compareValue > 0)
                {
                    start = mid + 1;
                }
                else
                {
                    end = mid - 1;
                }
            }

            compareValue = exeCompareKey(searchKey, getCompareKeyFromValue(_list[start]));
            if (compareValue == 0)
            {
                return start;
            }
            else
            {
                return -1;
            }
        }
        public int[] bFindIndexs(K searchKey)
        {
            List<int> indexs = new List<int>();
            var fIndex = bFindIndex(searchKey);
            for (int i = fIndex; i >= 0; i--)
            {
                if (exeCompareKey(getCompareKeyFromValue(_list[i]), searchKey) == 0)
                {
                    indexs.Add(i);
                }
                else
                {
                    break;
                }
            }

            indexs.Reverse();

            for (int i = fIndex + 1; i < _list.Count; ++i)
            {
                if (exeCompareKey(getCompareKeyFromValue(_list[i]), searchKey) == 0)
                {
                    indexs.Add(i);
                }
                else
                {
                    break;
                }
            }
            return indexs.ToArray();
        }
        public int findIndex(Predicate<V> predicate)
        {
            return _list.FindIndex(predicate);
        }
        public int bTryGetValue(K searchKey, out V item)
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
        public int bIndexOf(V item)
        {
            var indexs = bFindIndexs(getCompareKeyFromValue(item));
            foreach (int i in indexs)
            {
                if (item.Equals(_list[i]))
                {
                    return i;
                }
            }
            return -1;
        }
        public bool bContains(V item)
        {
            return bIndexOf(item) >= 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void add(V item)
        {
            _list.Add(item);
        }

        /// <summary>
        /// return false if item already exists in the list, otherwise return true
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool addAndSortIfNotExist(V item)
        {
            if (bContains(item))
            {
                return false;
            }
            addAndSort(item);
            return true;
        }
        public int addAndSort(V item)
        {
            int listCount = _list.Count;
            K searchKey = getCompareKeyFromValue(item);
            switch (listCount)
            {
                case 0:
                    _list.Insert(0, item);
                    return 0;
                case 1:
                    if (exeCompareKey(searchKey, getCompareKeyFromValue(_list[0])) >= 0)
                    {
                        _list.Add(item);
                        return listCount;
                    }
                    else
                    {
                        _list.Insert(0, item);
                        return 0;
                    }
            }

            if (exeCompareKey(searchKey, getCompareKeyFromValue(_list[0])) < 0)
            {
                _list.Insert(0, item);
                return 0;
            }
            if (exeCompareKey(searchKey, getCompareKeyFromValue(_list[listCount - 1])) >= 0)
            {
                _list.Add(item);
                return listCount;
            }

            int start = 0;
            int end = listCount - 1;
            int mid;
            int midplus1;
            int compareValue;
            int compareValue1;

            while (start < end)
            {
                mid = (start + end) >> 1;
                midplus1 = mid + 1;
                compareValue = exeCompareKey(searchKey, getCompareKeyFromValue(_list[mid]));
                switch (compareValue)
                {
                    case 0:
                        _list.Insert(midplus1, item);
                        return midplus1;
                    case 1:
                        compareValue1 = exeCompareKey(searchKey, getCompareKeyFromValue(_list[midplus1]));
                        switch (compareValue1)
                        {
                            case 0:
                            case -1:
                                _list.Insert(midplus1, item);
                                return midplus1;
                            default:
                                start = midplus1;
                                break;
                        }
                        break;
                    default:
                        end = mid;
                        break;
                }
            }

            _list.Insert(start + 1, item);
            return start + 1;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void addRange(ICollection<V> collection)
        {
            _list.AddRange(collection);
        }
        public void addRangeAndSort(ICollection<V> collection)
        {
            addRange(collection);
            sort();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void clear()
        {
            _list.Clear();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void copyTo(V[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }
        public bool remove(K key)
        {
            var index = bFindIndex(key);
            if (index >= 0)
            {
                _list.RemoveAt(index);
                return true;
            }
            return false;
        }
        public int removeAll(K key)
        {
            var indexs = bFindIndexs(key);
            for (int i = indexs.Length - 1; i >= 0; i--)
            {
                _list.RemoveAt(i);
            }
            return indexs.Length;
        }
        public void removeRange(int index, int count)
        {
            _list.RemoveRange(index, count);
        }
        public bool remove(V item)
        {
            var indexs = bFindIndexs(getCompareKeyFromValue(item));
            for (int i = indexs.Length - 1; i >= 0; i--)
            {
                if (item.Equals(_list[i]))
                {
                    _list.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void removeAt(int index)
        {
            _list.RemoveAt(index);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<V> toList()
        {
            return new List<V>(_list);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public V[] toArray()
        {
            return _list.ToArray();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<V> asEnumerable()
        {
            return _list.AsEnumerable();
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="V">Value Type of List</typeparam>
    /// <typeparam name="K">Key Type to compare Value in List</typeparam>
    public abstract class AbsBListCompareableK<V, K> : AbsBList<V, K> where K : IComparable<K>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected sealed override int compareKey(K k1, K k2)
        {
            return k1.CompareTo(k2);
        }
        public AbsBListCompareableK() : base() { }
        public AbsBListCompareableK(IEnumerable<V> collection) : base(collection) { }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="V">Value Type of List</typeparam>
    /// <typeparam name="K">Key Type to compare Value in List</typeparam>
    public abstract class AbsBListCompareable<V, K> : AbsBList<V, K> where K : IComparable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected sealed override int compareKey(K k1, K k2)
        {
            return k1.CompareTo(k2);
        }
        public AbsBListCompareable() : base() { }
        public AbsBListCompareable(IEnumerable<V> collection) : base(collection) { }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="V">Value Type of List</typeparam>
    /// <typeparam name="K">Key Type to compare Value in List</typeparam>
    public sealed class NBList<V, K> : AbsBListCompareableK<V, K> where K : IComparable<K>
    {
        private Func<V, K> _getCompareKeyFromValueFunc;
        protected override K getCompareKeyFromValue(V item)
        {
            return _getCompareKeyFromValueFunc(item);
        }

        public NBList(Func<V, K> getCompareKeyFromValueFunc) : base()
        {
            _getCompareKeyFromValueFunc = getCompareKeyFromValueFunc;
        }
        public NBList(IEnumerable<V> collection, Func<V, K> getCompareKeyFromValueFunc) : base(collection)
        {
            _getCompareKeyFromValueFunc = getCompareKeyFromValueFunc;
        }
    }

    [Serializable]
    public sealed class NBListUseHashCode<V> : AbsBListCompareableK<V, int>
    {
        protected override int getCompareKeyFromValue(V item)
        {
            return item.GetHashCode();
        }
    }
}
