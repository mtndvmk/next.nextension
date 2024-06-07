using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
namespace Nextension.NEditor
{
    internal class EditorSingletonScriptableContainer : ScriptableObject
    {
        internal const string FileNameOnResource = "AutoCreated/Editor/[AutoCreated][EditorSingletonScriptableContainer]";
        [SerializeField] private List<ScriptableObject> _editorSingletonScriptables;

        private void OnEnable()
        {
            hideFlags = HideFlags.NotEditable;
            reload(false);
        }
        private void OnValidate()
        {
            reload(false);
        }
        [ContextMenu("Scan and reload")]
        private void scanAndReload()
        {
            EditorSingletonScriptableLoader.scanAndReload(true);
        }
        private long _lastReloadTime;
        internal void clear()
        {
            _editorSingletonScriptables.Clear();
        }
        internal void reload(bool isDeleteIfEmpty = true)
        {
            var current = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (current - _lastReloadTime < 300)
            {
                return;
            }
            _lastReloadTime = current;

            bool hasChanged = false;
            _editorSingletonScriptables ??= new List<ScriptableObject>();
            var span = _editorSingletonScriptables.asSpan();
            int maxIndex = _editorSingletonScriptables.Count - 1;
            for (int i = maxIndex; i >= 0; i--)
            {
                var scriptable = span[i];
                if (scriptable == null)
                {
                    _editorSingletonScriptables.RemoveAt(i);
                    hasChanged = true;
                    continue;
                }
                if (!ScriptableLoader.isSingletonable(scriptable))
                {
                    _editorSingletonScriptables.RemoveAt(i);
                    hasChanged = true;
                    continue;
                }
            }

            if (maxIndex >= 0)
            {
                if (hasChanged)
                {
                    _editorSingletonScriptables.Sort((a, b) => a.name.CompareTo(b.name));
                    NAssetUtils.saveAsset(this);
                }

                foreach (var scriptable in _editorSingletonScriptables)
                {
                    ScriptableLoader.updateScriptable(scriptable);
                }
            }
            else
            {
                if (isDeleteIfEmpty)
                {
                    if (ScriptableLoader.getContainer() != null)
                    {
                        WarningTracker.trackWarning($"Delete... {ScriptableLoader.getContainer().name}", 1);
                        NAssetUtils.delete(ScriptableLoader.getContainer());
                        NAssetUtils.refresh();
                    }
                }
            }
        }
        internal bool add(ScriptableObject scriptableObject)
        {
            _editorSingletonScriptables ??= new List<ScriptableObject>();
            if (_editorSingletonScriptables.Contains(scriptableObject))
            {
                if (ScriptableLoader.contains(scriptableObject)) return false;
                return false;
            }
            _editorSingletonScriptables.Add(scriptableObject);
            _editorSingletonScriptables.Sort((a, b) => a.name.CompareTo(b.name));
            NAssetUtils.saveAsset(this);
            return true;
        }
    }
}
#endif