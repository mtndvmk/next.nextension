using System;
using UnityEngine;

namespace Nextension
{
    [Serializable]
    public class SceneRef
    {
        [SerializeField] internal string guid;
        public string GUID => guid;

#if UNITY_EDITOR
        public void setGUID(string guid)
        {
            this.guid = guid;
        }
#endif
        public override bool Equals(object obj)
        {
            if (obj is not SceneRef other) return false;
            return guid == other.guid;
        }
        public override int GetHashCode()
        {
            return guid.GetHashCode();
        }
    }
}
