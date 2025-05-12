using UnityEngine;

namespace Nextension
{
    internal static class InstancesPoolUtil
    {
        public static GameObject getGameObject<T>(T instance) where T : Object
        {
            if (UnityGeneric<T>.IsGameObject)
            {
                return instance as GameObject;
            }
            else
            {
                return (instance as Component).gameObject;
            }
        }
        public static int computePoolId<T>(T prefab) where T : Object
        {
            return getGameObject(prefab).GetInstanceID();
        }
        public static T getInstanceFromGO<T>(GameObject go) where T : Object
        {
            if (UnityGeneric<T>.IsGameObject)
            {
                return go as T;
            }
            else
            {
                return go.GetComponent<T>();
            }
        }
    }
}