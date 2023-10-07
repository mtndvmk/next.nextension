using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Nextension
{
    public class OriginInstance : MonoBehaviour
    {
        [field: SerializeField] public int Id { get; private set; }
        [field: SerializeField] public bool IsUniquePool { get; private set; }

        internal void setPoolId(int id, bool isUniquePool)
        {
            Id = id;
            IsUniquePool = isUniquePool;
#if UNITY_EDITOR
            isOrigin = true;
#endif
        }

        private void OnDestroy()
        {
            if (NStartRunner.IsPlaying)
            {
                Debug.LogWarning("Origin instance has been destroyed");
            }
        }

#if UNITY_EDITOR
        [NonSerialized] private bool isOrigin;

        [ContextMenu("Ping origin")]
        private void pingOrigin()
        {
            var alls = Object.FindObjectsOfType<OriginInstance>(true);
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
}
