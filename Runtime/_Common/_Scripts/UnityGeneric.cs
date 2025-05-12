using UnityEngine;

namespace Nextension
{
    public struct UnityGeneric<T>
    {
        internal readonly static bool IsGameObject = typeof(T).Equals(typeof(GameObject));
    }
}
