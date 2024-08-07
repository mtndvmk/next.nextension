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
        [ContextMenu("Scan and reload")]
        private void scanAndReload()
        {
            NEditor.EditorSingletonScriptableLoader.scanAndReload();
        }
        [ContextMenu("Reset and reload")]
        private void resetAndReload()
        {
            _preloadScriptables.Clear();
            _nonPreloadScriptables.Clear();
            NEditor.EditorSingletonScriptableLoader.getEditorContainer().clear();
            scanAndReload();
        }
        internal void updateScriptable(ScriptableObject scriptable)
        {
            bool hasChanged = false;
            bool isPreload = !NonPreloadScriptable.isNonPreload(scriptable);

            _preloadScriptables ??= new List<ScriptableObject>();
            var preloadSpan = _preloadScriptables.asSpan();

            int preloadMaxIndex = _preloadScriptables.Count - 1;
            for (int i = preloadMaxIndex; i >= 0; i--)
            {
                var scriptableObject = preloadSpan[i];
                if (scriptableObject == scriptable)
                {
                    if (!isPreload)
                    {
                        _preloadScriptables.RemoveAt(i);
                        hasChanged |= true;
                    }
                    break;
                }
            }

            _nonPreloadScriptables ??= new List<NonPreloadScriptable>();
            var nonPreloadSpan = _nonPreloadScriptables.AsSpan();

            int nonPreloadMaxIndex = _nonPreloadScriptables.Count - 1;
            for (int i = nonPreloadMaxIndex; i >= 0; i--)
            {
                var nonScriptable = nonPreloadSpan[i];
                var type = nonScriptable.getScriptableType();
                if (type == null)
                {
                    _nonPreloadScriptables.RemoveAt(i);
                    hasChanged |= true;
                    continue;
                }
                if (type == scriptable.GetType())
                {
                    if (isPreload)
                    {
                        _nonPreloadScriptables.RemoveAt(i);
                        hasChanged |= true;
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
                    hasChanged |= true;
                }
            }
            else
            {
                var existItem = _nonPreloadScriptables.Find(item => item.getScriptableType() == scriptable.GetType());
                NAssetUtils.getPathInMainResources(scriptable, out var path);
                path = path.removeExtension();
                if (existItem == null)
                {
                    NonPreloadScriptable nonPreloadScriptable = new NonPreloadScriptable();
                    nonPreloadScriptable.setup(scriptable.GetType(), path);
                    _nonPreloadScriptables.Add(nonPreloadScriptable);
                    _nonPreloadScriptables.Sort();
                    hasChanged |= true;
                }
                else if (existItem.PathInResource != path)
                {
                    existItem.setup(scriptable.GetType(), path);
                    hasChanged |= true;
                }
            }

            if (hasChanged)
            {
                NAssetUtils.saveAsset(this);
            }
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
