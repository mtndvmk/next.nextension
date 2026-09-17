using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Nextension.NEditor
{
    public class NEditorMenuItems
    {
        [MenuItem("Assets/Nextension/Save selected items")]
        public static void forceSaveProject_SelectedItems()
        {
            var objs = Selection.objects;
            int count = 0;
            var savedPrefabPaths = new HashSet<string>();
            var savedScenes = new HashSet<string>();
            foreach (var o in objs)
            {
                if (!o) continue;
                var path = AssetDatabase.GetAssetPath(o);
                if (!string.IsNullOrEmpty(path) && AssetDatabase.IsValidFolder(path))
                {
                    var guids = AssetDatabase.FindAssets("", new[] { path });
                    foreach (var guid in guids)
                    {
                        var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                        if (AssetDatabase.IsValidFolder(assetPath)) continue;
                        // For .asset files with sub-assets (AddObjectToAsset), saving main alone doesn't persist sub-asset dirties.
                        // Load all to ensure every sub-asset is dirtied. For other files (prefab, etc.) just save main.
                        if (assetPath.EndsWith(".asset", StringComparison.OrdinalIgnoreCase))
                        {
                            var subAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                            if (subAssets != null && subAssets.Length > 0)
                            {
                                foreach (var asset in subAssets)
                                {
                                    if (asset == null) continue;
                                    if (asset.hideFlags == HideFlags.HideInHierarchy) continue;
                                    // LoadAll includes the main asset + sub-assets; skip null but save all visible ones
                                    // For .asset sub-assets they share same path, so dirtying each ensures file is written
                                    NAssetUtils.setDirty(asset);
                                    count++;
                                }
                                AssetDatabase.SaveAssetIfDirty(new GUID(guid));
                                continue;
                            }
                        }
                        var mainAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
                        if (mainAsset != null)
                        {
                            NAssetUtils.saveAsset(mainAsset, true);
                            count++;
                        }
                    }
                }
                else
                {
                    // Persistent asset (including sub-asset, prefab's child, ScriptableObject inside .asset)
                    if (EditorUtility.IsPersistent(o))
                    {
                        NAssetUtils.saveAsset(o, true);
                        count++;
                    }
                    else
                    {
                        GameObject go = null;
                        if (o is GameObject g) go = g;
                        else if (o is Component c) go = c.gameObject;

                        if (go != null)
                        {
                            var prefabStage = PrefabStageUtility.GetPrefabStage(go);
                            if (prefabStage != null)
                            {
                                // Object lives inside prefab asset opened in Prefab Stage (not in scene)
                                EditorUtility.SetDirty(o);
                                if (prefabStage.prefabContentsRoot != null)
                                    EditorUtility.SetDirty(prefabStage.prefabContentsRoot);
                                EditorSceneManager.MarkSceneDirty(prefabStage.scene);
                                if (!savedPrefabPaths.Contains(prefabStage.assetPath))
                                {
                                    PrefabUtility.SaveAsPrefabAsset(prefabStage.prefabContentsRoot, prefabStage.assetPath);
                                    savedPrefabPaths.Add(prefabStage.assetPath);
                                }
                                count++;
                                continue;
                            }

                            // Regular object in scene or prefab asset preview (scene.name == "" means asset preview, not a scene)
                            if (go.scene.IsValid() && !string.IsNullOrEmpty(go.scene.name))
                            {
                                EditorUtility.SetDirty(o);
                                var scene = go.scene;
                                EditorSceneManager.MarkSceneDirty(scene);
                                var scenePath = scene.path;
                                if (!string.IsNullOrEmpty(scenePath) && !savedScenes.Contains(scenePath))
                                {
                                    EditorSceneManager.SaveScene(scene);
                                    savedScenes.Add(scenePath);
                                }
                                count++;
                                continue;
                            }

                            // Fallback for preview / no valid scene (e.g., prefab asset child retrieved via LoadAsset but IsPersistent was false due to preview)
                            // Try to resolve prefab asset path via PrefabUtility
                            var prefabAssetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go);
                            if (!string.IsNullOrEmpty(prefabAssetPath))
                            {
                                EditorUtility.SetDirty(o);
                                if (!savedPrefabPaths.Contains(prefabAssetPath))
                                {
                                    var prefabRoot = PrefabUtility.GetCorrespondingObjectFromSource(go);
                                    if (prefabRoot != null) NAssetUtils.saveAsset(prefabRoot, true);
                                    savedPrefabPaths.Add(prefabAssetPath);
                                }
                                count++;
                                continue;
                            }
                        }

                        // Fallback: non-persistent ScriptableObject or other
                        NAssetUtils.saveAsset(o, true);
                        count++;
                    }
                }
            }
            Debug.Log($"Saved {count} objects");
        }
        [MenuItem("Nextension/Project/Force save project/*.asset")]
        public static void forceSaveProject_Asset()
        {
            var paths = AssetDatabase.GetAllAssetPaths();
            int count = 0;
            foreach (var p in paths)
            {
                try
                {
                    if (p.StartsWith("Assets") && p.EndsWith(".asset"))
                    {
                        var @object = AssetDatabase.LoadMainAssetAtPath(p);
                        if (@object != null)
                        {
                            NAssetUtils.setDirty(@object);
                            count++;
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            NAssetUtils.saveAssets();
            Debug.Log($"Saved {count} objects");
        }
        [MenuItem("Nextension/Project/Force save project/*.prefab")]
        public static void forceSaveProject_Prefab()
        {
            var paths = AssetDatabase.GetAllAssetPaths();
            int count = 0;
            foreach (var p in paths)
            {
                try
                {
                    if (p.StartsWith("Assets") && p.EndsWith(".prefab"))
                    {
                        var @object = AssetDatabase.LoadMainAssetAtPath(p);
                        if (@object != null)
                        {
                            NAssetUtils.setDirty(@object);
                            count++;
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            NAssetUtils.saveAssets();
            Debug.Log($"Saved {count} objects");
        }
        [MenuItem("Nextension/Project/Force save project/All")]
        public static void forceSaveProject_All()
        {
            var paths = AssetDatabase.GetAllAssetPaths();
            int count = 0;
            foreach (var p in paths)
            {
                try
                {
                    if (p.StartsWith("Assets"))
                    {
                        var @object = AssetDatabase.LoadMainAssetAtPath(p);
                        if (@object != null)
                        {
                            NAssetUtils.setDirty(@object);
                            count++;
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            NAssetUtils.saveAssets();
            Debug.Log($"Saved {count} objects");
        }

        [MenuItem("Nextension/Collider/Generate BoxCollider (Renderer)", priority = 2)]
        public static void addBoxCollider()
        {
            var sltObject = Selection.gameObjects;
            foreach (var g in sltObject)
            {
                g.addBoxCollider();
            }
        }

        [MenuItem("Nextension/Collider/Generate BoxCollider (UI)")]
        public static void addBoxColliderForUI()
        {
            var sltObject = Selection.gameObjects;
            foreach (var g in sltObject)
            {
                var rt = g.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.generateBoxCollider();
                }
            }
        }

        [MenuItem("Nextension/Json/Escape json", priority = 3)]
        public static void escapeJson()
        {
            string json = EditorInputDialog.Show("Escape json", "Please enter json text:", "");
            if (string.IsNullOrEmpty(json))
            {
                return;
            }
            var s = JavaScriptStringEncode(json, true);
            EditorGUIUtility.systemCopyBuffer = s;
            NoticeWindow.show("Copied to clipboard", s);
        }

        [MenuItem("Nextension/Json/Unescape json")]
        public static void unescapeJson()
        {
            string text = EditorInputDialog.Show("Unescape json", "Please enter string text:", "");
            if (string.IsNullOrEmpty(text))
            {
                return;
            }
            var s = Regex.Unescape(text);
            s = s.Trim('\"');
            EditorGUIUtility.systemCopyBuffer = s;
            NoticeWindow.show("Copied to clipboard", s);
        }

        public static string JavaScriptStringEncode(string value, bool addDoubleQuotes)
        {
            if (string.IsNullOrEmpty(value))
                return addDoubleQuotes ? "\"\"" : string.Empty;

            int len = value.Length;
            bool needEncode = false;
            char c;
            for (int i = 0; i < len; ++i)
            {
                c = value[i];

                if (c >= 0 && c <= 31 || c == 34 || c == 39 || c == 60 || c == 62 || c == 92)
                {
                    needEncode = true;
                    break;
                }
            }

            if (!needEncode)
                return addDoubleQuotes ? "\"" + value + "\"" : value;

            var sb = new System.Text.StringBuilder();
            if (addDoubleQuotes)
                sb.Append('"');

            for (int i = 0; i < len; ++i)
            {
                c = value[i];
                if (c >= 0 && c <= 7 || c == 11 || c >= 14 && c <= 31 || c == 39 || c == 60 || c == 62)
                    sb.AppendFormat("\\u{0:x4}", (int)c);
                else switch ((int)c)
                    {
                        case 8:
                            sb.Append("\\b");
                            break;

                        case 9:
                            sb.Append("\\t");
                            break;

                        case 10:
                            sb.Append("\\n");
                            break;

                        case 12:
                            sb.Append("\\f");
                            break;

                        case 13:
                            sb.Append("\\r");
                            break;

                        case 34:
                            sb.Append("\\\"");
                            break;

                        case 92:
                            sb.Append("\\\\");
                            break;

                        default:
                            sb.Append(c);
                            break;
                    }
            }

            if (addDoubleQuotes)
                sb.Append('"');

            return sb.ToString();
        }

        [MenuItem("Nextension/UI/Anchor to parent for selected items", priority = 4)]
        public static void anchorToParentForSelected()
        {
            var objs = Selection.objects;
            foreach (var o in objs)
            {
                if (o is GameObject go)
                {
                    var rectTf = go.rectTransform();
                    if (rectTf)
                    {
                        Undo.RecordObject(rectTf, "Anchor to parent");
                        rectTf.anchorToParent();
                        NAssetUtils.setDirty(rectTf);
                    }
                }
            }
        }
        [MenuItem("Nextension/UI/Stretch to parent for selected items")]
        public static void stretchToParentForSelected()
        {
            var objs = Selection.objects;
            foreach (var o in objs)
            {
                if (o is GameObject go)
                {
                    var rectTf = go.rectTransform();
                    if (rectTf)
                    {
                        Undo.RecordObject(rectTf, "Stretch to parent");
                        rectTf.stretchToParent();
                        NAssetUtils.setDirty(rectTf);
                    }
                }
            }
        }
        [MenuItem("CONTEXT/RectTransform/Anchor to parent")]
        public static void anchorToParent(MenuCommand menuCommand)
        {
            var rectTf = menuCommand.context as RectTransform;
            Undo.RecordObject(rectTf, "Anchor to parent");
            rectTf.anchorToParent();
            NAssetUtils.setDirty(rectTf);
        }
        [MenuItem("CONTEXT/RectTransform/Stretch to parent")]
        public static void stretchToParent(MenuCommand menuCommand)
        {
            var rectTf = menuCommand.context as RectTransform;
            Undo.RecordObject(rectTf, "Stretch to parent");
            rectTf.stretchToParent();
            NAssetUtils.setDirty(rectTf);
        }

        private const string NPoolLogFullStackTraceSymbol = "NPOOL_TRACKING_PRINT_STACK_TRACE";

        [MenuItem("Nextension/Project/Full Stack Trace for NPool Log/Enable", priority = 0)]
        public static void enableNPoolLogFullStackTrace()
        {
            foreach (var group in EnumIndex<BuildTargetGroup>.asSpan())
            {
                try
                {
                    var buildTarget = UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(group);
                    PlayerSettings.GetScriptingDefineSymbols(buildTarget, out var defines);
                    if (defines.Contains(NPoolLogFullStackTraceSymbol)) continue;
                    defines = defines.add(NPoolLogFullStackTraceSymbol);
                    PlayerSettings.SetScriptingDefineSymbols(buildTarget, defines);
                }
                catch (Exception) { }
            }

            AssetDatabase.SaveAssets();
        }
        [MenuItem("Nextension/Project/Full Stack Trace for NPool Log/Enable", true)]
        private static bool validateEnableNPoolLogFullStackTrace()
        {
            var isEnabled = EditorUserBuildSettings.activeScriptCompilationDefines.Contains(NPoolLogFullStackTraceSymbol);
            Menu.SetChecked("Nextension/Project/Full Stack Trace for NPool Log/Enable", isEnabled);
            return !isEnabled;
        }
        [MenuItem("Nextension/Project/Full Stack Trace for NPool Log/Disable")]
        public static void disableNPoolLogFullStackTrace()
        {
            foreach (var group in EnumIndex<BuildTargetGroup>.asSpan())
            {
                try
                {
                    var buildTarget = UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(group);
                    PlayerSettings.GetScriptingDefineSymbols(buildTarget, out var defines);
                    if (!defines.Contains(NPoolLogFullStackTraceSymbol)) continue;
                    defines = defines.remove(NPoolLogFullStackTraceSymbol);
                    PlayerSettings.SetScriptingDefineSymbols(buildTarget, defines);
                }
                catch (Exception) { }
            }

            AssetDatabase.SaveAssets();
        }
        [MenuItem("Nextension/Project/Full Stack Trace for NPool Log/Disable", true)]
        private static bool validateDisableNPoolLogFullStackTrace()
        {
            var isEnabled = EditorUserBuildSettings.activeScriptCompilationDefines.Contains(NPoolLogFullStackTraceSymbol);
            Menu.SetChecked("Nextension/Project/Full Stack Trace for NPool Log/Disable", !isEnabled);
            return isEnabled;
        }

        [MenuItem("Assets/Nextension/Export to PNG")]
        private static void exportTexture2DToPng()
        {
            var selections = Selection.objects;
            if (selections == null || selections.Length == 0)
            {
                return;
            }
            for (int i = 0; i < selections.Length; i++)
            {
                var selection = selections[i];
                Rect? rect = null;
                if (selection is not Texture texture)
                {
                    if (selection is Sprite sprite)
                    {
                        texture = sprite.texture;
                        rect = sprite.rect;
                    }
                    else
                    {
                        continue;
                    }
                }

                var assetPath = AssetDatabase.GetAssetPath(texture);
                if (assetPath == null)
                {
                    Debug.LogWarning($"{texture} must be in asset directory", selection);
                    continue;
                }

                byte[] png;

                if (!texture.isReadable || texture is not Texture2D tex2d)
                {
                    var readableTex2d = NTextureUtils.cloneNonReadableUseBlit(texture, rect ?? new Rect(0, 0, texture.width, texture.height));
                    png = readableTex2d.EncodeToPNG();
                    UnityEngine.Object.DestroyImmediate(readableTex2d);
                }
                else
                {
                    png = tex2d.EncodeToPNG();
                }

                var filename = $"{Path.GetFileNameWithoutExtension(assetPath)}.png";
                var pngPath = Path.Combine(Directory.GetParent(assetPath).FullName, filename);
                int index = 0;
                while (File.Exists(pngPath))
                {
                    filename = $"{Path.GetFileNameWithoutExtension(assetPath)}_{index++}.png";
                    pngPath = Path.Combine(Directory.GetParent(assetPath).FullName, filename);
                }

                File.WriteAllBytes(pngPath, png);

                if (NEditorAssetUtils.tryLoadAssetAt<Texture2D>(pngPath, out var newAsset))
                {
                    Debug.Log($"Exported texture {texture.name} to {filename}", newAsset);
                }
                else
                {
                    Debug.Log($"Exported texture {texture.name} to {filename} at {pngPath}");
                }
            }
            AssetDatabase.Refresh();
        }
        [MenuItem("Assets/Nextension/Export to PNG", true)]
        private static bool checkExportTexture2DToPng()
        {
            foreach (var obj in Selection.objects)
            {
                if (obj is Texture) return true;
                if (obj is Sprite) return true;
            }
            return false;
        }
        [MenuItem("CONTEXT/Component/Open Debug Window")]
        private static void openDebugWindow(MenuCommand menuCommand)
        {
            ComponentDebugWindow.show(menuCommand.context as Component);
        }

        [InitializeOnLoadMethod]
        private static void registerFinishedDefaultHeaderGUI()
        {
            Editor.finishedDefaultHeaderGUI += __drawHeaderGUI;
        }

        private static void __drawHeaderGUI(Editor editor)
        {
            var selectedObjects = editor.targets;
            if (selectedObjects.Length == 0)
            {
                return;
            }

            bool hasRectTransform = false;
            foreach (var obj in selectedObjects)
            {
                if (obj is GameObject go && go.GetComponent<RectTransform>() != null)
                {
                    hasRectTransform = true;
                    break;
                }
            }

            if (hasRectTransform)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Anchor to parent"))
                {
                    foreach (var obj in selectedObjects)
                    {
                        if (obj is GameObject go && go.GetComponent<RectTransform>() != null)
                        {
                            var rectTf = go.transform.asRectTransform();
                            rectTf.anchorToParent();
                            Undo.RecordObject(rectTf, "Anchor to parent");
                            NAssetUtils.setDirty(rectTf);
                        }
                    }
                }
                if (GUILayout.Button("Median anchor X"))
                {
                    foreach (var obj in selectedObjects)
                    {
                        if (obj is GameObject go && go.GetComponent<RectTransform>() != null)
                        {
                            var rectTf = go.transform.asRectTransform();
                            var anchorMin = rectTf.anchorMin;
                            var anchorMax = rectTf.anchorMax;
                            anchorMin.x = anchorMax.x = (anchorMin.x + anchorMax.x) / 2;
                            Undo.RecordObject(rectTf, "Median anchor X");
                            rectTf.setAnchorsWithoutChange(anchorMin, anchorMax);
                            NAssetUtils.setDirty(rectTf);
                        }
                    }
                }
                if (GUILayout.Button("Median anchor Y"))
                {
                    foreach (var obj in selectedObjects)
                    {
                        if (obj is GameObject go && go.GetComponent<RectTransform>() != null)
                        {
                            var rectTf = go.transform.asRectTransform();
                            var anchorMin = rectTf.anchorMin;
                            var anchorMax = rectTf.anchorMax;
                            anchorMin.y = anchorMax.y = (anchorMin.y + anchorMax.y) / 2;
                            Undo.RecordObject(rectTf, "Median anchor Y");
                            rectTf.setAnchorsWithoutChange(anchorMin, anchorMax);
                            NAssetUtils.setDirty(rectTf);
                        }
                    }
                }
                if (GUILayout.Button("Stretch to parent"))
                {
                    foreach (var obj in selectedObjects)
                    {
                        if (obj is GameObject go && go.GetComponent<RectTransform>() != null)
                        {
                            var rectTf = go.transform.asRectTransform();
                            rectTf.stretchToParent();
                            Undo.RecordObject(rectTf, "Stretch to parent");
                            NAssetUtils.setDirty(rectTf);
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }
        }
    }

    public class EditorInputDialog : EditorWindow
    {
        string description, inputText;
        string okButton, cancelButton;
        bool initializedPosition = false;
        Action onOKButton;

        bool shouldClose = false;

        #region OnGUI()

        void OnGUI()
        {
            // Check if Esc/Return have been pressed
            var e = Event.current;
            if (e.type == EventType.KeyDown)
            {
                switch (e.keyCode)
                {
                    // Escape pressed
                    case KeyCode.Escape:
                        shouldClose = true;
                        break;

                    // Enter pressed
                    case KeyCode.Return:
                    case KeyCode.KeypadEnter:
                        onOKButton?.Invoke();
                        shouldClose = true;
                        break;
                }
            }

            if (shouldClose)
            {
                // Close this dialog
                Close();
                //return;
            }

            // Draw our control
            var rect = EditorGUILayout.BeginVertical();

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField(description);

            EditorGUILayout.Space(8);
            GUI.SetNextControlName("inText");
            inputText = EditorGUILayout.TextField("", inputText);
            GUI.FocusControl("inText"); // Focus text field
            EditorGUILayout.Space(12);

            // Draw OK / Cancel buttons
            var r = EditorGUILayout.GetControlRect();
            r.width /= 2;
            if (GUI.Button(r, okButton))
            {
                onOKButton?.Invoke();
                shouldClose = true;
            }

            r.x += r.width;
            if (GUI.Button(r, cancelButton))
            {
                inputText = null; // Cancel - delete inputText
                shouldClose = true;
            }

            EditorGUILayout.Space(8);
            EditorGUILayout.EndVertical();

            // Force change size of the window
            if (rect.width != 0 && minSize != rect.size)
            {
                minSize = maxSize = rect.size;
            }

            // Set dialog position next to mouse position
            if (!initializedPosition)
            {
                var mousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                position = new Rect(mousePos.x + 32, mousePos.y, position.width, position.height);
                initializedPosition = true;
            }
        }

        #endregion OnGUI()

        #region Show()

        /// <summary>
        /// Returns text player entered, or null if player canceled the dialog.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="inputText"></param>
        /// <param name="okButton"></param>
        /// <param name="cancelButton"></param>
        /// <returns></returns>
        public static string Show(string title, string description, string inputText, string okButton = "OK",
            string cancelButton = "Cancel")
        {
            string ret = null;
            //var window = EditorWindow.GetWindow<InputDialog>();
            var window = CreateInstance<EditorInputDialog>();
            window.titleContent = new GUIContent(title);
            window.description = description;
            window.inputText = inputText;
            window.okButton = okButton;
            window.cancelButton = cancelButton;
            window.onOKButton += () => ret = window.inputText;
            window.ShowModal();

            return ret;
        }

        #endregion Show()
    }
    public class NoticeWindow : EditorWindow
    {
        private string result;
        public static void show(string content, string result)
        {
            NoticeWindow window = NoticeWindow.GetWindow(typeof(NoticeWindow)) as NoticeWindow;
            window.titleContent = new GUIContent("Result");
            window.ShowNotification(new GUIContent(content));
            window.result = result;
        }

        private void OnGUI()
        {
            GUIStyle style = GUI.skin.textField;
            style.wordWrap = true;
            GUI.TextField(new Rect(10, 10, position.size.x - 20, position.size.y - 20), result, style);
        }
    }
}