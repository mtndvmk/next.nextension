using System.Collections.Generic;

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
}