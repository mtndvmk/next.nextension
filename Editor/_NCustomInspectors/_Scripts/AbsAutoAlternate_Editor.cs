using Nextension.Tween;
using System;
using UnityEditor;

namespace Nextension.NEditor
{
    [CustomEditor(typeof(AbsAutoAlternate<>), true), CanEditMultipleObjects]
    public class AbsAutoAlternate_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            SerializedProperty iterator = serializedObject.GetIterator();
            iterator.NextVisible(true);

            bool isFromToOnly = false;
            AutoAlternateColor.AutoAlternateColorType? autoAlternateColorType = null;

            bool isAutoAlternateColor = target.GetType() == typeof(AutoAlternateColor);

            while (iterator.NextVisible(false))
            {
                var iteratorName = iterator.name;
                if (iteratorName == "_onlyFromTo")
                {
                    isFromToOnly = iterator.boolValue;
                    EditorGUILayout.PropertyField(iterator);
                }
                else if (iteratorName == "_delayOnlyFromTo")
                {
                    if (isFromToOnly)
                    {
                        EditorGUILayout.PropertyField(iterator);
                    }
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
                        else
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

            serializedObject.ApplyModifiedProperties();
        }
    }
}
