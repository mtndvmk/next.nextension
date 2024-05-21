using Nextension.Tween;
using UnityEditor;
using UnityEngine;
using Unity.Mathematics;

namespace Nextension.NEditor
{
    [CustomEditor(typeof(AbsAutoAlternate<>), true), CanEditMultipleObjects]
    public class AbsAutoAlternate_Editor : Editor
    {
        private static bool _isUniformScale;
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
            EditorGUI.EndDisabledGroup();

            SerializedProperty iterator = serializedObject.GetIterator();
            iterator.NextVisible(true);

            bool isFromToOnly = false;
            AutoAlternateColor.AutoAlternateColorType? autoAlternateColorType = null;

            bool isAutoAlternateColor = target.GetType() == typeof(AutoAlternateColor);
            bool isAutoAlterScale = !isAutoAlternateColor && target.GetType() == typeof(AutoAlternateScale);

            if (isAutoAlterScale)
            {
                _isUniformScale = EditorGUILayout.Toggle("Uniform Scale", _isUniformScale);
            }

            while (iterator.NextVisible(false))
            {
                var iteratorName = iterator.name;
                if (iteratorName == "_onlyFromTo")
                {
                    isFromToOnly = iterator.boolValue;
                    EditorGUILayout.PropertyField(iterator);
                }
                else
                {
                    if (isAutoAlternateColor)
                    {
                        if (iteratorName == "_autoAlternateColorType")
                        {
                            autoAlternateColorType = (AutoAlternateColor.AutoAlternateColorType)iterator.intValue;
                            EditorGUILayout.PropertyField(iterator);
                        }
                        else if (iteratorName == "_graphic")
                        {
                            if (autoAlternateColorType == AutoAlternateColor.AutoAlternateColorType.Graphic)
                            {
                                EditorGUILayout.PropertyField(iterator);
                            }
                        }
                        else if (iteratorName == "_spriteRenderer")
                        {
                            if (autoAlternateColorType == AutoAlternateColor.AutoAlternateColorType.SpriteRenderer)
                            {
                                EditorGUILayout.PropertyField(iterator);
                            }
                        }
                        else if (iteratorName == "_material")
                        {
                            if (autoAlternateColorType == AutoAlternateColor.AutoAlternateColorType.Material)
                            {
                                EditorGUILayout.PropertyField(iterator);
                            }
                        }
                        else if (iteratorName == "_canvasRenderer")
                        {
                            if (autoAlternateColorType == AutoAlternateColor.AutoAlternateColorType.CanvasRenderer)
                            {
                                EditorGUILayout.PropertyField(iterator);
                            }
                        }
                        else if (iteratorName == "_delayToFrom")
                        {
                            if (!isFromToOnly)
                            {
                                EditorGUILayout.PropertyField(iterator);
                            }
                        }
                        else
                        {
                            EditorGUILayout.PropertyField(iterator);
                        }
                    }
                    else if (isAutoAlterScale)
                    {
                        if (iteratorName == "_fromValue")
                        {
                            if (_isUniformScale)
                            {
                                var v = EditorGUILayout.FloatField("From value", ((float3)iterator.boxedValue).x);
                                iterator.boxedValue = new float3(v);
                            }
                            else
                            {
                                EditorGUILayout.PropertyField(iterator);
                            }
                        }
                        else if (iteratorName == "_toValue")
                        {
                            if (_isUniformScale)
                            {
                                var v = EditorGUILayout.FloatField("To value", ((float3)iterator.boxedValue).x);
                                iterator.boxedValue = new float3(v);
                            }
                            else
                            {
                                EditorGUILayout.PropertyField(iterator);
                            }
                        }
                        else if (iteratorName == "_delayToFrom")
                        {
                            if (!isFromToOnly)
                            {
                                EditorGUILayout.PropertyField(iterator);
                            }
                        }
                        else
                        {
                            EditorGUILayout.PropertyField(iterator);
                        }
                    }
                    else
                    {
                        if (iteratorName == "_delayToFrom")
                        {
                            if (!isFromToOnly)
                            {
                                EditorGUILayout.PropertyField(iterator);
                            }
                        }
                        else
                        {
                            EditorGUILayout.PropertyField(iterator);
                        }
                    }
                }
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Capture fromValue"))
            {
                ((IAutoAlternate)target).captureFromValue();
            }
            if (GUILayout.Button("Capture toValue"))
            {
                ((IAutoAlternate)target).captureToValue();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset to fromValue"))
            {
                ((IAutoAlternate)target).resetToFromValue();
            }
            if (GUILayout.Button("Reset to toValue"))
            {
                ((IAutoAlternate)target).resetToToValue();
            }
            EditorGUILayout.EndHorizontal();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
