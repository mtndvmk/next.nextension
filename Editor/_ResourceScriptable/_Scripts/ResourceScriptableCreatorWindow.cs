using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;

namespace Nextension.NEditor
{
    public class ResourceScriptableCreatorWindow : EditorWindow
    {
        [MenuItem("Nextension/ResourceScriptableTable/AutoCreateOnResource Creator")]
        private static void open()
        {
            ResourceScriptableCreatorWindow window = (ResourceScriptableCreatorWindow)EditorWindow.GetWindow(typeof(ResourceScriptableCreatorWindow));
            window.titleContent = new GUIContent("AutoCreateOnResource Creator"); 
            window.Show();
        }

        void OnGUI()
        {
            var types = NUtils.getCustomTypes();
            foreach (var type in types)
            {
                var autoCreateAttr = type.GetCustomAttribute(typeof(AutoCreateOnResourceAttribute)) as AutoCreateOnResourceAttribute;
                if (autoCreateAttr != null)
                {
                    var fileName = autoCreateAttr.getFileName(type);
                    var scriptable = NEditorUtils.getScriptableOnResource(fileName);
                    if (scriptable)
                    {
                        if (GUILayout.Button("Ping " + type.Name))
                        {
                            EditorGUIUtility.PingObject(scriptable);
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("Create " + type.Name + " on resource"))
                        {
                            NEditorUtils.createScriptableOnResource(type, fileName);
                        }
                    }
                }
            }
        }
    }
}
