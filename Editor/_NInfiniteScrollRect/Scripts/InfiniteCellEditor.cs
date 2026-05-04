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
            if (infiniteCell.HasData)
            {
                EditorGUILayout.LabelField($"Cell Size: {infiniteCell.CellData.cellSize}");
            }
            EditorGUI.EndDisabledGroup();
            base.OnInspectorGUI();
        }
    }
}
