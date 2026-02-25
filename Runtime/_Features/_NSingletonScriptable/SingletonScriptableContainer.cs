using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nextension
{
    public class SingletonScriptableContainer : ScriptableObject
    {
        internal const string FileNameInResource = "AutoCreated/[AutoCreated][SingletonScriptableContainer]";

        [SerializeField] private List<ScriptableObject> _preloadScriptables;
        [SerializeField] private List<NonPreloadScriptable> _nonPreloadScriptables;

        internal Dictionary<Type, string> _nonPreloadScriptablePaths;

        private void loadNonPreloadScriptablePaths()
        {
            _nonPreloadScriptablePaths = new(_nonPreloadScriptables.Count);
            foreach (var nonPreloadScriptable in _nonPreloadScriptables.asSpan())
            {
                _nonPreloadScriptablePaths[nonPreloadScriptable.getScriptableType()] = nonPreloadScriptable.PathInResource;
            }
#if !UNITY_EDITOR
            _nonPreloadScriptables.Clear();
#endif
        }

        internal T get<T>() where T : ScriptableObject
        {
            var typeOfT = typeof(T);
            foreach (var scriptableObject in _preloadScriptables.asSpan())
            {
                if (scriptableObject.GetType() == typeOfT)
                {
                    return (T)scriptableObject;
                }
            }

            if (_nonPreloadScriptablePaths == null)
            {
                loadNonPreloadScriptablePaths();
            }

            if (_nonPreloadScriptablePaths.TryGetValue(typeOfT, out var path))
            {
                return NAssetUtils.getMainObjectInMainResources<T>(path);
            }
            return null;
        }

        #region Editor
#if UNITY_EDITOR
        private void OnEnable()
        {
            hideFlags = HideFlags.NotEditable;
        }
        [ContextMenu("Clear and reload")]
        private void hardReload()
        {
            NEditor.EditorSingletonScriptableLoader.scanAndReload(true);
        }
        internal void clearScriptableObjectList()
        {
            _preloadScriptables?.Clear();
            _nonPreloadScriptables?.Clear();
        }
        internal void removeNullScriptableObjects()
        {
            for (int i = _preloadScriptables.Count - 1; i >= 0; i--)
            {
                if (_preloadScriptables[i] == null)
                {
                    _preloadScriptables.RemoveAt(i);
                }
            }
            for (int i = _nonPreloadScriptables.Count - 1; i >= 0; i--)
            {
                if (_nonPreloadScriptables[i] == null ||
                    !_nonPreloadScriptables[i].getScriptableObject())
                {
                    _nonPreloadScriptables.RemoveAt(i);
                }
            }
        }
        internal void updateScriptable(ScriptableObject scriptable)
        {
            bool hasChanged = false;
            bool isPreload = !NonPreloadScriptable.isNonPreload(scriptable);

            _preloadScriptables ??= new List<ScriptableObject>();
            _nonPreloadScriptables ??= new List<NonPreloadScriptable>();

            var preloadSpan = _preloadScriptables.asSpan();

            int preloadMaxIndex = _preloadScriptables.Count - 1;
            for (int i = preloadMaxIndex; i >= 0; i--)
            {
                var scriptableObject = preloadSpan[i];
                if (NonPreloadScriptable.isNonPreload(scriptableObject))
                {
                    _preloadScriptables.RemoveAt(i);
                    hasChanged = true;
                    addNonPreloadScriptable(scriptable);
                    continue;
                }
                if (scriptableObject == scriptable)
                {
                    if (!isPreload)
                    {
                        _preloadScriptables.RemoveAt(i);
                        hasChanged = true;
                    }
                    break;
                }
            }

            var nonPreloadSpan = _nonPreloadScriptables.AsSpan();
            int nonPreloadMaxIndex = _nonPreloadScriptables.Count - 1;
            for (int i = nonPreloadMaxIndex; i >= 0; i--)
            {
                var nonScriptable = nonPreloadSpan[i];

                var tmpType = nonScriptable.getScriptableType();
                var tmpScriptableObject = nonScriptable.getScriptableObject();

                if (tmpType == null || tmpScriptableObject == null)
                {
                    _nonPreloadScriptables.RemoveAt(i);
                    hasChanged = true;
                    continue;
                }

                if (!NonPreloadScriptable.isNonPreload(tmpScriptableObject))
                {
                    _nonPreloadScriptables.RemoveAt(i);
                    _preloadScriptables.addIfNotPresent(tmpScriptableObject);
                    hasChanged = true;
                    continue;
                }
                if (tmpType == scriptable.GetType())
                {
                    if (isPreload)
                    {
                        _nonPreloadScriptables.RemoveAt(i);
                        hasChanged = true;
                    }
                    break;
                }
            }

            if (isPreload)
            {
                if (!_preloadScriptables.Contains(scriptable))
                {
                    _preloadScriptables.Add(scriptable);
                    _preloadScriptables.Sort((a, b) => a.name.CompareTo(b.name));
                    hasChanged = true;
                }
            }
            else
            {
                hasChanged |= addNonPreloadScriptable(scriptable);
            }

            if (hasChanged)
            {
                NAssetUtils.saveAsset(this);
            }
        }

        private bool addNonPreloadScriptable(ScriptableObject scriptable)
        {
            var existItem = _nonPreloadScriptables.Find(item => item.getScriptableType() == scriptable.GetType());
            NEditorAssetUtils.getPathInMainResources(scriptable, out var path);
            path = path.removeExtension();
            if (existItem == null)
            {
                NonPreloadScriptable nonPreloadScriptable = new NonPreloadScriptable();
                nonPreloadScriptable.setup(scriptable.GetType(), path);
                _nonPreloadScriptables.Add(nonPreloadScriptable);
                _nonPreloadScriptables.Sort();
                return true;
            }
            else if (existItem.PathInResource != path)
            {
                existItem.setup(scriptable.GetType(), path);
                return true;
            }
            return false;
        }

        internal bool contains(ScriptableObject scriptableObject)
        {
            bool isPreload = _preloadScriptables.Contains(scriptableObject);
            if (isPreload) { return true; }
            return _nonPreloadScriptables.FindIndex(nonLoadScriptable => nonLoadScriptable.getScriptableObject() == scriptableObject) >= 0;
        }
        internal bool isNonPreloadSingletonScriptable(Type type)
        {
            if (_nonPreloadScriptablePaths != null)
            {
                return _nonPreloadScriptablePaths.ContainsKey(type);
            }
            return _nonPreloadScriptables.FindIndex(nonLoadScriptable => nonLoadScriptable.getScriptableType() == type) >= 0;
        }
#endif
        #endregion
    }
}
