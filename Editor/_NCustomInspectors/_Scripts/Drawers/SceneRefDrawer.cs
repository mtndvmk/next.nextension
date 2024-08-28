using UnityEditor;
using UnityEngine;

namespace Nextension.NEditor
{
    [CustomPropertyDrawer(typeof(SceneRef))]
    public class SceneRefDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var sceneRef = NEditorHelper.getValue<SceneRef>(property);
            bool isDirty = false;
            string guid = sceneRef.GUID;
            SceneAsset sceneAsset;

            if (string.IsNullOrEmpty(guid))
            {
                sceneAsset = null;
            }
            else
            {
                var scenePath = AssetDatabase.GUIDToAssetPath(guid);

                if (scenePath == null)
                {
                    sceneAsset = null;
                    guid = string.Empty;
                    isDirty = true;
                }
                else
                {
                    sceneAsset = AssetDatabase.LoadMainAssetAtPath(scenePath) as SceneAsset;
                    if (sceneAsset == null)
                    {
                        guid = string.Empty;
                        isDirty = true;
                    }
                }
            }

            var guiContent = new GUIContent(label.text);
            var newAsset = EditorGUI.ObjectField(position, guiContent, sceneAsset, typeof(SceneAsset), false);
            if (newAsset != sceneAsset)
            {
                guid = NAssetUtils.getGUID(newAsset);
                isDirty = true;
            }

            if (isDirty)
            {
                sceneRef.setGUID(guid);
                NAssetUtils.setDirty(property.serializedObject.targetObject);
                SceneRefPathManager.Instance.addOrUpdate(guid);
            }
        }
    }
}
