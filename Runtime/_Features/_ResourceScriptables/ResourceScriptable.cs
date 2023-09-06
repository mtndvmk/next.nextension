using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Nextension
{
    public abstract class ResourceScriptable : ScriptableObject
    {
#if UNITY_EDITOR
        protected virtual async void OnEnable()
        {
            await new NWaitUntil_Editor(() => NAssetUtils.isFile(this));
            addThis();
        }
        [ContextMenu("Add to ResourceScriptableTable")]
        protected void addThis()
        {
            ResourceScriptableTable.add(this);
        }

        [ContextMenu("Ping ResourceScriptableTable")]
        protected void pingTable()
        {
            ResourceScriptableTable.pingTable();
        }
        [ContextMenu("Ping this")]
        protected void pingThis()
        {
            EditorGUIUtility.PingObject(this);
        }
#endif
    }

    //[CreateAssetMenu(fileName = "...", menuName = "ResourceScriptable/...")]
    public abstract class ResourceScriptable<T> : ResourceScriptable where T : ScriptableObject
    {
        private static T _getter;
        public static T Getter
        {
            get
            {
                if (_getter == null)
                {
                    _getter = ResourceScriptableTable.get(typeof(T)) as T;
                }
                return _getter;
            }
        }
    }
}
