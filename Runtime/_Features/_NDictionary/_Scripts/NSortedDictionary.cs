using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Nextension
{
    [Serializable]
    public sealed class NSortedDictionary<K, V> : AbsNDictionary<K, V> where K : IComparable<K>
    {
        [Serializable]
        internal class DItemSortedList : AbsBListGenericComparable<DItem, K>
        {
            public DItemSortedList() { }
            public DItemSortedList(int capacity) : base(capacity) { }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected override K getCompareKeyFromValue(DItem item)
            {
                return item.key;
            }
        }

        [SerializeField] private DItemSortedList items = new();

        public NSortedDictionary()
        {
            items = new DItemSortedList();
        }
        public NSortedDictionary(int capacity)
        {
            items = new DItemSortedList(capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void sort()
        {
            items.sort();
        }

        public override int Count => items.Count;
        public override bool isHasInvalidKeys()
        {
            if (items.Count == 0)
            {
                return false;
            }
            HashSet<K> existKeys = new();
            for (int i = items.Count - 1; i >= 0; i--)
            {
                var k = items[i].key;
                if (k == null || k.Equals(null))
                {
                    return true;
                }
                else
                {
                    if (existKeys.Contains(k))
                    {
                        return true;
                    }
                    else
                    {
                        existKeys.Add(k);
                    }
                }
            }
            return false;
        }
        public override void removeInvalidItems()
        {
            HashSet<K> existKeys = new();
            for (int i = items.Count - 1; i >= 0; i--)
            {
                var k = items[i].key;
                if (k == null || k.Equals(null))
                {
                    items.removeAt(i);
                }
                else
                {
                    if (existKeys.Contains(k))
                    {
                        items.removeAt(i);
                    }
                    else
                    {
                        existKeys.Add(k);
                    }
                }
            }
        }

        public override void set(K key, V value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("NDictionary.key");
            }
            var index = items.bFindIndex(key);
            if (index < 0)
            {
                var item = new DItem(key, value);
                items.addAndSort(item);
            }
            else
            {
                items[index] = new DItem(key, value);
            }
        }
        public override V get(K key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("NDictionary.key");
            }
            var index = items.bFindIndex(key);
            if (index < 0)
            {
                return default;
            }
            else
            {
                var item = items[index];
                return item.value;
            }
        }

        public override bool containKey(K key)
        {
            var index = items.bFindIndex(key);
            return index >= 0;
        }
        public override bool containValue(V value)
        {
            var index = items.findIndex(item => item.value.equals(value));
            return index >= 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override IEnumerable<DItem> enumerateDItems()
        {
            return items.asEnumerable();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override Span<DItem> asSpan()
        {
            return items.asSpan();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void removeDItemAtSwapBack(int index)
        {
            items.removeAt(index);
        }
    }
}