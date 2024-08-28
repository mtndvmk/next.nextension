using UnityEditor;

namespace Nextension.NEditor
{
    [CustomEditor(typeof(InfiniteCell), true), CanEditMultipleObjects]
    public class InfiniteCellEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginDisabledGroup(true);
            var infiniteCell = (InfiniteCell)target;
            EditorGUILayout.LabelField($"Cell Index: {infiniteCell.CellIndex}");
            EditorGUI.EndDisabledGroup();
            base.OnInspectorGUI();
        }
    }
}
