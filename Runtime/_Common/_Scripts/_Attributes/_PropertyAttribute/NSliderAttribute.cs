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
            if (isNumber(predicateValue))
            {
                return predicateValue;
            }
            else
            {
                var val = NUtils.getValue(containerObject, predicateValue.ToString());
                if (isNumber(val))
                {
                    return val;
                }
                throw new Exception($"[NSlider] {predicateValue} is not number: {predicateValue.GetType()}");
            }
        }

        private static bool isNumber(object numObject)
        {
            if (numObject is int
                || numObject is uint
                || numObject is long
                || numObject is ulong
                || numObject is float
                || numObject is double)
            {
                return true;
            }
            return false;
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
