using System;
using UnityEngine;

namespace Nextension
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class NSliderAttribute : PropertyAttribute
    {
        protected readonly object min;
        protected readonly object max;

        public NSliderAttribute(string minPredicate, string maxPredicate)
        {
            this.min = minPredicate;
            this.max = maxPredicate;
            this.order = NAttributeOrder.SLIDER;
        }
        public NSliderAttribute(int minValue, int maxValue)
        {
            this.min = minValue;
            this.max = maxValue;
            this.order = NAttributeOrder.SLIDER;
        }
        public NSliderAttribute(int minValue, string maxPredicate)
        {
            this.min = minValue;
            this.max = maxPredicate;
            this.order = NAttributeOrder.SLIDER;
        }
        public NSliderAttribute(string minPredicate, int maxValue)
        {
            this.min = minPredicate;
            this.max = maxValue;
            this.order = NAttributeOrder.SLIDER;
        }
        public NSliderAttribute(float minValue, float maxValue)
        {
            this.min = minValue;
            this.max = maxValue;
            this.order = NAttributeOrder.SLIDER;
        }
        public NSliderAttribute(float minValue, string maxPredicate)
        {
            this.min = minValue;
            this.max = maxPredicate;
            this.order = NAttributeOrder.SLIDER;
        }
        public NSliderAttribute(string minPredicate, float maxValue)
        {
            this.min = minPredicate;
            this.max = maxValue;
            this.order = NAttributeOrder.SLIDER;
        }

        protected static object getValue(object predicateValue, object containerObject)
        {
            if (predicateValue is int intVal)
            {
                return intVal;
            }
            else if (predicateValue is float floatVal)
            {
                return floatVal;
            }
            else
            {
                var val = NUtils.getValue(containerObject, predicateValue.ToString());
                if (val is int intValue)
                {
                    return intValue;
                }
                if (val is float floatValue)
                {
                    return floatValue;
                }
                else
                {
                    throw new Exception($"[NSlider] {predicateValue} is not number");
                }
            }
        }

        public object getMinValue(object containerObject)
        {
            return getValue(min, containerObject);
        }
        public object getMaxValue(object containerObject)
        {
            return getValue(max, containerObject);
        }
    }
}
