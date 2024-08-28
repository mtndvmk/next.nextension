using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Nextension
{
    internal static class InstancesPoolUtil
    {
        public static GameObject getGameObject<T>(T instance) where T : Object
        {
            if (InstancesPool<T>.IS_GENERIC_OF_GAMEOBJECT)
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
            if (InstancesPool<T>.IS_GENERIC_OF_GAMEOBJECT)
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