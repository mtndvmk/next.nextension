using UnityEditor;
using UnityEngine;

namespace Nextension.NEditor
{
    [CustomPropertyDrawer(typeof(SceneRefPathManager.SceneRefPath))]
    public class SceneRefPathDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var sceneRefPath = NEditorHelper.getValue<SceneRefPathManager.SceneRefPath>(property);
            string guid = sceneRefPath.guid;
            SceneAsset sceneAsset;
            string scenePath;

            if (string.IsNullOrEmpty(guid))
            {
                scenePath = string.Empty;
                sceneAsset = null;
            }
            else
            {
                scenePath = AssetDatabase.GUIDToAssetPath(guid);

                if (scenePath == null)
                {
                    sceneAsset = null;
                }
                else
                {
                    sceneAsset = AssetDatabase.LoadMainAssetAtPath(scenePath) as SceneAsset;
                }
            }


            var guiContent = new GUIContent("Scene");
            position.height -= EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.ObjectField(position, guiContent, sceneAsset, typeof(SceneAsset), false);
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.LabelField(position, $"Path: {scenePath}");
        }
    }
}
