using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Nextension
{
    public class SimpleDictionary<TKey, TValue> : ICollection<KeyValuePair<TKey, TValue>>
    {
        private struct Entry
        {
            public int hashCode;

            public int next;

            public TKey key;

            public TValue value;
        }

        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IDisposable
        {
            private readonly Entry[] entries;
            private readonly uint count;
            private uint index;

            private KeyValuePair<TKey, TValue> current;

            public KeyValuePair<TKey, TValue> Current => current;

            //
            // Summary:
            //     Gets the element at the current position of the enumerator.
            //
            // Returns:
            //     The element in the collection at the current position of the enumerator, as an
            //     System.Object.
            //
            // Exceptions:
            //   T:System.InvalidOperationException:
            //     The enumerator is positioned before the first element of the collection or after
            //     the last element.

            object IEnumerator.Current => current;

            internal Enumerator(SimpleDictionary<TKey, TValue> dictionary)
            {
                entries = dictionary.entries;
                count = (uint)dictionary.count;
                index = 0;
                current = default;
            }

            //
            // Summary:
            //     Advances the enumerator to the next element of the System.Collections.Generic.Dictionary`2.
            //
            // Returns:
            //     true if the enumerator was successfully advanced to the next element; false if
            //     the enumerator has passed the end of the collection.
            //
            // Exceptions:
            //   T:System.InvalidOperationException:
            //     The collection was modified after the enumerator was created.

            public bool MoveNext()
            {
                while (index < count)
                {
                    if (entries[index].hashCode >= 0)
                    {
                        current = new KeyValuePair<TKey, TValue>(entries[index].key, entries[index].value);
                        index++;
                        return true;
                    }

                    index++;
                }

                index = count + 1;
                current = default;
                return false;
            }

            //
            // Summary:
            //     Releases all resources used by the System.Collections.Generic.Dictionary`2.Enumerator.

            public void Dispose()
            {
            }

            //
            // Summary:
            //     Sets the enumerator to its initial position, which is before the first element
            //     in the collection.
            //
            // Exceptions:
            //   T:System.InvalidOperationException:
            //     The collection was modified after the enumerator was created.

            void IEnumerator.Reset()
            {
                index = 0;
                current = default;
            }
        }

        private int[] buckets;

        private Entry[] entries;

        private int count;

        private int freeList;

        private int freeCount;

        private readonly IEqualityComparer<TKey> comparer;

        //
        // Summary:
        //     Gets the number of key/value pairs contained in the System.Collections.Generic.Dictionary`2.
        //
        // Returns:
        //     The number of key/value pairs contained in the System.Collections.Generic.Dictionary`2.

        public int Count
        {

            get
            {
                return count - freeCount;
            }
        }

        //
        // Summary:
        //     Gets or sets the value associated with the specified key.
        //
        // Parameters:
        //   key:
        //     The key of the value to get or set.
        //
        // Returns:
        //     The value associated with the specified key. If the specified key is not found,
        //     a get operation throws a System.Collections.Generic.KeyNotFoundException, and
        //     a set operation creates a new element with the specified key.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     key is null.
        //
        //   T:System.Collections.Generic.KeyNotFoundException:
        //     The property is retrieved and key does not exist in the collection.

        public TValue this[TKey key]
        {

            get
            {
                int num = FindEntry(key);
                if (num >= 0)
                {
                    return entries[num].value;
                }

                throw new KeyNotFoundException(nameof(key));
            }

            set
            {
                Insert(key, value, add: false);
            }
        }


        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {

            get
            {
                return false;
            }
        }

        //
        // Summary:
        //     Gets a value that indicates whether access to the System.Collections.ICollection
        //     is synchronized (thread safe).
        //
        // Returns:
        //     true if access to the System.Collections.ICollection is synchronized (thread
        //     safe); otherwise, false. In the default implementation of System.Collections.Generic.Dictionary`2,
        //     this property always returns false.


        //
        // Summary:
        //     Initializes a new instance of the System.Collections.Generic.Dictionary`2 class
        //     that is empty, has the default initial capacity, and uses the default equality
        //     comparer for the key type.

        public SimpleDictionary()
            : this(0, default)
        {
        }

        //
        // Summary:
        //     Initializes a new instance of the System.Collections.Generic.Dictionary`2 class
        //     that is empty, has the specified initial capacity, and uses the default equality
        //     comparer for the key type.
        //
        // Parameters:
        //   capacity:
        //     The initial number of elements that the System.Collections.Generic.Dictionary`2
        //     can contain.
        //
        // Exceptions:
        //   T:System.ArgumentOutOfRangeException:
        //     capacity is less than 0.

        public SimpleDictionary(uint capacity)
            : this(capacity, default)
        {
        }

        //
        // Summary:
        //     Initializes a new instance of the System.Collections.Generic.Dictionary`2 class
        //     that is empty, has the specified initial capacity, and uses the specified System.Collections.Generic.IEqualityComparer`1.
        //
        // Parameters:
        //   capacity:
        //     The initial number of elements that the System.Collections.Generic.Dictionary`2
        //     can contain.
        //
        //   comparer:
        //     The System.Collections.Generic.IEqualityComparer`1 implementation to use when
        //     comparing keys, or null to use the default System.Collections.Generic.EqualityComparer`1
        //     for the type of the key.
        //
        // Exceptions:
        //   T:System.ArgumentOutOfRangeException:
        //     capacity is less than 0.

        public SimpleDictionary(uint capacity, IEqualityComparer<TKey> comparer)
        {
            if (capacity > 0)
            {
                Initialize(capacity);
            }

            this.comparer = comparer ?? EqualityComparer<TKey>.Default;
        }

        //
        // Summary:
        //     Initializes a new instance of the System.Collections.Generic.Dictionary`2 class
        //     that contains elements copied from the specified System.Collections.Generic.IDictionary`2
        //     and uses the default equality comparer for the key type.
        //
        // Parameters:
        //   dictionary:
        //     The System.Collections.Generic.IDictionary`2 whose elements are copied to the
        //     new System.Collections.Generic.Dictionary`2.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     dictionary is null.
        //
        //   T:System.ArgumentException:
        //     dictionary contains one or more duplicate keys.

        public SimpleDictionary(IDictionary<TKey, TValue> dictionary)
            : this(dictionary, default)
        {
        }

        //
        // Summary:
        //     Initializes a new instance of the System.Collections.Generic.Dictionary`2 class
        //     that contains elements copied from the specified System.Collections.Generic.IDictionary`2
        //     and uses the specified System.Collections.Generic.IEqualityComparer`1.
        //
        // Parameters:
        //   dictionary:
        //     The System.Collections.Generic.IDictionary`2 whose elements are copied to the
        //     new System.Collections.Generic.Dictionary`2.
        //
        //   comparer:
        //     The System.Collections.Generic.IEqualityComparer`1 implementation to use when
        //     comparing keys, or null to use the default System.Collections.Generic.EqualityComparer`1
        //     for the type of the key.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     dictionary is null.
        //
        //   T:System.ArgumentException:
        //     dictionary contains one or more duplicate keys.

        public SimpleDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
            : this((uint)(dictionary?.Count ?? 0), comparer)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            foreach (KeyValuePair<TKey, TValue> item in dictionary)
            {
                Add(item.Key, item.Value);
            }
        }

        //
        // Summary:
        //     Adds the specified key and value to the dictionary.
        //
        // Parameters:
        //   key:
        //     The key of the element to add.
        //
        //   value:
        //     The value of the element to add. The value can be null for reference types.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     key is null.
        //
        //   T:System.ArgumentException:
        //     An element with the same key already exists in the System.Collections.Generic.Dictionary`2.

        public void Add(TKey key, TValue value)
        {
            Insert(key, value, add: true);
        }

        public bool tryAdd(TKey key, TValue value)
        {
            checkNullKey(key);

            if (buckets == null)
            {
                Initialize(0);
            }

            int num = comparer.GetHashCode(key) & 0x7FFFFFFF;
            int num2 = num % buckets.Length;
            int num3 = 0;
            for (int num4 = buckets[num2]; num4 >= 0; num4 = entries[num4].next)
            {
                if (entries[num4].hashCode == num && comparer.Equals(entries[num4].key, key))
                {
                    return false;
                }

                num3++;
            }

            int num5;
            if (freeCount > 0)
            {
                num5 = freeList;
                freeList = entries[num5].next;
                freeCount--;
            }
            else
            {
                if (count == entries.Length)
                {
                    Resize();
                    num2 = num % buckets.Length;
                }

                num5 = count;
                count++;
            }

            entries[num5].hashCode = num;
            entries[num5].next = buckets[num2];
            entries[num5].key = key;
            entries[num5].value = value;
            buckets[num2] = num5;
            if (num3 > 100)
            {
                throw new NotSupportedException($"Current comparer {comparer} is not supported");
            }
            return true;
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> keyValuePair)
        {
            Add(keyValuePair.Key, keyValuePair.Value);
        }
        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> keyValuePair)
        {
            int num = FindEntry(keyValuePair.Key);
            if (num >= 0 && EqualityComparer<TValue>.Default.Equals(entries[num].value, keyValuePair.Value))
            {
                return true;
            }

            return false;
        }
        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> keyValuePair)
        {
            int num = FindEntry(keyValuePair.Key);
            if (num >= 0 && EqualityComparer<TValue>.Default.Equals(entries[num].value, keyValuePair.Value))
            {
                Remove(keyValuePair.Key);
                return true;
            }

            return false;
        }

        //
        // Summary:
        //     Removes all keys and values from the System.Collections.Generic.Dictionary`2.

        public void Clear()
        {
            if (count > 0)
            {
                for (int i = 0; i < buckets.Length; i++)
                {
                    buckets[i] = -1;
                }

                Array.Clear(entries, 0, count);
                freeList = -1;
                count = 0;
                freeCount = 0;
            }
        }

        //
        // Summary:
        //     Determines whether the System.Collections.Generic.Dictionary`2 contains the specified
        //     key.
        //
        // Parameters:
        //   key:
        //     The key to locate in the System.Collections.Generic.Dictionary`2.
        //
        // Returns:
        //     true if the System.Collections.Generic.Dictionary`2 contains an element with
        //     the specified key; otherwise, false.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     key is null.

        public bool ContainsKey(TKey key)
        {
            return FindEntry(key) >= 0;
        }

        //
        // Summary:
        //     Determines whether the System.Collections.Generic.Dictionary`2 contains a specific
        //     value.
        //
        // Parameters:
        //   value:
        //     The value to locate in the System.Collections.Generic.Dictionary`2. The value
        //     can be null for reference types.
        //
        // Returns:
        //     true if the System.Collections.Generic.Dictionary`2 contains an element with
        //     the specified value; otherwise, false.

        public bool ContainsValue(TValue value)
        {
            if (value == null)
            {
                for (int i = 0; i < count; i++)
                {
                    if (entries[i].hashCode >= 0 && entries[i].value == null)
                    {
                        return true;
                    }
                }
            }
            else
            {
                EqualityComparer<TValue> @default = EqualityComparer<TValue>.Default;
                for (int j = 0; j < count; j++)
                {
                    if (entries[j].hashCode >= 0 && @default.Equals(entries[j].value, value))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (index < 0 || index > array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (array.Length - index < Count)
            {
                throw new Exception("ExceptionResource.Arg_ArrayPlusOffTooSmall");
            }

            int num = count;
            Entry[] array2 = entries;
            for (int i = 0; i < num; i++)
            {
                if (array2[i].hashCode >= 0)
                {
                    array[index++] = new KeyValuePair<TKey, TValue>(array2[i].key, array2[i].value);
                }
            }
        }

        //
        // Summary:
        //     Returns an enumerator that iterates through the System.Collections.Generic.Dictionary`2.
        //
        // Returns:
        //     A System.Collections.Generic.Dictionary`2.Enumerator structure for the System.Collections.Generic.Dictionary`2.

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }


        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        [Conditional("UNITY_EDITOR")]
        private void checkNullKey(TKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
        }

        private int FindEntry(TKey key)
        {
            checkNullKey(key);

            if (buckets != null)
            {
                int num = comparer.GetHashCode(key) & 0x7FFFFFFF;
                for (int num2 = buckets[num % buckets.Length]; num2 >= 0;)
                {
                    var entry = entries[num2];
                    if (entry.hashCode == num && comparer.Equals(entry.key, key))
                    {
                        return num2;
                    }
                    num2 = entry.next;
                }
            }

            return -1;
        }

        private void Initialize(uint capacity)
        {
            int prime = HashHelpers.GetPrime(capacity);
            buckets = new int[prime];
            buckets.fill(-1);

            entries = new Entry[prime];
            freeList = -1;
        }

        private void Insert(TKey key, TValue value, bool add)
        {
            checkNullKey(key);

            if (buckets == null)
            {
                Initialize(0);
            }

            int num = comparer.GetHashCode(key) & 0x7FFFFFFF;
            int num2 = num % buckets.Length;
            int num3 = 0;
            for (int num4 = buckets[num2]; num4 >= 0;)
            {
                var entry = entries[num4];
                if (entry.hashCode == num && comparer.Equals(entry.key, key))
                {
                    if (add)
                    {
                        throw new ArgumentException("ExceptionResource.Argument_AddingDuplicate");
                    }

                    entries[num4].value = value;
                    return;
                }

                num3++;
                num4 = entry.next;
            }

            int num5;
            if (freeCount > 0)
            {
                num5 = freeList;
                freeList = entries[num5].next;
                freeCount--;
            }
            else
            {
                if (count == entries.Length)
                {
                    Resize();
                    num2 = num % buckets.Length;
                }

                num5 = count;
                count++;
            }

            entries[num5].hashCode = num;
            entries[num5].next = buckets[num2];
            entries[num5].key = key;
            entries[num5].value = value;
            buckets[num2] = num5;
            if (num3 > 100)
            {
                throw new NotSupportedException($"Current comparer {comparer} is not supported");
            }
        }

        public int EnsureCapacity(uint capacity)
        {
            if (capacity <= entries.Length) return entries.Length;
            Resize(HashHelpers.ExpandPrime(capacity), forceNewHashCodes: false);
            return entries.Length;
        }

        private void Resize()
        {
            Resize(HashHelpers.ExpandPrime((uint)count), forceNewHashCodes: false);
        }

        private void Resize(int newSize, bool forceNewHashCodes)
        {
            int[] array = new int[newSize];
            array.fill(-1);

            Entry[] array2 = new Entry[newSize];
            Array.Copy(entries, 0, array2, 0, count);
            if (forceNewHashCodes)
            {
                for (int j = 0; j < count; j++)
                {
                    if (array2[j].hashCode != -1)
                    {
                        array2[j].hashCode = comparer.GetHashCode(array2[j].key) & 0x7FFFFFFF;
                    }
                }
            }

            for (int k = 0; k < count; k++)
            {
                if (array2[k].hashCode >= 0)
                {
                    int num = array2[k].hashCode % newSize;
                    array2[k].next = array[num];
                    array[num] = k;
                }
            }

            buckets = array;
            entries = array2;
        }

        //
        // Summary:
        //     Removes the value with the specified key from the System.Collections.Generic.Dictionary`2.
        //
        // Parameters:
        //   key:
        //     The key of the element to remove.
        //
        // Returns:
        //     true if the element is successfully found and removed; otherwise, false. This
        //     method returns false if key is not found in the System.Collections.Generic.Dictionary`2.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     key is null.

        public bool Remove(TKey key)
        {
            checkNullKey(key);

            if (buckets != null)
            {
                int num = comparer.GetHashCode(key) & 0x7FFFFFFF;
                int num2 = num % buckets.Length;
                int num3 = -1;
                for (int num4 = buckets[num2]; num4 >= 0; num4 = entries[num4].next)
                {
                    if (entries[num4].hashCode == num && comparer.Equals(entries[num4].key, key))
                    {
                        if (num3 < 0)
                        {
                            buckets[num2] = entries[num4].next;
                        }
                        else
                        {
                            entries[num3].next = entries[num4].next;
                        }

                        entries[num4].hashCode = -1;
                        entries[num4].next = freeList;
                        entries[num4].key = default;
                        entries[num4].value = default;
                        freeList = num4;
                        freeCount++;
                        return true;
                    }

                    num3 = num4;
                }
            }

            return false;
        }

        public TValue takeAndRemove(TKey key)
        {
            checkNullKey(key);

            if (buckets != null)
            {
                int num = comparer.GetHashCode(key) & 0x7FFFFFFF;
                int num2 = num % buckets.Length;
                int num3 = -1;
                for (int num4 = buckets[num2]; num4 >= 0; num4 = entries[num4].next)
                {
                    if (entries[num4].hashCode == num && comparer.Equals(entries[num4].key, key))
                    {
                        var value = entries[num4].value;
                        if (num3 < 0)
                        {
                            buckets[num2] = entries[num4].next;
                        }
                        else
                        {
                            entries[num3].next = entries[num4].next;
                        }

                        entries[num4].hashCode = -1;
                        entries[num4].next = freeList;
                        entries[num4].key = default;
                        entries[num4].value = default;
                        freeList = num4;
                        freeCount++;
                        return value;
                    }

                    num3 = num4;
                }
            }

            throw new KeyNotFoundException(nameof(key));
        }

        public bool tryTakeAndRemove(TKey key, out TValue value)
        {
            checkNullKey(key);

            if (buckets != null)
            {
                int num = comparer.GetHashCode(key) & 0x7FFFFFFF;
                int num2 = num % buckets.Length;
                int num3 = -1;
                for (int num4 = buckets[num2]; num4 >= 0; num4 = entries[num4].next)
                {
                    if (entries[num4].hashCode == num && comparer.Equals(entries[num4].key, key))
                    {
                        value = entries[num4].value;
                        if (num3 < 0)
                        {
                            buckets[num2] = entries[num4].next;
                        }
                        else
                        {
                            entries[num3].next = entries[num4].next;
                        }

                        entries[num4].hashCode = -1;
                        entries[num4].next = freeList;
                        entries[num4].key = default;
                        entries[num4].value = default;
                        freeList = num4;
                        freeCount++;
                        return true;
                    }

                    num3 = num4;
                }
            }

            value = default;
            return false;
        }

        //
        // Summary:
        //     Gets the value associated with the specified key.
        //
        // Parameters:
        //   key:
        //     The key of the value to get.
        //
        //   value:
        //     When this method returns, contains the value associated with the specified key,
        //     if the key is found; otherwise, the default value for the type of the value parameter.
        //     This parameter is passed uninitialized.
        //
        // Returns:
        //     true if the System.Collections.Generic.Dictionary`2 contains an element with
        //     the specified key; otherwise, false.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     key is null.

        public bool TryGetValue(TKey key, out TValue value)
        {
            int num = FindEntry(key);
            if (num >= 0)
            {
                value = entries[num].value;
                return true;
            }

            value = default;
            return false;
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
        {
            CopyTo(array, index);
        }

        //
        // Summary:
        //     Returns an enumerator that iterates through the collection.
        //
        // Returns:
        //     An System.Collections.IEnumerator that can be used to iterate through the collection.

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }
    }
}
