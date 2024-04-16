using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Nextension
{
    internal static class InstancesPoolUtil
    {
        private static Exception NOT_SUPPORT_EXCEPTION(Type type) => new Exception($"InstancesPool of {type} is not supported");
        public static GameObject getGameObject<T>(T prefab) where T : Object
        {
            if (prefab == null) throw new ArgumentNullException(nameof(prefab));
            if (prefab is GameObject go)
            {
                return go;
            }
            else if (prefab is Component com)
            {
                return com.gameObject;
            }
            else
            {
                throw NOT_SUPPORT_EXCEPTION(typeof(T));
            }
        }
        public static int computePoolId<T>(T target) where T : Object
        {
            return getGameObject(target).GetInstanceID();
        }
        public static T getInstanceFromGO<T>(GameObject go) where T : Object
        {
            if (InstancesPool<T>.IS_GENERIC_OF_GAMEOBJECT)
            {
                return go as T;
            }
            else
            {
                return go.TryGetComponent(out T com) ? com : throw NOT_SUPPORT_EXCEPTION(typeof(T));
            }
        }
    }
}