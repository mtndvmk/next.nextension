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

        [MenuItem("Nextension/SerializedFieldCheck/Run now", priority = 5, secondaryPriority = 0)]
        static void manualCheck()
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.Now;
            Debug.Log("[EnumArrayValueReloader] Begin");
            __check(ISerializedFieldCheckable.Flag.OnAssetImported);
            __check(ISerializedFieldCheckable.Flag.OnLoadOrRecompiled);
            __check(ISerializedFieldCheckable.Flag.OnPreprocessBuild);
            Debug.Log($"[EnumArrayValueReloader] Finished, process time: {(DateTimeOffset.Now - dateTimeOffset).Milliseconds}");
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

        private static void __check(ISerializedFieldCheckable.Flag flag)
        {
            try
            {
                __checkPrefabs(flag);
                __checkScriptableObjects(flag);
                __checkScenes(flag);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
        private static void __checkPrefabs(ISerializedFieldCheckable.Flag flag)
        {
            var guids = AssetDatabase.FindAssets("t:GameObject");
            float count = 0;
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                if (!NEditorAssetUtils.isValidAssetPath(path)) continue;
                var obj = NEditorAssetUtils.loadAssetAt<GameObject>(path);
                if (obj)
                {
                    EditorUtility.DisplayProgressBar(nameof(ISerializedFieldCheckable), $"Checking GameObject... {path}", count / guids.Length);
                    __checkSerializedChangableIn(obj, flag);
                    count++;
                }
            }
        }
        private static void __checkScriptableObjects(ISerializedFieldCheckable.Flag flag)
        {
            var guids = AssetDatabase.FindAssets("t:ScriptableObject");
            float count = 0;
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                if (!NEditorAssetUtils.isValidAssetPath(path)) continue;
                var so = NEditorAssetUtils.loadAssetAt<ScriptableObject>(path);
                if (so)
                {
                    EditorUtility.DisplayProgressBar(nameof(ISerializedFieldCheckable), $"Checking ScriptableObject... {path}", count / guids.Length);
                    if (__checkObject(so, so, flag))
                    {
                        NAssetUtils.saveAsset(so);
                    }
                    count++;
                }
            }
        }
        private static void __checkScenes(ISerializedFieldCheckable.Flag flag)
        {
            var guids = AssetDatabase.FindAssets("t:Scene");
            var count = 0;
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                if (!NEditorAssetUtils.isValidAssetPath(path)) continue;
                var scene = EditorSceneManager.GetActiveScene();
                EditorUtility.DisplayProgressBar(nameof(ISerializedFieldCheckable), $"Checking Scenes... {path}", count / guids.Length);
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
                        Debug.Log($"[{nameof(ISerializedFieldCheckable)}] Saved scene... " + scene.path);
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
            foreach (var behaviour in behaviours)
            {
                isDirty |= __checkObject(go, behaviour, flag);
            }

            if (isDirty)
            {
                if (go.scene.name.isNullOrEmpty())
                {
                    NAssetUtils.saveAsset(go);
                    Debug.Log($"[{nameof(ISerializedFieldCheckable)}] Saved... " + go, go);
                }
                return true;
            }
            return false;
        }
        private static bool __checkObject(UnityEngine.Object origin, object obj, ISerializedFieldCheckable.Flag flag)
        {
            if (obj == null) return false;
            var objectType = obj.GetType();
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
                if (subObj == null) continue;
                if (subObj is ISerializedFieldCheckable val)
                {
                    if (val.onSerializedChanged(flag))
                    {
                        if (origin is GameObject go && !go.scene.name.isNullOrEmpty())
                        {
                            Debug.Log($"[{nameof(ISerializedFieldCheckable)}] Serialized of {fieldInfo.FieldType.Name} was changed at scene: {go.scene.name}, GameObject: {go}");
                        }
                        else
                        {
                            Debug.Log($"[{nameof(ISerializedFieldCheckable)}] Serialized of {fieldInfo.FieldType.Name} was changed at: {origin}", origin);
                        }
                        isDirty = true;
                    }
                    continue;
                }
                isDirty |= __checkObject(origin, subObj, flag);
            }
            return isDirty;
        }
        private static bool __canCheckType(Type objectType)
        {
            if (_ignoreCheckTypes.Contains(objectType))
            {
                return false;
            }
            if (objectType == typeof(string) || objectType.IsValueType || !objectType.IsClass || !NUtils.getCustomTypes().Contains(objectType))
            {
                _ignoreCheckTypes.Add(objectType);
                return false;
            }
            return true;
        }
    }
}
#endif