using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
namespace Nextension.NEditor
{
    internal class EditorLoadableScriptableContainer : ScriptableObject
    {
        internal const string FileNameOnResource = "AutoCreated/Editor/[AutoCreated][EditorLoadableScriptableContainer]";
        [SerializeField] private List<ScriptableObject> _editorLoadableScriptables;

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
            EditorScriptableLoader.scanAndReload(true);
        }
        private long _lastReloadTime;
        internal void reload(bool isDeleteIfEmpty = true)
        {
            var current = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (current - _lastReloadTime < 300)
            {
                return;
            }
            _lastReloadTime = current;

            bool hasChanged = false;
            _editorLoadableScriptables ??= new List<ScriptableObject>();
            var span = _editorLoadableScriptables.asSpan();
            int maxIndex = _editorLoadableScriptables.Count - 1;
            for (int i = maxIndex; i >= 0; i--)
            {
                var scriptable = span[i];
                if (scriptable == null)
                {
                    _editorLoadableScriptables.RemoveAt(i);
                    hasChanged = true;
                    continue;
                }
                if (!ScriptableLoader.isLoadable(scriptable))
                {
                    _editorLoadableScriptables.RemoveAt(i);
                    hasChanged = true;
                    continue;
                }
            }

            if (maxIndex >= 0)
            {
                if (hasChanged)
                {
                    _editorLoadableScriptables.Sort((a, b) => a.name.CompareTo(b.name));
                    NAssetUtils.saveAsset(this);
                }

                foreach (var scriptable in _editorLoadableScriptables)
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
            _editorLoadableScriptables ??= new List<ScriptableObject>();
            if (_editorLoadableScriptables.Contains(scriptableObject))
            {
                if (ScriptableLoader.contains(scriptableObject)) return false;
                return false;
            }
            _editorLoadableScriptables.Add(scriptableObject);
            _editorLoadableScriptables.Sort((a, b) => a.name.CompareTo(b.name));
            NAssetUtils.saveAsset(this);
            return true;
        }
    }
}
#endif