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
                if (property.numericType == SerializedPropertyNumericType.Unknown)
                {
                    CustomPropertyDrawerCache.forceDraw(position, property, label);
                }
                else
                {
                    var containerObj = NEditorHelper.getContainerObject(property);
                    if (containerObj != null)
                    {
                        if (attribute is NSliderAttribute nSliderAttribute)
                        {
                            var min = nSliderAttribute.getMinValue(containerObj);
                            var max = nSliderAttribute.getMaxValue(containerObj);

                            bool isInt = property.numericType == SerializedPropertyNumericType.Int32;

                            if (isInt)
                            {
                                var val = property.intValue;
                                int minValue = min is int intMin ? intMin : (int)(float)min;
                                int maxValue = max is int intMax ? intMax : (int)(float)max;
                                property.intValue = EditorGUI.IntSlider(position, label, val, minValue, maxValue);
                            }
                            else
                            {
                                var val = property.floatValue;
                                float minValue = min is float fMin ? fMin : (int)min;
                                float maxValue = max is float fMax ? fMax : (int)max;
                                property.floatValue = EditorGUI.Slider(position, label, val, minValue, maxValue);
                            }
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
                return;
            }
        }
    }
}