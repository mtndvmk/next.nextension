//using System;
//using System.Collections;
//using System.Collections.Generic;

//namespace Nextension
//{
//    public struct NPUHSet<T> : ICollection<T>, IEnumerable<T>, IEnumerable, ISet<T>, IReadOnlyCollection<T>
//        where T : unmanaged
//    {
//        internal struct ElementCount
//        {
//            internal int uniqueCount;

//            internal int unfoundCount;
//        }

//        internal struct Slot
//        {
//            internal int hashCode;

//            internal int next;

//            internal T value;
//        }

//        public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
//        {
//            private NPUHSet<T> set;

//            private int index;

//            private T current;

//            //
//            // Summary:
//            //     Gets the element at the current position of the enumerator.
//            //
//            // Returns:
//            //     The element in the System.Collections.Generic.HashSet`1 collection at the current
//            //     position of the enumerator.   
//            public T Current
//            {

//                get
//                {
//                    return current;
//                }
//            }

//            //
//            // Summary:
//            //     Gets the element at the current position of the enumerator.
//            //
//            // Returns:
//            //     The element in the collection at the current position of the enumerator, as an
//            //     System.Object.
//            //
//            // Exceptions:
//            //   T:System.InvalidOperationException:
//            //     The enumerator is positioned before the first element of the collection or after
//            //     the last element.

//            object IEnumerator.Current
//            {

//                get
//                {
//                    if (index == 0 || index == set.m_lastIndex + 1)
//                    {
//                        throw new InvalidOperationException("InvalidOperation_EnumOpCantHappen");
//                    }

//                    return Current;
//                }
//            }

//            internal Enumerator(NPUHSet<T> set)
//            {
//                this.set = set;
//                index = 0;
//                current = default(T);
//            }

//            //
//            // Summary:
//            //     Releases all resources used by a System.Collections.Generic.HashSet`1.Enumerator
//            //     object.

//            public void Dispose()
//            {
//            }

//            //
//            // Summary:
//            //     Advances the enumerator to the next element of the System.Collections.Generic.HashSet`1
//            //     collection.
//            //
//            // Returns:
//            //     true if the enumerator was successfully advanced to the next element; false if
//            //     the enumerator has passed the end of the collection.
//            //
//            // Exceptions:
//            //   T:System.InvalidOperationException:
//            //     The collection was modified after the enumerator was created.

//            public bool MoveNext()
//            {
//                while (index < set.m_lastIndex)
//                {
//                    if (set.m_slots[index].hashCode >= 0)
//                    {
//                        current = set.m_slots[index].value;
//                        index++;
//                        return true;
//                    }

//                    index++;
//                }

//                index = set.m_lastIndex + 1;
//                current = default(T);
//                return false;
//            }

//            //
//            // Summary:
//            //     Sets the enumerator to its initial position, which is before the first element
//            //     in the collection.
//            //
//            // Exceptions:
//            //   T:System.InvalidOperationException:
//            //     The collection was modified after the enumerator was created.

//            void IEnumerator.Reset()
//            {
//                index = 0;
//                current = default(T);
//            }
//        }

//        private NPUArray<int> m_buckets;

//        private NPUArray<Slot> m_slots;

//        private int m_count;

//        private int m_lastIndex;

//        private int m_freeList;

//        public bool IsCreated => m_buckets.IsCreated;

//        //
//        // Summary:
//        //     Gets the number of elements that are contained in a set.
//        //
//        // Returns:
//        //     The number of elements that are contained in the set.

//        public int Count
//        {

//            get
//            {
//                return m_count;
//            }
//        }

//        //
//        // Summary:
//        //     Gets a value indicating whether a collection is read-only.
//        //
//        // Returns:
//        //     true if the collection is read-only; otherwise, false.

//        bool ICollection<T>.IsReadOnly
//        {

//            get
//            {
//                return false;
//            }
//        }

//        //
//        // Summary:
//        //     Gets the System.Collections.Generic.IEqualityComparer`1 object that is used to
//        //     determine equality for the values in the set.
//        //
//        // Returns:
//        //     The System.Collections.Generic.IEqualityComparer`1 object that is used to determine
//        //     equality for the values in the set.

//        public IEqualityComparer<T> Comparer
//        {

//            get
//            {
//                return EqualityComparer<T>.Default;
//            }
//        }

