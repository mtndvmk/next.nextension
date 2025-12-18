using UnityEngine;

namespace Nextension
{
    public class OriginInstance : MonoBehaviour, IInsPoolable
    {
        public int Id => Pool.Id;

        public bool IsInPool { get; private set; } = true;
        public bool IsSharedPool => SharedInsPoolUtil.exists(Id);
        public IInsPool Pool { get; private set; }

        internal void setPool(IInsPool pool, bool isOrigin)
        {
#if UNITY_EDITOR
            if (TryGetComponent<INotAllowInsPool>(out var notSupport))
            {
                Debug.LogError($"Found [INotAllowInsPool] in {gameObject}, removed OriginInstance", gameObject);
            }
            this.isOrigin = isOrigin;
#endif
            Pool = pool;
        }

        void IInsPoolable.onSpawn(bool isRoot)
        {
            IsInPool = false;
        }
        void IInsPoolable.onDespawn(bool isRoot)
        {
            IsInPool = true;
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (!NStartRunner.IsPlaying)
            {
                Debug.LogError($"OriginInstance can only be added at runtime, removed from {gameObject}", gameObject);
                DestroyImmediate(this);
                return;
            }
        }
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

    public interface INotAllowInsPool { }
}
