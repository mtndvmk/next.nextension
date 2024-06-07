using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;

namespace Nextension.NEditor
{
    internal static class EditorSingletonScriptableLoader
    {
        private static bool _isLoaded;
        private static EditorSingletonScriptableContainer _editorContainer;
        private static void loadEditorContainer()
        {
            _editorContainer = NAssetUtils.getMainObjectOnResources<EditorSingletonScriptableContainer>(EditorSingletonScriptableContainer.FileNameOnResource);
        }
        internal static EditorSingletonScriptableContainer getEditorContainer()
        {
            if (!_isLoaded)
            {
                _isLoaded = true;
                loadEditorContainer();
            }
            return _editorContainer;
        }
        private static EditorSingletonScriptableContainer getOrCreateEditorContainer()
        {
            if (!_editorContainer)
            {
                loadEditorContainer();
            }
            if (!_editorContainer)
            {
                _editorContainer = NAssetUtils.createOnResource<EditorSingletonScriptableContainer>(EditorSingletonScriptableContainer.FileNameOnResource);
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
                if (!path.Contains("Assets/")) continue;
                var item = NAssetUtils.loadAssetAt<ScriptableObject>(path);
                if (ScriptableLoader.isSingletonable(item))
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

        [MenuItem("Assets/Nextension/Add to SingletonScriptableContainer")]
        private static void manualImport()
        {
            if (Selection.objects == null || Selection.objects.Length == 0) return;
            List<ScriptableObject> validScriptables = new List<ScriptableObject>();
            foreach (var item in Selection.objects)
            {
                if (ScriptableLoader.isSingletonable(item))
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
                        Debug.Log($"Added [{scriptableObject}] to SingletonScriptableContainer", scriptableObject);
                        addCount++;
                    }
                    else
                    {
                        Debug.LogWarning($"[{scriptableObject}] is added or is not SingletonScriptable", scriptableObject);
                    }
                   
                }

                Debug.Log($"Added {addCount} SingletonScriptables", container);
                if (addCount > 0) container.reload();
            }
        }
        [MenuItem("Assets/Nextension/Add to SingletonScriptableContainer", true)]
        private static bool validateManualImport()
        {
            if (Selection.objects == null || Selection.objects.Length == 0) return false;
            List<ScriptableObject> validScriptables = new List<ScriptableObject>();
            foreach (var item in Selection.objects)
            {
                if (ScriptableLoader.isSingletonable(item))
                {
                    validScriptables.Add(item as ScriptableObject);
                }
            }
            return validScriptables.Count > 0;
        }

        public class OnLoadOrRecompiled : IErrorCheckable, IAssetImportedCallback
        {
            static async void onLoadOrRecompiled()
            {
                loadEditorContainer();
                int count = 2;
                while (count > 0)
                {
                    try
                    {
                        scanAndReload();
                        break;
                    }
                    catch (Exception e)
                    {
                        if (--count <= 0) 
                        {
                            Debug.LogException(e);
                            break;
                        }
                        else
                        {
                            Debug.LogWarning(e);
                            await new NWaitSecond_Editor(0.2f);
                        }
                    }
                }
            }
            static async void onAssetImported(string path)
            {
                if (!path.EndsWith(NAssetUtils.SCRIPTABLE_OBJECT_EXTENSION))
                {
                    return;
                }
                var scriptableObject = NAssetUtils.loadAssetAt<ScriptableObject>(path);
                if (ScriptableLoader.isSingletonable(scriptableObject))
                {
                    var container = getOrCreateEditorContainer();
                    if (!container)
                    {
                        Debug.LogError($"Missing {EditorSingletonScriptableContainer.FileNameOnResource} on Resource directory");
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
                if (ScriptableLoader.isSingletonable(scriptableObject))
                {
                    var container = getOrCreateEditorContainer();
                    if (!container)
                    {
                        Debug.LogError($"Missing {EditorSingletonScriptableContainer.FileNameOnResource} on Resource directory");
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
                var editorName = EditorSingletonScriptableContainer.FileNameOnResource + NAssetUtils.SCRIPTABLE_OBJECT_EXTENSION;
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

                var runtimeName = SingletonScriptableContainer.FileNameOnResource + NAssetUtils.SCRIPTABLE_OBJECT_EXTENSION;
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