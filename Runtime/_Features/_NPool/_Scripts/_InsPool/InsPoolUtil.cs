using UnityEngine;

namespace Nextension
{
    internal static class InsPoolUtil
    {
        public static GameObject getGameObject<T>(T instance) where T : Object
        {
            if (UnityGeneric<T>.IsComponent)
            {
                return (instance as Component).gameObject;
            }
            else
            {
                return instance as GameObject;
            }
        }
        public static int computePoolId<T>(T prefab) where T : Object
        {
            return getGameObject(prefab).GetInstanceID();
        }
        public static T getInstanceFromGO<T>(GameObject go) where T : Object
        {
            if (UnityGeneric<T>.IsComponent)
            {
                return go.GetComponent<T>();
            }
            else
            {
                return go as T;
            }
        }
    }
}