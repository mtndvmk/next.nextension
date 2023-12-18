using System;
using UnityEngine;

namespace Nextension
{
    [Serializable]
    public class NonPreloadScriptable : IComparable<NonPreloadScriptable>
    {
        private const string DEFAULT_TYPE_NAME_SUFFIX = " Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
        [SerializeField] private string pathInResource;
        [SerializeField] private string typeName;

        public string PathInResource => pathInResource;

        public Type getScriptableType()
        {
            var assemblyQualifiedName = typeName;
            if (assemblyQualifiedName.EndsWith(','))
            {
                assemblyQualifiedName += DEFAULT_TYPE_NAME_SUFFIX;
            }
            return Type.GetType(assemblyQualifiedName);
        }

        public ScriptableObject getScriptableObject()
        {
            return NAssetUtils.getObjectOnResources<ScriptableObject>(pathInResource);
        }
        public int CompareTo(NonPreloadScriptable other)
        {
            if (other is null) return -1;
            if (this == other) return 0;
            return typeName.CompareTo(other.typeName);
        }

#if UNITY_EDITOR
        public void setup(Type type, string pathInResource)
        {
            this.pathInResource = pathInResource;
            typeName = type.AssemblyQualifiedName.Replace(DEFAULT_TYPE_NAME_SUFFIX, string.Empty);
        }
        public static bool isNonPreload(ScriptableObject scriptable)
        {
            if (!ScriptableLoader.isLoadable(scriptable, out var attribute))
            {
                return false;
            }

            if (attribute.isPreload)
            {
                return false;
            }

            if (!NAssetUtils.getPathInResource(scriptable, out _))
            {
                return false;
            }

            return true;
        }
#endif
    }
}