//        //
//        // Summary:
//        //     Initializes a new instance of the System.Collections.Generic.HashSet`1 class
//        //     that is empty and uses the specified equality comparer for the set type.
//        //
//        // Parameters:
//        //   comparer:
//        //     The System.Collections.Generic.IEqualityComparer`1 implementation to use when
//        //     comparing values in the set, or null to use the default System.Collections.Generic.EqualityComparer`1
//        //     implementation for the set type.

//        public static NPUHSet<T> get()
//        {
//            var set = new NPUHSet<T>()
//            {
//                m_lastIndex = 0,
//                m_count = 0,
//                m_freeList = -1,
//            };
//            set.Initialize(0);
//            return set;
//        }

//        //
//        // Summary:
//        //     Initializes a new instance of the System.Collections.Generic.HashSet`1 class
//        //     that uses the specified equality comparer for the set type, and has sufficient
//        //     capacity to accommodate capacity elements.
//        //
//        // Parameters:
//        //   capacity:
//        //     The initial size of the System.Collections.Generic.HashSet`1
//        //
//        //   comparer:
//        //     The System.Collections.Generic.IEqualityComparer`1 implementation to use when
//        //     comparing values in the set, or null (Nothing in Visual Basic) to use the default
//        //     System.Collections.Generic.IEqualityComparer`1 implementation for the set type.
//        public NPUHSet(int capacity)
//            : this()
//        {
//            if (capacity < 0)
//            {
//                throw new ArgumentOutOfRangeException("capacity");
//            }

//            if (capacity > 0)
//            {
//                Initialize(capacity);
//            }
//        }

//        //
//        // Summary:
//        //     Adds an item to an System.Collections.Generic.ICollection`1 object.
//        //
//        // Parameters:
//        //   item:
//        //     The object to add to the System.Collections.Generic.ICollection`1 object.
//        //
//        // Exceptions:
//        //   T:System.NotSupportedException:
//        //     The System.Collections.Generic.ICollection`1 is read-only.

//        void ICollection<T>.Add(T item)
//        {
//            AddIfNotPresent(item);
//        }

//        //
//        // Summary:
//        //     Removes all elements from a System.Collections.Generic.HashSet`1 object.

//        public void Clear()
//        {
//            if (m_lastIndex > 0)
//            {
//                m_slots.Dispose();
//                m_buckets.Dispose();

//                m_lastIndex = 0;
//                m_count = 0;
//                m_freeList = -1;
//            }
//        }

//        //
//        // Summary:
//        //     Determines whether a System.Collections.Generic.HashSet`1 object contains the
//        //     specified element.
//        //
//        // Parameters:
//        //   item:
//        //     The element to locate in the System.Collections.Generic.HashSet`1 object.
//        //
//        // Returns:
//        //     true if the System.Collections.Generic.HashSet`1 object contains the specified
//        //     element; otherwise, false.

//        public bool Contains(T item)
//        {
//            int num = InternalGetHashCode(item);
//            var m_comparer = Comparer;
//            for (int num2 = m_buckets[num % m_buckets.Count] - 1; num2 >= 0; num2 = m_slots[num2].next)
//            {
//                if (m_slots[num2].hashCode == num && m_comparer.Equals(m_slots[num2].value, item))
//                {
//                    return true;
//                }
//            }

//            return false;
//        }

//        //
//        // Summary:
//        //     Copies the elements of a System.Collections.Generic.HashSet`1 object to an array,
//        //     starting at the specified array index.
//        //
//        // Parameters:
//        //   array:
//        //     The one-dimensional array that is the destination of the elements copied from
//        //     the System.Collections.Generic.HashSet`1 object. The array must have zero-based
//        //     indexing.
//        //
//        //   arrayIndex:
//        //     The zero-based index in array at which copying begins.
//        //
//        // Exceptions:
//        //   T:System.ArgumentNullException:
//        //     array is null.
//        //
//        //   T:System.ArgumentOutOfRangeException:
//        //     arrayIndex is less than 0.
//        //
//        //   T:System.ArgumentException:
//        //     arrayIndex is greater than the length of the destination array.

//        public void CopyTo(T[] array, int arrayIndex)
//        {
//            CopyTo(array, arrayIndex, m_count);
//        }

