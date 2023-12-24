using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace Nextension.NEditor
{
    internal static class EditorScriptableLoader
    {
        private static bool _isLoaded;
        private static EditorLoadableScriptableContainer _editorContainer;
        private static void loadEditorContainer()
        {
            _editorContainer = NAssetUtils.getObjectOnResources<EditorLoadableScriptableContainer>(EditorLoadableScriptableContainer.FileNameOnResource);
        }
        private static EditorLoadableScriptableContainer getEditorContainer()
        {
            if (!_isLoaded)
            {
                _isLoaded = true;
                loadEditorContainer();
            }
            return _editorContainer;
        }
        private static EditorLoadableScriptableContainer getOrCreateEditorContainer()
        {
            if (!_editorContainer)
            {
                loadEditorContainer();
            }
            if (!_editorContainer)
            {
                _editorContainer = NAssetUtils.createOnResource<EditorLoadableScriptableContainer>(EditorLoadableScriptableContainer.FileNameOnResource);
            }
            return _editorContainer;
        }

        internal static void scanAndReload(bool forceReload = false)
        {
            List<ScriptableObject> validScriptables = new List<ScriptableObject>();
            var guids = AssetDatabase.FindAssets("t:ScriptableObject");
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var item = NAssetUtils.loadAssetAt<ScriptableObject>(path);
                if (ScriptableLoader.isLoadable(item))
                {
                    validScriptables.Add(item);
                }
            }
            if (validScriptables.Count > 0)
            {
                var container = getOrCreateEditorContainer();
                bool isAdded = false;
                foreach (var scriptableObject in validScriptables.asSpan())
                {
                    isAdded |= container.add(scriptableObject);
                }
                if (isAdded || forceReload) container.reload();
            }
            else
            {
                getEditorContainer()?.reload();
            }
        }

        [MenuItem("Assets/Nextension/Add to LoadableScriptableContainer")]
        private static void manualImport()
        {
            if (Selection.objects == null || Selection.objects.Length == 0) return;
            List<ScriptableObject> validScriptables = new List<ScriptableObject>();
            foreach (var item in Selection.objects)
            {
                if (ScriptableLoader.isLoadable(item))
                {
                    validScriptables.Add(item as ScriptableObject);
                }
            }
            if (validScriptables.Count > 0)
            {
                var container = getOrCreateEditorContainer();
                int addCount = 0;
                foreach (var scriptableObject in validScriptables.asSpan())
                {
                    if (container.add(scriptableObject))
                    {
                        Debug.Log($"Added [{scriptableObject}] to LoadableScriptableContainer", scriptableObject);
                        addCount++;
                    }
                    else
                    {
                        Debug.LogWarning($"[{scriptableObject}] is added or is not LoadableScriptable", scriptableObject);
                    }
                   
                }

                Debug.Log($"Added {addCount} LoadableScriptables", container);
                if (addCount > 0) container.reload();
            }
        }
        [MenuItem("Assets/Nextension/Add to LoadableScriptableContainer", true)]
        private static bool validateManualImport()
        {
            if (Selection.objects == null || Selection.objects.Length == 0) return false;
            List<ScriptableObject> validScriptables = new List<ScriptableObject>();
            foreach (var item in Selection.objects)
            {
                if (ScriptableLoader.isLoadable(item))
                {
                    validScriptables.Add(item as ScriptableObject);
                }
            }
            return validScriptables.Count > 0;
        }

        public class OnLoadOrRecompiled : IErrorCheckable, IAssetImportedCallback
        {
            static void onLoadOrRecompiled()
            {
                loadEditorContainer();
                scanAndReload();
            }
            static async void onAssetImported(string path)
            {
                if (!path.EndsWith(NAssetUtils.SCRIPTABLE_OBJECT_EXTENSION))
                {
                    return;
                }
                var scriptableObject = NAssetUtils.loadAssetAt<ScriptableObject>(path);
                if (ScriptableLoader.isLoadable(scriptableObject))
                {
                    var container = getOrCreateEditorContainer();
                    if (!container)
                    {
                        Debug.LogError($"Missing {EditorLoadableScriptableContainer.FileNameOnResource} on Resource directory");
                        return;
                    }
                    await new NWaitFrame_Editor(1);
                    if (container.add(scriptableObject))
                    {
                        container.reload();
                    }
                }
            }
            static async void onAssetMoved(string path)
            {
                if (!path.EndsWith(NAssetUtils.SCRIPTABLE_OBJECT_EXTENSION))
                {
                    return;
                }
                var scriptableObject = NAssetUtils.loadAssetAt<ScriptableObject>(path);
                if (ScriptableLoader.isLoadable(scriptableObject))
                {
                    var container = getOrCreateEditorContainer();
                    if (!container)
                    {
                        Debug.LogError($"Missing {EditorLoadableScriptableContainer.FileNameOnResource} on Resource directory");
                        return;
                    }
                    await new NWaitFrame_Editor(1);
                    container.add(scriptableObject);
                    container.reload();
                }
            }
            static async void onAssetDeleted(string path)
            {
                if (!path.EndsWith(NAssetUtils.SCRIPTABLE_OBJECT_EXTENSION))
                {
                    return;
                }
                var editorName = EditorLoadableScriptableContainer.FileNameOnResource + NAssetUtils.SCRIPTABLE_OBJECT_EXTENSION;
                if (path.EndsWith(editorName))
                {
                    scanAndReload();
                    if (_editorContainer != null)
                    {
                        WarningTracker.trackError($"Did you delete `{editorName}`?, please don't do that!");
                    }
                    return;
                }

                if (getEditorContainer() == null)
                {
                    return;
                }

                var runtimeName = LoadableScriptableContainer.FileNameOnResource + NAssetUtils.SCRIPTABLE_OBJECT_EXTENSION;
                if (path.EndsWith(runtimeName) && !NAssetUtils.hasAssetAt(path))
                {
                    WarningTracker.trackError($"Did you delete `{runtimeName}`?, please don't do that!");
                }

                if (path.EndsWith(NAssetUtils.SCRIPTABLE_OBJECT_EXTENSION))
                {
                    await new NWaitFrame_Editor(1);
                    _editorContainer.reload();
                }
            }
        }
    }
}
#endif