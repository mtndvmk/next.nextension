using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Nextension.NEditor
{
    public class NEditorMenuItems
    {
        [MenuItem("Nextension/Project/Force save project/Selected items", priority = 1)]
        public static void forceSaveProject_SelectedItems()
        {
            var objs = Selection.objects;
            foreach (var o in objs)
            {
                NAssetUtils.saveAsset(o);
            }
            Debug.Log($"Saved {objs.Length} objects");
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
        [MenuItem("Nextension/UI/Stretch to parent for selected items %q")]
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
            foreach (var group in EnumIndex<BuildTargetGroup>.asReadOnlySpan())
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
            return !EditorUserBuildSettings.activeScriptCompilationDefines.Contains(NPoolLogFullStackTraceSymbol);
        }
        [MenuItem("Nextension/Project/Full Stack Trace for NPool Log/Disable")]
        public static void disableNPoolLogFullStackTrace()
        {
            foreach (var group in EnumIndex<BuildTargetGroup>.asReadOnlySpan())
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
            return EditorUserBuildSettings.activeScriptCompilationDefines.Contains(NPoolLogFullStackTraceSymbol);
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
                if (selection is not Texture texture) { continue; }

                var assetPath = AssetDatabase.GetAssetPath(texture);
                if (assetPath == null)
                {
                    Debug.LogWarning($"{texture} must be in asset directory", selection);
                    continue;
                }

                byte[] png;

                if (!texture.isReadable || texture is not Texture2D tex2d)
                {
                    var readableTex2d = NTextureUtils.cloneNonReadableUseBlit(texture);
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
            }
            return false;
        }
        [MenuItem("CONTEXT/Component/Open Debug Window")]
        private static void openDebugWindow(MenuCommand menuCommand)
        {
            ComponentDebugWindow.show(menuCommand.context as Component);
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