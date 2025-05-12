using System.Collections.Generic;
using UnityEngine;

namespace Nextension
{
    public class OriginInstance : MonoBehaviour, IPoolable
    {
        public int Id => Pool.Id;

        public bool IsInPool { get; private set; } = true;
        public bool IsSharedPool => SharedInstancesPool.exists(Id);
        public IInstancePool Pool { get; private set; }


        internal void setPool(IInstancePool pool)
        {
#if UNITY_EDITOR
            if (TryGetComponent<INotAllowInstancesPool>(out var notSupport))
            {
                Debug.LogError($"Found [INotAllowInstancesPool] in {gameObject}, removed OriginInstance", gameObject);
            }
            isOrigin = true;
#endif
            Pool = pool;
        }

        void IPoolable.onSpawn()
        {
            IsInPool = false;
        }
        void IPoolable.onDespawn()
        {
            IsInPool = true;
        }

#if UNITY_EDITOR
        private bool isOrigin { get; set; }

        [ContextMenu("Ping origin")]
        private void pingOrigin()
        {
            var alls = FindObjectsByType<OriginInstance>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
            foreach (var o in alls)
            {
                if (o.isOrigin && o.Id == Id)
                {
                    UnityEditor.EditorGUIUtility.PingObject(o);
                    return;
                }
            }
        }
#endif
    }

    public interface INotAllowInstancesPool { }
}