//        //
//        // Summary:
//        //     Removes the specified element from a System.Collections.Generic.HashSet`1 object.
//        //
//        //
//        // Parameters:
//        //   item:
//        //     The element to remove.
//        //
//        // Returns:
//        //     true if the element is successfully found and removed; otherwise, false. This
//        //     method returns false if item is not found in the System.Collections.Generic.HashSet`1
//        //     object.

//        public bool Remove(T item)
//        {
//            int num = InternalGetHashCode(item);
//            int num2 = num % m_buckets.Count;
//            int num3 = -1;
//            var m_comparer = Comparer;
//            for (int num4 = m_buckets[num2] - 1; num4 >= 0; num4 = m_slots[num4].next)
//            {
//                if (m_slots[num4].hashCode == num && m_comparer.Equals(m_slots[num4].value, item))
//                {
//                    if (num3 < 0)
//                    {
//                        m_buckets[num2] = m_slots[num4].next + 1;
//                    }
//                    else
//                    {
//                        var slot3 = m_slots[num3];
//                        slot3.next = m_slots[num4].next;
//                        m_slots[num3] = slot3;
//                    }

//                    var slot4 = m_slots[num4];
//                    slot4.hashCode = -1;
//                    slot4.value = default(T);
//                    slot4.next = m_freeList;
//                    m_slots[num4] = slot4;

//                    m_count--;
//                    if (m_count == 0)
//                    {
//                        m_lastIndex = 0;
//                        m_freeList = -1;
//                    }
//                    else
//                    {
//                        m_freeList = num4;
//                    }

//                    return true;
//                }

//                num3 = num4;
//            }

//            return false;
//        }

//        //
//        // Summary:
//        //     Returns an enumerator that iterates through a System.Collections.Generic.HashSet`1
//        //     object.
//        //
//        // Returns:
//        //     A System.Collections.Generic.HashSet`1.Enumerator object for the System.Collections.Generic.HashSet`1
//        //     object.

//        public Enumerator GetEnumerator()
//        {
//            return new Enumerator(this);
//        }

//        //
//        // Summary:
//        //     Returns an enumerator that iterates through a collection.
//        //
//        // Returns:
//        //     An System.Collections.Generic.IEnumerator`1 object that can be used to iterate
//        //     through the collection.

//        IEnumerator<T> IEnumerable<T>.GetEnumerator()
//        {
//            return new Enumerator(this);
//        }

//        //
//        // Summary:
//        //     Returns an enumerator that iterates through a collection.
//        //
//        // Returns:
//        //     An System.Collections.IEnumerator object that can be used to iterate through
//        //     the collection.

//        IEnumerator IEnumerable.GetEnumerator()
//        {
//            return new Enumerator(this);
//        }

//        //
//        // Summary:
//        //     Adds the specified element to a set.
//        //
//        // Parameters:
//        //   item:
//        //     The element to add to the set.
//        //
//        // Returns:
//        //     true if the element is added to the System.Collections.Generic.HashSet`1 object;
//        //     false if the element is already present.

//        public bool Add(T item)
//        {
//            return AddIfNotPresent(item);
//        }

//        //
//        // Summary:
//        //     Searches the set for a given value and returns the equal value it finds, if any.
//        //
//        //
//        // Parameters:
//        //   equalValue:
//        //     The value to search for.
//        //
//        //   actualValue:
//        //     The value from the set that the search found, or the default value of T when
//        //     the search yielded no match.
//        //
//        // Returns:
//        //     A value indicating whether the search was successful.
//        public bool TryGetValue(T equalValue, out T actualValue)
//        {
//            int num = InternalIndexOf(equalValue);
//            if (num >= 0)
//            {
//                actualValue = m_slots[num].value;
//                return true;
//            }

//            actualValue = default(T);
//            return false;
//        }

//        //
//        // Summary:
//        //     Modifies the current System.Collections.Generic.HashSet`1 object to contain all
//        //     elements that are present in itself, the specified collection, or both.
//        //
//        // Parameters:
//        //   other:
//        //     The collection to compare to the current System.Collections.Generic.HashSet`1
//        //     object.
//        //
//        // Exceptions:
//        //   T:System.ArgumentNullException:
//        //     other is null.

//        public void UnionWith(IEnumerable<T> other)
//        {
//            if (other == null)
//            {
//                throw new ArgumentNullException("other");
//            }

//            foreach (T item in other)
//            {
//                AddIfNotPresent(item);
//            }
//        }

