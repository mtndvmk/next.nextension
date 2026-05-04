using UnityEngine;

namespace Nextension
{
    public readonly struct UnityGeneric<T> where T : UnityEngine.Object
    {
        public readonly static bool IsComponent = typeof(T).IsSubclassOf(typeof(Component));
        public readonly static bool IsGameObject = typeof(T) == typeof(GameObject);
    }
}