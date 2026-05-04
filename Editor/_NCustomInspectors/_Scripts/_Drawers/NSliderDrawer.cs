using System;
using UnityEditor;
using UnityEngine;

namespace Nextension.NEditor
{
    [CustomPropertyDrawer(typeof(NSliderAttribute))]
    public class NSliderDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            try
            {
                if (attribute is not NSliderAttribute nSliderAttribute)
                {
                    CustomPropertyDrawerCache.forceDraw(position, property, label);
                    return;
                }

                EditorGUI.BeginChangeCheck();
                if (property.numericType == SerializedPropertyNumericType.Unknown)
                {
                    if (property.boxedValue is NMinMax minMaxValue)
                    {
                        var containerObj = NEditorHelper.getContainerObject(property);
                        var minLimit = Convert.ToSingle(nSliderAttribute.getMinValue(containerObj));
                        var maxLimit = Convert.ToSingle(nSliderAttribute.getMaxValue(containerObj));

                        var minVal = minMaxValue.min;
                        var maxVal = minMaxValue.max;

                        var labelRect = position;
                        labelRect.width = GUI.skin.label.CalcSize(label).x + 20;
                        EditorGUI.LabelField(labelRect, label);

                        var totalX = position.width;

                        var minRect = position;
                        minRect.width = 75;
                        minRect.x = labelRect.width + minRect.width;
                        var maxRect = minRect;

                        position.x = minRect.width + minRect.x;
                        position.width -= position.x + 30;
                        maxRect.x = totalX - 30;

                        minVal = EditorGUI.FloatField(minRect, minVal);
                        if (minVal > maxVal)
                        {
                            minVal = maxVal;
                        }
                        else
                        {
                            minVal = Mathf.Clamp(minVal, minLimit, maxLimit);
                        }
                        maxVal = EditorGUI.FloatField(maxRect, maxVal);
                        if (maxVal < minVal)
                        {
                            maxVal = minVal;
                        }
                        else
                        {
                            maxVal = Mathf.Clamp(maxVal, minLimit, maxLimit);
                        }
                        EditorGUI.MinMaxSlider(position, "", ref minVal, ref maxVal, minLimit, maxLimit);

                        minMaxValue.min = minVal;
                        minMaxValue.max = maxVal;

                        property.boxedValue = minMaxValue;
                        return;
                    }
                    CustomPropertyDrawerCache.forceDraw(position, property, label);
                }
                else
                {
                    var containerObj = NEditorHelper.getContainerObject(property);
                    if (containerObj != null)
                    {
                        var min = nSliderAttribute.getMinValue(containerObj);
                        var max = nSliderAttribute.getMaxValue(containerObj);

                        if (property.numericType == SerializedPropertyNumericType.Int32)
                        {
                            var val = property.intValue;
                            int minLimit = Convert.ToInt32(min);
                            int maxLimit = Convert.ToInt32(max);
                            label.text += $" ({min}:{max})";
                            property.intValue = EditorGUI.IntSlider(position, label, val, minLimit, maxLimit);
                        }
                        else if (property.numericType == SerializedPropertyNumericType.UInt32)
                        {
                            var val = (int)property.uintValue;
                            int minLimit = Convert.ToInt32(min);
                            int maxLimit = Convert.ToInt32(max);
                            label.text += $" ({min}:{max})";
                            property.uintValue = (uint)EditorGUI.IntSlider(position, label, val, minLimit, maxLimit);
                        }
                        else if (property.numericType == SerializedPropertyNumericType.Int64)
                        {
                            var val = (int)property.longValue;
                            int minLimit = Convert.ToInt32(min);
                            int maxLimit = Convert.ToInt32(max);
                            label.text += $" ({min}:{max})";
                            property.longValue = EditorGUI.IntSlider(position, label, val, minLimit, maxLimit);
                        }
                        else if (property.numericType == SerializedPropertyNumericType.UInt64)
                        {
                            var val = (int)property.ulongValue;
                            int minLimit = Convert.ToInt32(min);
                            int maxLimit = Convert.ToInt32(max);
                            label.text += $" ({min}:{max})";
                            property.ulongValue = (ulong)EditorGUI.IntSlider(position, label, val, minLimit, maxLimit);
                        }
                        else if (property.numericType == SerializedPropertyNumericType.Double)
                        {
                            var val = (float)property.doubleValue;
                            var minLimit = Convert.ToSingle(min);
                            var maxLimit = Convert.ToSingle(max);
                            label.text += $" ({min}:{max})";
                            property.doubleValue = EditorGUI.Slider(position, label, val, minLimit, maxLimit);
                        }
                        else
                        {
                            var val = property.floatValue;
                            var minLimit = Convert.ToSingle(min);
                            var maxLimit = Convert.ToSingle(max);
                            label.text += $" ({min}:{max})";
                            property.floatValue = EditorGUI.Slider(position, label, val, minLimit, maxLimit);
                        }
                    }
                    else
                    {
                        throw new Exception("Can't get container object");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                EditorGUI.EndChangeCheck();
            }
        }
    }
}