//        //
//        // Summary:
//        //     Modifies the current System.Collections.Generic.HashSet`1 object to contain only
//        //     elements that are present in that object and in the specified collection.
//        //
//        // Parameters:
//        //   other:
//        //     The collection to compare to the current System.Collections.Generic.HashSet`1
//        //     object.
//        //
//        // Exceptions:
//        //   T:System.ArgumentNullException:
//        //     other is null.

//        public void IntersectWith(IEnumerable<T> other)
//        {
//            if (other == null)
//            {
//                throw new ArgumentNullException("other");
//            }

//            if (m_count == 0)
//            {
//                return;
//            }

//            if (other is ICollection<T> collection)
//            {
//                if (collection.Count == 0)
//                {
//                    Clear();
//                    return;
//                }

//                if (other is NPUHSet<T> hashSet && AreEqualityComparersEqual(this, hashSet))
//                {
//                    IntersectWithHashSetWithSameEC(hashSet);
//                    return;
//                }
//            }

//            IntersectWithEnumerable(other);
//        }

//        //
//        // Summary:
//        //     Removes all elements in the specified collection from the current System.Collections.Generic.HashSet`1
//        //     object.
//        //
//        // Parameters:
//        //   other:
//        //     The collection of items to remove from the System.Collections.Generic.HashSet`1
//        //     object.
//        //
//        // Exceptions:
//        //   T:System.ArgumentNullException:
//        //     other is null.

//        public void ExceptWith(IEnumerable<T> other)
//        {
//            if (other == null)
//            {
//                throw new ArgumentNullException("other");
//            }

//            if (m_count == 0)
//            {
//                return;
//            }

//            //if (other == this)
//            //{
//            //    Clear();
//            //    return;
//            //}

//            foreach (T item in other)
//            {
//                Remove(item);
//            }
//        }

//        public void SymmetricExceptWith(IEnumerable<T> other)
//        {
//            if (other == null)
//            {
//                throw new ArgumentNullException("other");
//            }

//            if (m_count == 0)
//            {
//                UnionWith(other);
//            }
//            //else if (other == this)
//            //{
//            //    Clear();
//            //}
//            else if (other is NPUHSet<T> hashSet && AreEqualityComparersEqual(this, hashSet))
//            {
//                SymmetricExceptWithUniqueHashSet(hashSet);
//            }
//            else
//            {
//                SymmetricExceptWithEnumerable(other);
//            }
//        }

//        public bool IsSubsetOf(IEnumerable<T> other)
//        {
//            if (other == null)
//            {
//                throw new ArgumentNullException("other");
//            }

//            if (m_count == 0)
//            {
//                return true;
//            }

//            if (other is NPUHSet<T> hashSet && AreEqualityComparersEqual(this, hashSet))
//            {
//                if (m_count > hashSet.Count)
//                {
//                    return false;
//                }

//                return IsSubsetOfHashSetWithSameEC(hashSet);
//            }

//            ElementCount elementCount = CheckUniqueAndUnfoundElements(other, returnIfUnfound: false);
//            if (elementCount.uniqueCount == m_count)
//            {
//                return elementCount.unfoundCount >= 0;
//            }

//            return false;
//        }

//        public bool IsProperSubsetOf(IEnumerable<T> other)
//        {
//            if (other == null)
//            {
//                throw new ArgumentNullException("other");
//            }

//            if (other is ICollection<T> collection)
//            {
//                if (m_count == 0)
//                {
//                    return collection.Count > 0;
//                }

//                if (other is NPUHSet<T> hashSet && AreEqualityComparersEqual(this, hashSet))
//                {
//                    if (m_count >= hashSet.Count)
//                    {
//                        return false;
//                    }

//                    return IsSubsetOfHashSetWithSameEC(hashSet);
//                }
//            }

//            ElementCount elementCount = CheckUniqueAndUnfoundElements(other, returnIfUnfound: false);
//            if (elementCount.uniqueCount == m_count)
//            {
//                return elementCount.unfoundCount > 0;
//            }

//            return false;
//        }

//        public bool IsSupersetOf(IEnumerable<T> other)
//        {
//            if (other == null)
//            {
//                throw new ArgumentNullException("other");
//            }

//            if (other is ICollection<T> collection)
//            {
//                if (collection.Count == 0)
//                {
//                    return true;
//                }

//                if (other is NPUHSet<T> hashSet && AreEqualityComparersEqual(this, hashSet) && hashSet.Count > m_count)
//                {
//                    return false;
//                }
//            }

