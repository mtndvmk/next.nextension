using System;
using UnityEngine;

namespace Nextension
{
    [Serializable]
    public class SceneRef : ISerializedFieldCheckable
    {
        [SerializeField] internal string guid;
        [SerializeField] private string scenePath;
        public string GUID => guid;
        public string ScenePath => scenePath;

#if UNITY_EDITOR
        public void setGUID(string guid)
        {
            this.guid = guid;
            updateScenePath();
        }

        internal bool updateScenePath()
        {
            if (!string.IsNullOrEmpty(guid))
            {
                var scenePath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                if (scenePath.isNullOrEmpty())
                {
                    guid = string.Empty;
                    this.scenePath = string.Empty;
                    return true;
                }
                if (scenePath != this.scenePath)
                {
                    this.scenePath = scenePath;
                    return true;
                }
                return false;
            }
            else
            {
                if (!string.IsNullOrEmpty(scenePath))
                {
                    scenePath = string.Empty;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
#endif
        public override int GetHashCode()
        {
            return guid.GetHashCode();
        }

        public bool onSerializedChanged(ISerializedFieldCheckable.Flag flag)
        {
#if UNITY_EDITOR
            return updateScenePath();
#else
            return false;
#endif
        }
    }
}
