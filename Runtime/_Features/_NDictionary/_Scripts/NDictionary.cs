using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Nextension
{
    [Serializable]
    public sealed class NDictionary<K, V> : AbsNDictionary<K, V>
    {
        public NDictionary()
        {
            items = new List<DItem>();
        }
        public NDictionary(int capacity)
        {
            items = new List<DItem>(capacity);
        }

        [SerializeField] private List<DItem> items = new List<DItem>();

        public override int Count => items.Count;
        public override void set(K key, V value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("NDictionary.key");
            }
            var index = items.FindIndex(item => key.equals(item.key));
            if (index < 0)
            {
                var item = new DItem(key, value);
                items.Add(item);
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
            var index = items.FindIndex(item => key.equals(item.key));
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
        public bool remove(K key)
        {
            var index = items.FindIndex(item => key.equals(item.key));
            if (index >= 0)
            {
                items.removeAtSwapBack(index);
                return true;
            }
            return false;
        }
        public bool tryGetValue(K key, out V value)
        {
            var index = items.FindIndex(item => key.equals(item.key));
            if (index >= 0)
            {
                value = items[index].value;
                return true;
            }
            value = default;
            return false;
        }
        public bool tryTakeAndRemove(K key, out V value)
        {
            var index = items.FindIndex(item => key.equals(item.key));
            if (index >= 0)
            {
                value = items[index].value;
                items.removeAtSwapBack(index);
                return true;
            }

            value = default;
            return false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override IEnumerable<DItem> enumerateDItems()
        {
            return items;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override Span<DItem> asSpan()
        {
            return items.asSpan();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void removeDItemAtSwapBack(int index)
        {
            items.removeAtSwapBack(index);
        }
    }
}