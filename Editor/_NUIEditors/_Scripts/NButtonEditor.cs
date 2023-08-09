using UnityEditor;

namespace Nextension.UI.NEditor
{

    [CustomEditor(typeof(NButton))]
    public class NButtonEditor : Editor
    {
        private static bool _isShowEvent;
        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_betweenClickIntervalTime"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_delayInvokeTime"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_isInteractable"));

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
        }
    }
}
