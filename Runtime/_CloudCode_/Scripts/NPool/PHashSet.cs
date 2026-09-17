using System.Collections.Generic;

namespace Nextension
{
    public sealed class PHashSet<T> : PooledSet<T, HashSet<T>, PHashSet<T>>
    {
        public static PHashSet<T> get() => __get();
        public static PHashSet<T> getWithoutTracking() => __get();

        public HashSet<T>.Enumerator GetEnumerator() => Collection.GetEnumerator();

        protected override int __getCapacity() => Collection.EnsureCapacity(0);
    }
}
