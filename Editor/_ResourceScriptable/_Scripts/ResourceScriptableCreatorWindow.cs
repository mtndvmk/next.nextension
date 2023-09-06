using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Nextension.NEditor
{
    public class ResourceScriptableCreatorWindow : EditorWindow
    {
        [MenuItem("Nextension/ResourceScriptableTable/AutoCreateOnResource Creator")]
        private static void open()
        {
            ResourceScriptableCreatorWindow window = GetWindow<ResourceScriptableCreatorWindow>();
            window.titleContent = new GUIContent("AutoCreateOnResource Creator"); 
            window.Show();
        }

        void OnGUI()
        {
            var types = NUtils.getCustomTypes();
            foreach (var type in types)
            {
                var autoCreateAttr = type.GetCustomAttribute<AutoCreateOnResourceAttribute>();
                if (autoCreateAttr != null)
                {
                    if (!AutoCreateOnResourceAttribute.checkValid(type))
                    {
                        continue;
                    }
                    var fileName = autoCreateAttr.getFileName(type);
                    var scriptable = NUnityResourcesUtils.getObjectOnMainResource<ScriptableObject>(fileName);
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
                            NAssetUtils.createScriptableOnResource(type, fileName);
                        }
                    }
                }
            }
        }
    }
}