//            return ContainsAllElements(other);
//        }

//        public bool IsProperSupersetOf(IEnumerable<T> other)
//        {
//            if (other == null)
//            {
//                throw new ArgumentNullException("other");
//            }

//            if (m_count == 0)
//            {
//                return false;
//            }

//            if (other is ICollection<T> collection)
//            {
//                if (collection.Count == 0)
//                {
//                    return true;
//                }

//                if (other is NPUHSet<T> hashSet && AreEqualityComparersEqual(this, hashSet))
//                {
//                    if (hashSet.Count >= m_count)
//                    {
//                        return false;
//                    }

//                    return ContainsAllElements(hashSet);
//                }
//            }

//            ElementCount elementCount = CheckUniqueAndUnfoundElements(other, returnIfUnfound: true);
//            if (elementCount.uniqueCount < m_count)
//            {
//                return elementCount.unfoundCount == 0;
//            }

//            return false;
//        }

//        public bool Overlaps(IEnumerable<T> other)
//        {
//            if (other == null)
//            {
//                throw new ArgumentNullException("other");
//            }

//            if (m_count == 0)
//            {
//                return false;
//            }

//            foreach (T item in other)
//            {
//                if (Contains(item))
//                {
//                    return true;
//                }
//            }

//            return false;
//        }

//        public bool SetEquals(IEnumerable<T> other)
//        {
//            if (other == null)
//            {
//                throw new ArgumentNullException("other");
//            }

//            if (other is NPUHSet<T> hashSet && AreEqualityComparersEqual(this, hashSet))
//            {
//                if (m_count != hashSet.Count)
//                {
//                    return false;
//                }

//                return ContainsAllElements(hashSet);
//            }

//            if (other is ICollection<T> collection && m_count == 0 && collection.Count > 0)
//            {
//                return false;
//            }

//            ElementCount elementCount = CheckUniqueAndUnfoundElements(other, returnIfUnfound: true);
//            if (elementCount.uniqueCount == m_count)
//            {
//                return elementCount.unfoundCount == 0;
//            }

//            return false;
//        }

//        public void CopyTo(T[] array)
//        {
//            CopyTo(array, 0, m_count);
//        }


//        public void CopyTo(T[] array, int arrayIndex, int count)
//        {
//            if (array == null)
//            {
//                throw new ArgumentNullException("array");
//            }

//            if (arrayIndex < 0)
//            {
//                throw new ArgumentOutOfRangeException("arrayIndex", "ArgumentOutOfRange_NeedNonNegNum");
//            }

//            if (count < 0)
//            {
//                throw new ArgumentOutOfRangeException("count", "ArgumentOutOfRange_NeedNonNegNum");
//            }

//            if (arrayIndex > array.Length || count > array.Length - arrayIndex)
//            {
//                throw new ArgumentException("Arg_ArrayPlusOffTooSmall");
//            }

//            int num = 0;
//            for (int i = 0; i < m_lastIndex; i++)
//            {
//                if (num >= count)
//                {
//                    break;
//                }

//                if (m_slots[i].hashCode >= 0)
//                {
//                    array[arrayIndex + num] = m_slots[i].value;
//                    num++;
//                }
//            }
//        }

//        public int RemoveWhere(Predicate<T> match)
//        {
//            if (match == null)
//            {
//                throw new ArgumentNullException("match");
//            }

//            int num = 0;
//            for (int i = 0; i < m_lastIndex; i++)
//            {
//                if (m_slots[i].hashCode >= 0)
//                {
//                    T value = m_slots[i].value;
//                    if (match(value) && Remove(value))
//                    {
//                        num++;
//                    }
//                }
//            }

//            return num;
//        }

//        public void TrimExcess()
//        {
//            if (m_count == 0)
//            {
//                if (m_buckets.IsCreated)
//                {
//                    m_buckets.Dispose();
//                    m_buckets = default;
//                }
//                if (m_slots.IsCreated)
//                {
//                    m_slots.Dispose();
//                    m_slots = default;
//                }
//                return;
//            }

//            int prime = HashHelpers.GetPrime(m_count);

//            var array = NPUArray<Slot>.getWithoutTracking();
//            var array2 = NPUArray<int>.getWithoutTracking();

