using UnityEditor;

namespace Nextension.NEditor
{
    [CustomEditor(typeof(NSequenceSpriteAnimation)), CanEditMultipleObjects]
    public class NSequenceSpriteAnimation_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
            EditorGUI.EndDisabledGroup();

            var targetTypeProperty = serializedObject.FindProperty("_targetType");
            var targetType = (NSequenceSpriteAnimation.TargetType)(targetTypeProperty).intValue;

            EditorGUILayout.PropertyField(targetTypeProperty);
            switch (targetType)
            {
                case NSequenceSpriteAnimation.TargetType.SpriteRenderer:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_spriteRenderer"));
                    break;
                case NSequenceSpriteAnimation.TargetType.Image:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_image"));
                    break;
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_spriteFrames"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_fps"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_isLoop"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_autoPlayOnEnable"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_autoDisableOnEndOfFrames"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_autoClearSpriteOnEndOfFrames"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
