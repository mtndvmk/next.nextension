using UnityEditor;
using UnityEngine;

namespace Nextension.NEditor
{
    [CustomEditor(typeof(CameraAnimatorRecorder))]
    public class CameraAnimatorRecorderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Start record"))
            {
                ((CameraAnimatorRecorder)target).startRecordCurrentAnimation();
            }
        }
    }
}