//            array.ensureCapacity(prime);
//            array2.ensureCapacity(prime);
//            int num = 0;
//            for (int i = 0; i < m_lastIndex; i++)
//            {
//                if (m_slots[i].hashCode >= 0)
//                {
//                    array[num] = m_slots[i];
//                    int num2 = array[num].hashCode % prime;
//                    var slot = array[num];
//                    slot.next = array2[num2] - 1;
//                    array[num] = slot;
//                    array2[num2] = num + 1;
//                    num++;
//                }
//            }

//            m_lastIndex = num;

//            m_buckets.Dispose();
//            m_slots.Dispose();

//            m_slots = array;
//            m_buckets = array2;
//            m_freeList = -1;
//        }

//        private void Initialize(int capacity)
//        {
//            int prime = HashHelpers.GetPrime(capacity);

//            if (m_buckets.IsCreated)
//            {
//                m_buckets.Dispose();
//            }
//            if (m_slots.IsCreated)
//            {
//                m_slots.Dispose();
//            }

//            m_buckets = NPUArray<int>.getWithoutTracking();
//            m_slots = NPUArray<Slot>.getWithoutTracking();

//            m_buckets.ensureCapacity(prime);
//            m_slots.ensureCapacity(prime);
//        }

//        private void IncreaseCapacity()
//        {
//            int num = HashHelpers.ExpandPrime(m_count);
//            if (num <= m_count)
//            {
//                throw new ArgumentException("Arg_HSCapacityOverflow");
//            }

//            SetCapacity(num, forceNewHashCodes: false);
//        }

//        private void SetCapacity(int newSize, bool forceNewHashCodes)
//        {
//            var array = NPUArray<Slot>.getWithoutTracking();
//            array.ensureCapacity(newSize);
//            if (m_slots.IsCreated)
//            {
//                m_slots.CopyTo(array);
//            }

//            if (forceNewHashCodes)
//            {
//                for (int i = 0; i < m_lastIndex; i++)
//                {
//                    if (array[i].hashCode != -1)
//                    {
//                        var slot = array[i];
//                        slot.hashCode = InternalGetHashCode(array[i].value);
//                        array[i] = slot;
//                    }
//                }
//            }

//            var array2 = NPUArray<int>.getWithoutTracking();
//            array2.ensureCapacity(newSize);
//            for (int j = 0; j < m_lastIndex; j++)
//            {
//                int num = array[j].hashCode % newSize;
//                var slot = array[j];
//                slot.next = array2[num] - 1;
//                array[j] = slot;
//                array2[num] = j + 1;
//            }

//            m_slots.Dispose();
//            m_slots = array;

//            m_buckets.Dispose();
//            m_buckets = array2;
//        }

//        private bool AddIfNotPresent(T value)
//        {
//            int num = InternalGetHashCode(value);
//            int num2 = num % m_buckets.Count;
//            int num3 = 0;
//            var m_comparer = Comparer;
//            for (int num4 = m_buckets[num % m_buckets.Count] - 1; num4 >= 0; num4 = m_slots[num4].next)
//            {
//                if (m_slots[num4].hashCode == num && m_comparer.Equals(m_slots[num4].value, value))
//                {
//                    return false;
//                }

//                num3++;
//            }

//            int num5;
//            if (m_freeList >= 0)
//            {
//                num5 = m_freeList;
//                m_freeList = m_slots[num5].next;
//            }
//            else
//            {
//                if (m_lastIndex == m_slots.Length)
//                {
//                    IncreaseCapacity();
//                    num2 = num % m_buckets.Length;
//                }

//                num5 = m_lastIndex;
//                m_lastIndex++;
//            }

//            m_slots[num5].hashCode = num;
//            m_slots[num5].value = value;
//            m_slots[num5].next = m_buckets[num2] - 1;
//            m_buckets[num2] = num5 + 1;
//            m_count++;
//            m_version++;
//            if (num3 > 100)
//            {
//                throw new NotSupportedException($"Current comparer {m_comparer} is not supported");
//            }

//            return true;
//        }

//        private void AddValue(int index, int hashCode, T value)
//        {
//            int num = hashCode % m_buckets.Length;
//            m_slots[index].hashCode = hashCode;
//            m_slots[index].value = value;
//            m_slots[index].next = m_buckets[num] - 1;
//            m_buckets[num] = index + 1;
//        }

//        private bool ContainsAllElements(IEnumerable<T> other)
//        {
//            foreach (T item in other)
//            {
//                if (!Contains(item))
//                {
//                    return false;
//                }
//            }

