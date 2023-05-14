using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Nextension
{
    [Serializable]
    internal class ResourceScriptableReference
    {
        [field: SerializeField, HideInInspector] public string Path { get; private set; }
        [field: SerializeField, HideInInspector] public string FullNameType { get; private set; }
#if UNITY_EDITOR
        [field: SerializeField] public ResourceScriptable ResourceScriptable { get; private set; }
        public void updateRef()
        {
            FullNameType = ResourceScriptable.GetType().FullName;
            Path = AssetDatabase.GetAssetPath(ResourceScriptable);
            if (Path.EndsWith(NEditorUtils.SCRIPTABLE_OBJECT_EXTENSION))
            {
                Path = Path.Remove(Path.Length - NEditorUtils.SCRIPTABLE_OBJECT_EXTENSION.Length);
            }
            int indexOf = Path.IndexOf("/Resources/");
            Path = Path.Remove(0, indexOf + 11);
        }
        public ResourceScriptableReference(ResourceScriptable resourceScriptable)
        {
            ResourceScriptable = resourceScriptable;
        }
#endif
        private ResourceScriptableReference()
        {

        }
    }
}
