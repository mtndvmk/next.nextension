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
        public bool updateRef()
        {
            bool hasChanged = false;
            var fName = ResourceScriptable.GetType().FullName;
            if (FullNameType != fName)
            {
                hasChanged = true;
                FullNameType = fName;
            }

            var path = AssetDatabase.GetAssetPath(ResourceScriptable);
            if (path.EndsWith(NEditorUtils.SCRIPTABLE_OBJECT_EXTENSION))
            {
                path = path.Remove(path.Length - NEditorUtils.SCRIPTABLE_OBJECT_EXTENSION.Length);
            }
            int indexOf = path.IndexOf("/Resources/");
            path = path.Remove(0, indexOf + 11);

            if (Path != path)
            {
                Path = path;
                hasChanged = true;
            }
            return hasChanged;
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
