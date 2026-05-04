#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Nextension
{
    public class SerializedFieldCheckEditor : IOnCompiled, IOnAssetImported
    {
        private static string _editorPrefKey;
        private static HashSet<Type> _ignoreCheckTypes = new HashSet<Type>();

        private static string __getEditorPrefKey()
        {
            if (_editorPrefKey.isNullOrEmpty())
            {
                _editorPrefKey = $"_{Application.dataPath[..^7]}_{nameof(SerializedFieldCheckEditor)}";
            }
            return _editorPrefKey;
        }

        [MenuItem("Nextension/SerializedFieldCheck/Enable Auto Check", secondaryPriority = 1)]
        static void enableAutoCheck()
        {
            EditorPrefs.SetBool(__getEditorPrefKey(), true);
        }
        [MenuItem("Nextension/SerializedFieldCheck/Enable Auto Check", secondaryPriority = 1, validate = true)]
        static bool isDisableAutoCheck()
        {
            return !EditorPrefs.GetBool(__getEditorPrefKey(), false);
        }

        [MenuItem("Nextension/SerializedFieldCheck/Disable Auto Check", secondaryPriority = 2)]
        static void disableAutoCheck()
        {
            EditorPrefs.SetBool(__getEditorPrefKey(), false);
        }
        [MenuItem("Nextension/SerializedFieldCheck/Disable Auto Check", secondaryPriority = 2, validate = true)]
        static bool isEnableAutoCheck()
        {
            return EditorPrefs.GetBool(__getEditorPrefKey(), false);
        }

        [MenuItem("Nextension/SerializedFieldCheck/Check All", priority = 5, secondaryPriority = 0)]
        static void manualCheck()
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.Now;
            NDebug.Log("[SerializedFieldCheck] Begin");
            __check(ISerializedFieldCheckable.Flag.OnAssetImported);
            __check(ISerializedFieldCheckable.Flag.OnLoadOrRecompiled);
            __check(ISerializedFieldCheckable.Flag.OnPreprocessBuild);
            NDebug.Log($"[SerializedFieldCheck] Finished, process time: {(DateTimeOffset.Now - dateTimeOffset).Milliseconds}ms");
        }

        [MenuItem("Nextension/SerializedFieldCheck/Check selected items", priority = 5, secondaryPriority = 1)]
        static void manualCheck_SelectedItems()
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.Now;
            NDebug.Log("[SerializedFieldCheck] Begin (Selected Items)");

            var folders = new List<string>();
            var directGuids = new List<string>();
            foreach (var guid in Selection.assetGUIDs)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetDatabase.IsValidFolder(path))
                {
                    folders.Add(path);
                }
                else
                {
                    directGuids.Add(guid);
                }
            }

            if (folders.Count == 0 && directGuids.Count == 0)
            {
                NDebug.Log("[SerializedFieldCheck] Nothing selected to check.");
                return;
            }

            string[] folderArray = folders.ToArray();

            __check(ISerializedFieldCheckable.Flag.OnAssetImported, folderArray, directGuids);
            __check(ISerializedFieldCheckable.Flag.OnLoadOrRecompiled, folderArray, directGuids);
            __check(ISerializedFieldCheckable.Flag.OnPreprocessBuild, folderArray, directGuids);

            NDebug.Log($"[SerializedFieldCheck] Finished (Selected Items), process time: {(DateTimeOffset.Now - dateTimeOffset).Milliseconds}ms");
        }

        static void onPreprocessBuild()
        {
            if (!isEnableAutoCheck()) return;
            __check(ISerializedFieldCheckable.Flag.OnPreprocessBuild);
        }
        static void onLoadOrRecompiled()
        {
            if (!isEnableAutoCheck()) return;
            __check(ISerializedFieldCheckable.Flag.OnLoadOrRecompiled);
        }
        static void onPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (!isEnableAutoCheck()) return;
            __check(ISerializedFieldCheckable.Flag.OnAssetImported);
        }

        private static void __check(ISerializedFieldCheckable.Flag flag, string[] searchFolders = null, IReadOnlyList<string> directGuids = null)
        {
            try
            {
                __checkPrefabs(flag, searchFolders, directGuids);
                __checkScriptableObjects(flag, searchFolders, directGuids);
                __checkScenes(flag, searchFolders, directGuids);
            }
            catch (Exception ex)
            {
                NDebug.LogException(ex);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
        private static void __checkPrefabs(ISerializedFieldCheckable.Flag flag, string[] searchFolders = null, IReadOnlyList<string> directGuids = null)
        {
            string[] guids = (searchFolders != null && searchFolders.Length == 0) ? Array.Empty<string>() : AssetDatabase.FindAssets("t:GameObject", searchFolders);
            var guidSet = new HashSet<string>(guids);
            if (directGuids != null)
            {
                foreach (var g in directGuids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(g);
                    if (path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)) guidSet.Add(g);
                }
            }

            float count = 0;
            float total = guidSet.Count;
            foreach (var g in guidSet)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                if (!NEditorAssetUtils.isValidAssetPath(path)) continue;
                if (!path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)) continue;
                var obj = NEditorAssetUtils.loadAssetAt<GameObject>(path);
                if (obj)
                {
                    EditorUtility.DisplayProgressBar(nameof(ISerializedFieldCheckable), $"Checking GameObject... {path}", count / total);
                    __checkSerializedChangableIn(obj, flag);
                }
                count++;
            }
        }
        private static void __checkScriptableObjects(ISerializedFieldCheckable.Flag flag, string[] searchFolders = null, IReadOnlyList<string> directGuids = null)
        {
            string[] guids = (searchFolders != null && searchFolders.Length == 0) ? Array.Empty<string>() : AssetDatabase.FindAssets("t:ScriptableObject", searchFolders);
            var guidSet = new HashSet<string>(guids);
            if (directGuids != null)
            {
                foreach (var g in directGuids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(g);
                    if (path.EndsWith(".asset", StringComparison.OrdinalIgnoreCase)) guidSet.Add(g);
                }
            }

            float count = 0;
            float total = guidSet.Count;
            var tempList = new HashSet<object>();
            foreach (var g in guidSet)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                if (!NEditorAssetUtils.isValidAssetPath(path)) continue;
                var so = NEditorAssetUtils.loadAssetAt<ScriptableObject>(path);
                if (so)
                {
                    EditorUtility.DisplayProgressBar(nameof(ISerializedFieldCheckable), $"Checking ScriptableObject... {path}", count / total);
                    tempList.Clear();
                    if (__checkObject(so, so, flag, tempList))
                    {
                        NAssetUtils.saveAsset(so);
                    }
                }
                count++;
            }
        }
        private static void __checkScenes(ISerializedFieldCheckable.Flag flag, string[] searchFolders = null, IReadOnlyList<string> directGuids = null)
        {
            string[] guids = (searchFolders != null && searchFolders.Length == 0) ? Array.Empty<string>() : AssetDatabase.FindAssets("t:Scene", searchFolders);
            var guidSet = new HashSet<string>(guids);
            if (directGuids != null)
            {
                foreach (var g in directGuids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(g);
                    if (path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase)) guidSet.Add(g);
                }
            }

            float count = 0;
            float total = guidSet.Count;
            foreach (var g in guidSet)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                if (!NEditorAssetUtils.isValidAssetPath(path)) continue;
                var scene = EditorSceneManager.GetActiveScene();
                EditorUtility.DisplayProgressBar(nameof(ISerializedFieldCheckable), $"Checking Scenes... {path}", count / total);
                bool needClose = false;
                if (scene.path != path)
                {
                    scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                    needClose = true;
                }
                var allObjects = scene.GetRootGameObjects();
                foreach (var obj in allObjects)
                {
                    if (__checkSerializedChangableIn(obj, flag))
                    {
                        EditorSceneManager.MarkSceneDirty(scene);
                    }
                }
                if (needClose)
                {
                    if (scene.isDirty)
                    {
                        EditorSceneManager.SaveScene(scene);
                        NDebug.Log($"[{nameof(ISerializedFieldCheckable)}] Saved scene... " + scene.path);
                    }
                    EditorSceneManager.CloseScene(scene, true);
                }
                count++;
            }
        }
        private static bool __checkSerializedChangableIn(GameObject go, ISerializedFieldCheckable.Flag flag)
        {
            var behaviours = go.GetComponentsInChildren<MonoBehaviour>(true);
            bool isDirty = false;
            var tempList = new HashSet<object>();
            foreach (var behaviour in behaviours)
            {
                tempList.Clear();
                isDirty |= __checkObject(go, behaviour, flag, tempList);
            }

            if (isDirty)
            {
                if (go.scene.name.isNullOrEmpty())
                {
                    NAssetUtils.saveAsset(go);
                    NDebug.Log($"[{nameof(ISerializedFieldCheckable)}] Saved... " + go, go);
                }
                return true;
            }
            return false;
        }
        private static bool __checkObject(UnityEngine.Object origin, object obj, ISerializedFieldCheckable.Flag flag, HashSet<object> checkedList)
        {
            if (obj.isNull()) return false;
            int validFieldCount = 0;
            var isDirty = __checkObject(origin, obj, flag, checkedList, ref validFieldCount);
            if (validFieldCount == 0)
            {
                _ignoreCheckTypes.Add(obj.GetType());
            }
            return isDirty;
        }
        private static bool __checkObject(UnityEngine.Object origin, object obj, ISerializedFieldCheckable.Flag flag, HashSet<object> checkedList, ref int validFieldCount)
        {
            if (obj.isNull()) return false;
            var objectType = obj.GetType();
            if (checkedList.Contains(obj)) return false;
            if (!__canCheckType(objectType))
            {
                return false;
            }
            var fieldInfos = objectType.GetRuntimeFields();
            bool isDirty = false;
            foreach (var fieldInfo in fieldInfos)
            {
                if (fieldInfo.IsNotSerialized) continue;
                var subObj = fieldInfo.GetValue(obj);
                if (subObj.isNull()) continue;
                if (!subObj.GetType().isCustomType() && checkedList.Contains(subObj))
                {
                    continue;
                }
                if (subObj is ISerializedFieldCheckable val)
                {
                    validFieldCount++;
                    bool isChanged = val.onSerializedChanged(flag);
                    if (isChanged)
                    {
                        NAssetUtils.saveAsset(origin);
                        if (origin is GameObject go && !go.scene.name.isNullOrEmpty())
                        {
                            NDebug.Log($"[{nameof(ISerializedFieldCheckable)}] Serialized of {fieldInfo.FieldType.Name} was changed at scene: {go.scene.name}, GameObject: {go}", origin);
                        }
                        else
                        {
                            NDebug.Log($"[{nameof(ISerializedFieldCheckable)}] Serialized of {fieldInfo.FieldType.Name} was changed at: {origin}", origin);
                        }
                        isDirty = true;
                    }
                    continue;
                }

                checkedList.Add(obj);
                isDirty |= __checkObject(origin, subObj, flag, checkedList, ref validFieldCount);
            }
            return isDirty;
        }
        private static bool __canCheckType(Type objectType)
        {
            if (_ignoreCheckTypes.Contains(objectType))
            {
                return false;
            }
            if (!NUtils.isCustomType(objectType) || objectType.IsEnum)
            {
                _ignoreCheckTypes.Add(objectType);
                return false;
            }
            return true;
        }
    }
}
#endif