//            return true;
//        }

//        private bool IsSubsetOfHashSetWithSameEC(NPUHSet<T> other)
//        {
//            using (Enumerator enumerator = GetEnumerator())
//            {
//                while (enumerator.MoveNext())
//                {
//                    T current = enumerator.Current;
//                    if (!other.Contains(current))
//                    {
//                        return false;
//                    }
//                }
//            }

//            return true;
//        }

//        private void IntersectWithHashSetWithSameEC(NPUHSet<T> other)
//        {
//            for (int i = 0; i < m_lastIndex; i++)
//            {
//                if (m_slots[i].hashCode >= 0)
//                {
//                    T value = m_slots[i].value;
//                    if (!other.Contains(value))
//                    {
//                        Remove(value);
//                    }
//                }
//            }
//        }

//        private unsafe void IntersectWithEnumerable(IEnumerable<T> other)
//        {
//            throw new NotImplementedException();
//            //int lastIndex = m_lastIndex;
//            //int num = BitHelper.ToIntArrayLength(lastIndex);
//            //BitHelper bitHelper;
//            //if (num <= 100)
//            //{
//            //    int* bitArrayPtr = stackalloc int[num];
//            //    bitHelper = new BitHelper(bitArrayPtr, num);
//            //}
//            //else
//            //{
//            //    int[] bitArray = new int[num];
//            //    bitHelper = new BitHelper(bitArray, num);
//            //}

//            //foreach (T item in other)
//            //{
//            //    int num2 = InternalIndexOf(item);
//            //    if (num2 >= 0)
//            //    {
//            //        bitHelper.MarkBit(num2);
//            //    }
//            //}

//            //for (int i = 0; i < lastIndex; i++)
//            //{
//            //    if (m_slots[i].hashCode >= 0 && !bitHelper.IsMarked(i))
//            //    {
//            //        Remove(m_slots[i].value);
//            //    }
//            //}
//        }

//        private int InternalIndexOf(T item)
//        {
//            int num = InternalGetHashCode(item);
//            var m_comparer = Comparer;
//            for (int num2 = m_buckets[num % m_buckets.Length] - 1; num2 >= 0; num2 = m_slots[num2].next)
//            {
//                if (m_slots[num2].hashCode == num && m_comparer.Equals(m_slots[num2].value, item))
//                {
//                    return num2;
//                }
//            }

//            return -1;
//        }

//        private void SymmetricExceptWithUniqueHashSet(NPUHSet<T> other)
//        {
//            foreach (T item in other)
//            {
//                if (!Remove(item))
//                {
//                    AddIfNotPresent(item);
//                }
//            }
//        }

//        private unsafe void SymmetricExceptWithEnumerable(IEnumerable<T> other)
//        {
//            throw new NotImplementedException();
//            //int lastIndex = m_lastIndex;
//            //int num = BitHelper.ToIntArrayLength(lastIndex);
//            //BitHelper bitHelper;
//            //BitHelper bitHelper2;
//            //if (num <= 50)
//            //{
//            //    int* bitArrayPtr = stackalloc int[num];
//            //    bitHelper = new BitHelper(bitArrayPtr, num);
//            //    int* bitArrayPtr2 = stackalloc int[num];
//            //    bitHelper2 = new BitHelper(bitArrayPtr2, num);
//            //}
//            //else
//            //{
//            //    int[] bitArray = new int[num];
//            //    bitHelper = new BitHelper(bitArray, num);
//            //    int[] bitArray2 = new int[num];
//            //    bitHelper2 = new BitHelper(bitArray2, num);
//            //}

//            //foreach (T item in other)
//            //{
//            //    int location = 0;
//            //    if (AddOrGetLocation(item, out location))
//            //    {
//            //        bitHelper2.MarkBit(location);
//            //    }
//            //    else if (location < lastIndex && !bitHelper2.IsMarked(location))
//            //    {
//            //        bitHelper.MarkBit(location);
//            //    }
//            //}

//            //for (int i = 0; i < lastIndex; i++)
//            //{
//            //    if (bitHelper.IsMarked(i))
//            //    {
//            //        Remove(m_slots[i].value);
//            //    }
//            //}
//        }

