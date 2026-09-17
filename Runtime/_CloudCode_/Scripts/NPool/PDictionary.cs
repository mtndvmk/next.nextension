using System.Collections.Generic;

namespace Nextension
{
    public sealed class PDictionary<TKey, TValue> : PooledDictionary<TKey, TValue, Dictionary<TKey, TValue>, PDictionary<TKey, TValue>>
    {
        public static PDictionary<TKey, TValue> get() => __get();
        public static PDictionary<TKey, TValue> getWithoutTracking() => __get();

        public Dictionary<TKey, TValue>.Enumerator GetEnumerator() => Collection.GetEnumerator();

        protected override int __getCapacity() => Collection.EnsureCapacity(0);
    }
}
