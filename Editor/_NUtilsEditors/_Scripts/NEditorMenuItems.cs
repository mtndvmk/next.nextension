using System;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Nextension.NEditor
{
    public class NEditorMenuItems
    {
        [MenuItem("Nextension/Project/Force save project")]
        public static void forceSaveProject()
        {
            var paths = AssetDatabase.GetAllAssetPaths();
            foreach (var p in paths)
            {
                try
                {
                    if (p.StartsWith("Assets"))
                    {
                        var @object = AssetDatabase.LoadMainAssetAtPath(p);
                        NEditorUtils.setDirty(@object);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            NEditorUtils.saveAssets();
        }
        
        [MenuItem("Nextension/Collider/Generate BoxCollider (Renderer)")]
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

        [MenuItem("Nextension/Json/Escape json")]
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
            for (int i = 0; i < len; i++)
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

            for (int i = 0; i < len; i++)
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
        /// Returns text player entered, or null if player cancelled the dialog.
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