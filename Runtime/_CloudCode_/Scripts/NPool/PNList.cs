using System;
using System.Collections.Generic;

namespace Nextension
{
    public sealed class PNList<T> : PooledList<T, NList<T>, PNList<T>>
    {
        static PNList()
        {
            if (typeof(T) == typeof(byte))
            {
                CapacityThreshold = DEFAULT_CAPACITY_THRESHOLD * 2;
            }
        }
        public static PNList<T> get() => __get();
        public static PNList<T> getWithoutTracking() => __get();

        public int Capacity { get => Collection.Capacity; }
        public void SetSize(int count) => Collection.SetCount(count);
        public void EnsureCapacity(int capacity) => Collection.EnsureCapacity(capacity);

        protected override int __getCapacity() => Collection.Capacity;

        public Span<T> AsSpan() => Collection.AsSpan();
        public Span<T> AsSpan(int start, int length) => Collection.AsSpan(start, length);

        public void AddRange<TCollection>(TCollection collection) where TCollection : IEnumerable<T> => Collection.AddRange(collection);
        public void AddRange(ReadOnlySpan<T> span) => Collection.AddRange(span);

        public void Sort()
        {
            Collection.Sort();
        }

        public void RemoveLast() => Collection.RemoveLast();
        public void RemoveAtWithoutChecks(int index) => Collection.RemoveAtWithoutChecks(index);
        public void RemoveAtSwapBackWithoutChecks(int index) => Collection.RemoveAtSwapBackWithoutChecks(index);

        public ArrayEnumerator<T> GetEnumerator() => Collection.GetEnumerator();
    }

    public sealed class PList<T> : PooledList<T, List<T>, PList<T>>
    {
        public static PList<T> get() => __get();
        public static PList<T> getWithoutTracking() => __get();

        protected override int __getCapacity() => Collection.Capacity;

        public void AddRange(IEnumerable<T> collection) => Collection.AddRange(collection);
        public List<T>.Enumerator GetEnumerator() => Collection.GetEnumerator();
    }
}
