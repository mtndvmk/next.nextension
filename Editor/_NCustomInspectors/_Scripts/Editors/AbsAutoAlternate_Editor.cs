using Nextension.Tween;
using UnityEditor;
using UnityEngine;

namespace Nextension.NEditor
{
    [CustomEditor(typeof(AbsAutoAlternate), true), CanEditMultipleObjects]
    public class AbsAutoAlternate_Editor : Editor
    {
        void OnEnable()
        {
            if (Application.isPlaying)
            {
                EditorApplication.update += Repaint;
            }
        }

        void OnDisable()
        {
            EditorApplication.update -= Repaint;
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var obj = serializedObject.targetObject as AbsAutoAlternate;
            if (obj.IsRunning)
            {
                EditorGUI.BeginDisabledGroup(true);
                var currentTime = obj.CurrentTime;
                var totalTime = obj.FromToDuration;
                var normalizedTime = totalTime > 0 ? currentTime / totalTime : 0;
                EditorGUILayout.Slider($"Time ({normalizedTime:0.##})", currentTime,  0, totalTime);
                EditorGUI.EndDisabledGroup();
            }

            if (Application.isPlaying)
            {
                if (obj.IsRunning)
                {
                    if (obj.IsPaused)
                    {
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button("Resume"))
                        {
                            obj.resume();
                        }
                        if (GUILayout.Button("Stop"))
                        {
                            obj.stop();
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button("Pause"))
                        {
                            obj.pause();
                        }
                        if (GUILayout.Button("Stop"))
                        {
                            obj.stop();
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Start"))
                    {
                        obj.startImmediate();
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
