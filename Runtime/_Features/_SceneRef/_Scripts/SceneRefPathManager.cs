#if UNITY_EDITOR
using UnityEditor;
#endif

using System;
using UnityEngine;

namespace Nextension
{
    [AutoCreateInResource]
    public sealed class SceneRefPathManager : SingletonScriptableGettable<SceneRefPathManager>, IErrorCheckable
    {
        [Serializable]
        public struct SceneRefPath
        {
            [Serializable]
            internal class List : AbsBListGenericComparable<SceneRefPath, string>
            {
                protected override string getCompareKeyFromValue(SceneRefPath item)
                {
                    return item.guid;
                }
            }
            public string guid;
            public string path;
            internal void setPath(string path) => this.path = path;
        }


        [SerializeField] private SceneRefPath.List _sceneRefPaths = new();

#if UNITY_EDITOR
        private void OnEnable()
        {
            hideFlags = HideFlags.NotEditable;
            refresh();
        }
        public void addOrUpdate(string guid)
        {
            var sceneAsset = NAssetUtils.loadMainAssetFromGUID(guid, out var path) as SceneAsset;
            if (sceneAsset == null)
            {
                Debug.LogError($"Can't load sceneAsset from [guid: {guid}, path: {path}]");
                return;
            }

            var index = _sceneRefPaths.bFindIndex(guid);
            if (index >= 0)
            {
                if (_sceneRefPaths[index].path != path)
                {
                    _sceneRefPaths[index].setPath(path);
                    NAssetUtils.saveAsset(this);
                }
                return;
            }
            else
            {
                _sceneRefPaths.addAndSort(new SceneRefPath()
                {
                    guid = guid,
                    path = path
                });
                NAssetUtils.saveAsset(this);
            }
        }
        internal void refresh()
        {
            bool isDirty = false;
            for (int i = _sceneRefPaths.Count - 1; i >= 0; i--)
            {
                var guid = _sceneRefPaths[i].guid;
                if (string.IsNullOrEmpty(guid))
                {
                    _sceneRefPaths.removeAt(i);
                    isDirty = true;
                    continue;
                }

                var sceneAsset = NAssetUtils.loadMainAssetFromGUID(guid, out var path) as SceneAsset;
                if (string.IsNullOrEmpty(path) || sceneAsset == null)
                {
                    _sceneRefPaths.removeAt(i);
                    isDirty = true;
                    continue;
                }

                if (_sceneRefPaths[i].path != path)
                {
                    _sceneRefPaths[i].setPath(path);
                    isDirty = true;
                    continue;
                }
            }
            if (isDirty)
            {
                NAssetUtils.saveAsset(this, true);
            }
        }
        static void onPreprocessBuild()
        {
            Getter.refresh();
        }
#endif
        public string getScenePath(SceneRef sceneRef)
        {
            if (string.IsNullOrEmpty(sceneRef.guid))
            {
                throw new Exception("GUID is null or empty");
            }
#if UNITY_EDITOR
            if (!NStartRunner.IsPlaying)
            {
                refresh();
            }
#endif
            var refPath = _sceneRefPaths.bFind(sceneRef.guid);
            return refPath.path;
        }
    }
}
