using System;
using UnityEngine;

namespace Nextension
{
    public class OriginInstance : MonoBehaviour, IPoolable
    {
        [field: SerializeField] public int Id { get; private set; }
        [field: SerializeField] public bool IsUniquePool { get; private set; }

        public bool IsInPool { get; private set; } = true;

        internal void setPoolId(int id, bool isUniquePool)
        {
#if UNITY_EDITOR
            var notSupport = GetComponent<INotAllowInstancesPool>();
            if (notSupport != null)
            {
                Debug.LogError($"Found [INotAllowInstancesPool] in {gameObject}, removed OriginInstance", gameObject);
            }
            isOrigin = true;
#endif
            Id = id;
            IsUniquePool = isUniquePool;
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
