using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nextension
{
    public static class NDictionaryExtension
    {
        public static NDictionary<K, V> toNDictionary<K, V>(this IDictionary<K, V> dict)
        {
            var ndict = new NDictionary<K, V>();
            ndict.set(dict);
            return ndict;
        }
    }
    public abstract class NDictionary
    {
        public abstract Type KeyType { get; }
        public abstract Type ValueType { get; }
        public abstract int Count { get; }
        public abstract void removeInvalidKeyItems();
        public abstract bool isHasInvalidKeys();
    }
    [Serializable]
    public sealed class NDictionary<K, V> : NDictionary
    {
        [Serializable]
        public class DItem
        {
            public K key;
            public V value;
            internal DItem(K key, V value)
            {
                this.key = key;
                this.value = value;
            }
            internal DItem clone()
            {
                return new DItem(key, value);
            }
        }

        [SerializeField] private List<DItem> items = new List<DItem>();

        public override int Count => items.Count;
        public void set(K key, V value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("NDictionary.key");
            }
            var index = items.FindIndex(item => key.Equals(item.key));
            if (index < 0)
            {
                var item = new DItem(key, value);
                items.Add(item);
            }
            else
            {
                items[index].value = value;
            }
        }
        public V get(K key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("NDictionary.key");
            }
            var index = items.FindIndex(item => key.Equals(item.key));
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
        public void set(IDictionary<K, V> dict)
        {
            foreach (var k in dict.Keys)
            {
                set(k, dict[k]);
            }
        }

        public bool containKey(K key)
        {
            var index = items.FindIndex(item => key.Equals(item.key));
            return index >= 0;
        }
        public bool containValue(V value)
        {
            var index = items.FindIndex(item => value.Equals(item.value));
            return index >= 0;
        }

        public DItem find(Predicate<DItem> predicate)
        {
            var index = items.FindIndex(item => predicate(item));
            if (index < 0)
            {
                return default;
            }
            else
            {
                var item = items[index].clone();
                return item;
            }
        }

        public IEnumerable<V> Values
        {
            get
            {
                foreach (var item in items)
                {
                    yield return item.value;
                }
            }
        }
        public IEnumerable<K> Keys
        {
            get
            {
                foreach (var item in items)
                {
                    yield return item.key;
                }
            }
        }
        public IEnumerable<(K key, V value)> enumerateTupleValues()
        {
            foreach (var i in items)
            {
                yield return (i.key, i.value);
            }
        }

        public V this[K key]
        {
            get => get(key);
            set => set(key, value);
        }

        public override bool isHasInvalidKeys()
        {
            if (items.Count == 0)
            {
                return false;
            }
            HashSet<K> existKeys = new HashSet<K>();
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
        public override void removeInvalidKeyItems()
        {
            HashSet<K> existKeys = new HashSet<K>();
            for (int i = items.Count - 1; i >= 0; i--)
            {
                var k = items[i].key;
                if (k == null || k.Equals(null))
                {
                    items.RemoveAt(i);
                }
                else
                {
                    if (existKeys.Contains(k))
                    {
                        items.RemoveAt(i);
                    }
                    else
                    {
                        existKeys.Add(k);
                    }
                }
            }
        }
        public override Type KeyType => typeof(K);
        public override Type ValueType => typeof(V);

        public Dictionary<K, V> toDictionary()
        {
            var dict = new Dictionary<K, V>();
            foreach (var i in items)
            {
                dict.TryAdd(i.key, i.value);
            }
            return dict;
        }
    }
}