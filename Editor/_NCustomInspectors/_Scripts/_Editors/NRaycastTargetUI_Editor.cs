using UnityEditor;

namespace Nextension.NEditor
{
    [CustomEditor(typeof(NRaycastTargetUI), true), CanEditMultipleObjects]
    public class NRaycastTargetUI_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RaycastTarget"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RaycastPadding"));
        }
    }
}
