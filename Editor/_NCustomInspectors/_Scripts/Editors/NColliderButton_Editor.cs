using UnityEditor;

namespace Nextension.NEditor
{
    [CustomEditor(typeof(NColliderButton)), CanEditMultipleObjects]
    public class NColliderButton_Editor : Editor
    {
        private static bool _isShowEvent;

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("_betweenClickIntervalTime"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_delayInvokeTime"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_interactable"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_includeListenersInChildren"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_interactableWhenPointerOverUI"));

            _isShowEvent = EditorGUILayout.Foldout(_isShowEvent, " Events", true);
            if (_isShowEvent)
            {
                var button = (NColliderButton)target;
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(button.onButtonDownEvent)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(button.onButtonUpEvent)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(button.onButtonClickEvent)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(button.onButtonEnterEvent)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(button.onButtonExitEvent)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(button.onEnableInteractableEvent)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(button.onDisableInteractableEvent)));
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