//        private bool AddOrGetLocation(T value, out int location)
//        {
//            int num = InternalGetHashCode(value);
//            int num2 = num % m_buckets.Length;
//            var m_comparer = Comparer;
//            for (int num3 = m_buckets[num % m_buckets.Length] - 1; num3 >= 0; num3 = m_slots[num3].next)
//            {
//                if (m_slots[num3].hashCode == num && m_comparer.Equals(m_slots[num3].value, value))
//                {
//                    location = num3;
//                    return false;
//                }
//            }

//            int num4;
//            if (m_freeList >= 0)
//            {
//                num4 = m_freeList;
//                m_freeList = m_slots[num4].next;
//            }
//            else
//            {
//                if (m_lastIndex == m_slots.Length)
//                {
//                    IncreaseCapacity();
//                    num2 = num % m_buckets.Length;
//                }

//                num4 = m_lastIndex;
//                m_lastIndex++;
//            }

//            m_slots[num4].hashCode = num;
//            m_slots[num4].value = value;
//            m_slots[num4].next = m_buckets[num2] - 1;
//            m_buckets[num2] = num4 + 1;
//            m_count++;
//            m_version++;
//            location = num4;
//            return true;
//        }

//        private unsafe ElementCount CheckUniqueAndUnfoundElements(IEnumerable<T> other, bool returnIfUnfound)
//        {
//            throw new NotImplementedException();
//            //ElementCount result = default(ElementCount);
//            //if (m_count == 0)
//            //{
//            //    int num = 0;
//            //    using (IEnumerator<T> enumerator = other.GetEnumerator())
//            //    {
//            //        if (enumerator.MoveNext())
//            //        {
//            //            T current = enumerator.Current;
//            //            num++;
//            //        }
//            //    }

//            //    result.uniqueCount = 0;
//            //    result.unfoundCount = num;
//            //    return result;
//            //}

//            //int lastIndex = m_lastIndex;
//            //int num2 = BitHelper.ToIntArrayLength(lastIndex);
//            //BitHelper bitHelper;
//            //if (num2 <= 100)
//            //{
//            //    int* bitArrayPtr = stackalloc int[num2];
//            //    bitHelper = new BitHelper(bitArrayPtr, num2);
//            //}
//            //else
//            //{
//            //    int[] bitArray = new int[num2];
//            //    bitHelper = new BitHelper(bitArray, num2);
//            //}

//            //int num3 = 0;
//            //int num4 = 0;
//            //foreach (T item in other)
//            //{
//            //    int num5 = InternalIndexOf(item);
//            //    if (num5 >= 0)
//            //    {
//            //        if (!bitHelper.IsMarked(num5))
//            //        {
//            //            bitHelper.MarkBit(num5);
//            //            num4++;
//            //        }
//            //    }
//            //    else
//            //    {
//            //        num3++;
//            //        if (returnIfUnfound)
//            //        {
//            //            break;
//            //        }
//            //    }
//            //}

//            //result.uniqueCount = num4;
//            //result.unfoundCount = num3;
//            //return result;
//        }

//        internal T[] ToArray()
//        {
//            T[] array = new T[Count];
//            CopyTo(array);
//            return array;
//        }

//        internal static bool HashSetEquals(NPUHSet<T> set1, NPUHSet<T> set2, IEqualityComparer<T> comparer)
//        {
//            if (set1 == null)
//            {
//                return set2 == null;
//            }

//            if (set2 == null)
//            {
//                return false;
//            }

//            if (AreEqualityComparersEqual(set1, set2))
//            {
//                if (set1.Count != set2.Count)
//                {
//                    return false;
//                }

//                foreach (T item in set2)
//                {
//                    if (!set1.Contains(item))
//                    {
//                        return false;
//                    }
//                }

//                return true;
//            }

//            foreach (T item2 in set2)
//            {
//                bool flag = false;
//                foreach (T item3 in set1)
//                {
//                    if (comparer.Equals(item2, item3))
//                    {
//                        flag = true;
//                        break;
//                    }
//                }

//                if (!flag)
//                {
//                    return false;
//                }
//            }

//            return true;
//        }

//        private static bool AreEqualityComparersEqual(NPUHSet<T> set1, NPUHSet<T> set2)
//        {
//            return set1.Comparer.Equals(set2.Comparer);
//        }

//        private int InternalGetHashCode(T item)
//        {
//            return Comparer.GetHashCode(item) & 0x7FFFFFFF;
//        }
//    }
//}
