using UnityEditor;
using UnityEngine;

namespace Nextension.NEditor
{
    [CustomPropertyDrawer(typeof(IRefInResources), true)]
    public class RefInResourcesDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var refInResources = NEditorHelper.getValue<IRefInResources>(property);
            var guid = refInResources.getGuid();
            var currentObj = NAssetUtils.loadMainAssetFromGUID(guid, out _);
            var refType = refInResources.getRefValueType();
            if (!guid.isNullOrEmpty() && (currentObj == null || !currentObj.GetType().isInherited(refType)))
            {
                currentObj = null;
                refInResources.setValue(currentObj);
                NAssetUtils.setDirty(property.serializedObject.targetObject);
            }
            var guiContent = new GUIContent(property.displayName);
            var newObj = EditorGUI.ObjectField(position, guiContent, currentObj, refType, false);

            bool isInResources = NAssetUtils.getPathInResources(newObj, out var newPath);

            if (isInResources)
            {
                if (newObj != currentObj || !newPath.removeExtension().Equals(refInResources.getPath()))
                {
                    refInResources.setValue(newObj);
                    NAssetUtils.setDirty(property.serializedObject.targetObject);
                }
            }
            else
            {
                if (newObj.isNull())
                {
                    if (!currentObj.isNull())
                    {
                        refInResources.setValue(null);
                        NAssetUtils.setDirty(property.serializedObject.targetObject);
                    }
                }
                else
                {
                    Debug.LogError($"Object [{newObj.name}] does not exist in the Resources directory", newObj);
                }
            }
        }
    }
}
