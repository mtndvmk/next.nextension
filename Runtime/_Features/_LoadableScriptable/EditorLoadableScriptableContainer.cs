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
            EditorScriptableLoader.scanAndReload();
        }
        internal void reload(bool isDeleteIfEmpty = true)
        {
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

                ScriptableLoader.updateContainer(scriptable);
            }

            if (maxIndex >= 0)
            {
                if (hasChanged)
                {
                    _editorLoadableScriptables.Sort((a, b) => a.name.CompareTo(b.name));
                    NAssetUtils.saveAsset(this);
                }
            }
            else
            {
                if (isDeleteIfEmpty)
                {
                    WarningTracker.trackWarning($"Delete... {this} and {ScriptableLoader.getContainer()}");
                    //NAssetUtils.delete(this);
                    NAssetUtils.delete(ScriptableLoader.getContainer());
                }
            }
        }
        internal bool add(ScriptableObject scriptableObject)
        {
            _editorLoadableScriptables ??= new List<ScriptableObject>();
            if (_editorLoadableScriptables.Contains(scriptableObject))
            {
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