using UnityEngine;

namespace Nextension
{
    public class OriginInstance : MonoBehaviour, IPoolable
    {
        [SerializeField] private int _id;
        public int Id => _id;

        public bool IsInPool { get; private set; } = true;
        public bool IsSharedPool => SharedInstancesPool.exists(Id);

        internal void setPoolId(int id)
        {
#if UNITY_EDITOR
            if (TryGetComponent<INotAllowInstancesPool>(out var notSupport))
            {
                Debug.LogError($"Found [INotAllowInstancesPool] in {gameObject}, removed OriginInstance", gameObject);
            }
            isOrigin = true;
#endif
            _id = id;
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
                if (o.isOrigin && o._id == _id)
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
