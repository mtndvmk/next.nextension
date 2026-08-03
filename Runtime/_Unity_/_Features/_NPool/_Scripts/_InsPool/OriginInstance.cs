using UnityEngine;

namespace Nextension
{
    public class OriginInstance : MonoBehaviour, IInsPoolable
    {
        public ulong Id => Pool.Id;

        public bool IsInPool { get; private set; } = true;
        public bool IsSharedPool => SharedInsPoolUtil.exists(Id);
        public IInsPool Pool { get; private set; }

        internal void setPool(IInsPool pool, bool isOrigin)
        {
#if UNITY_EDITOR
            if (TryGetComponent<INotAllowInsPool>(out var notSupport))
            {
                NDebug.LogError($"Found [INotAllowInsPool] in {gameObject}, removed OriginInstance", gameObject);
            }
            this.isOrigin = isOrigin;
            this.hideFlags = HideFlags.HideAndDontSave;
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
        private bool isOrigin { get; set; }

        [ContextMenu("Ping origin")]
        private void pingOrigin()
        {
            OriginInstance[] alls;
#if UNITY_6000_4_OR_NEWER
            alls = FindObjectsByType<OriginInstance>(FindObjectsInactive.Include);
#else
            alls = FindObjectsByType<OriginInstance>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#endif
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

