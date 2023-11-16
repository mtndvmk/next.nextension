using UnityEditor;

namespace Nextension.UI.NEditor
{

    [CustomEditor(typeof(NButton))]
    public class NButtonEditor : Editor
    {
        private static bool _isShowEvent;
        private bool _isInteractable;
        private NButton _button;
        private void OnEnable()
        {
            _button = serializedObject.targetObject as NButton;
            _isInteractable = serializedObject.FindProperty("_isInteractable").boolValue;
        }
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("_betweenClickIntervalTime"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_delayInvokeTime"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_isInteractable"));

            if (_isInteractable != _button.IsInteractable)
            {
                var isInteractable = _button.IsInteractable;
                _button.setInteractableWithoutNotify(!isInteractable);
                _button.IsInteractable = _isInteractable = isInteractable;
            }

            _isShowEvent = EditorGUILayout.Foldout(_isShowEvent, " Events", true);
            if (_isShowEvent)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("onButtonDownEvent"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("onButtonUpEvent"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("onButtonClickEvent"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("onButtonEnterEvent"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("onButtonExitEvent"));
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
