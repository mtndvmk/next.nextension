using System;
using System.Collections.Generic;
using System.Linq;

namespace Nextension
{
    public abstract class NDictionary
    {
        public abstract Type KeyType { get; }
        public abstract Type ValueType { get; }
        public abstract int Count { get; }
        public abstract void removeInvalidItems();
        public abstract bool isHasInvalidKeys();
    }
    public abstract class AbsNDictionary<K, V> : NDictionary
    {
        [Serializable]
        internal class DItem
        {
            public K key;
            public V value;
            internal DItem(K key, V value)
            {
                this.key = key;
                this.value = value;
            }
            internal (K key, V value) asTuple()
            {
                return (key, value);
            }
        }

        public V this[K key]
        {
            get => get(key);
            set => set(key, value);
        }

        public Dictionary<K, V> toDictionary()
        {
            var dict = new Dictionary<K, V>();
            foreach (var i in enumerateDItems())
            {
                dict.TryAdd(i.key, i.value);
            }
            return dict;
        }

        public sealed override Type KeyType => typeof(K);
        public sealed override Type ValueType => typeof(V);
        public override bool isHasInvalidKeys()
        {
            var items = enumerateDItems();
            HashSet<K> existKeys = new();
            foreach (var item in items)
            {
                var k = item.key;
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
            HashSet<K> existKeys = new HashSet<K>();
            var items = enumerateDItems().ToArray();
            for (int i = items.Length - 1; i >= 0; i--)
            {
                var k = items[i].key;
                if (k == null || k.Equals(null))
                {
                    removeDItemAt(i);
                }
                else
                {
                    if (existKeys.Contains(k))
                    {
                        removeDItemAt(i);
                    }
                    else
                    {
                        existKeys.Add(k);
                    }
                }
            }
        }

        public IEnumerable<V> Values
        {
            get
            {
                foreach (var item in enumerateDItems())
                {
                    yield return item.value;
                }
            }
        }
        public IEnumerable<K> Keys
        {
            get
            {
                foreach (var item in enumerateDItems())
                {
                    yield return item.key;
                }
            }
        }
        public IEnumerable<(K key, V value)> enumerateTupleValues()
        {
            var dItems = enumerateDItems();
            foreach (var item in dItems)
            {
                yield return item.asTuple();
            }
        }
        public void set(IDictionary<K, V> dictionary)
        {
            foreach (var item in dictionary)
            {
                set(item.Key, item.Value);
            }
        }

        public virtual bool containValue(V value)
        {
            foreach (var item in enumerateDItems())
            {
                if (item.value.equals(value))
                {
                    return true;
                }
            }
            return false;
        }
        public virtual bool containKey(K key)
        {
            foreach (var item in enumerateDItems())
            {
                if (item.key.equals(key))
                {
                    return true;
                }
            }
            return false;
        }

        public abstract void set(K key, V value);
        public abstract V get(K key);
        internal abstract IEnumerable<DItem> enumerateDItems();
        protected abstract void removeDItemAt(int index);
    }
}