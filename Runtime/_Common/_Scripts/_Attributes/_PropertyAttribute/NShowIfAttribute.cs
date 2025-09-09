using System;
using UnityEngine;

namespace Nextension
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class NShowIfAttribute : PropertyAttribute
    {
        protected readonly string predicateName;
        protected readonly object[] conditionValues;

        static object DEFAULT = new object();

        public NShowIfAttribute(string predicateName)
        {
            if (predicateName.isNullOrEmpty())
            {
                Debug.LogError($"`{nameof(predicateName)}` is null or empty");
                return;
            }
            this.predicateName = predicateName;
            this.conditionValues = new object[] { DEFAULT };
            this.order = NAttributeOrder.SHOW_IF;
        }
        public NShowIfAttribute(string predicateName, object conditionValue)
        {
            if (predicateName.isNullOrEmpty())
            {
                Debug.LogError($"`{nameof(predicateName)}` is null or empty");
                return;
            }
            this.predicateName = predicateName;
            this.conditionValues = new object[] { conditionValue };
            this.order = NAttributeOrder.SHOW_IF;
        }
        public NShowIfAttribute(string predicateName, params object[] conditionValues)
        {
            if (predicateName.isNullOrEmpty())
            {
                Debug.LogError($"`{nameof(predicateName)}` is null or empty");
                return;
            }
            this.predicateName = predicateName;
            this.conditionValues = conditionValues;
            this.order = NAttributeOrder.SHOW_IF;
        }

        protected virtual bool checkCondition(object targetState, object containerObject)
        {
            if (conditionValues == null)
            {
                return targetState.compareTo(default) == 0;
            }
            foreach (var conditionValue in conditionValues)
            {
                if (conditionValue == DEFAULT)
                {
                    var targetType = targetState.GetType();
                    if (targetType.IsValueType)
                    {
                        var defaultValue = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(targetType);
                        if (targetState.compareTo(defaultValue) != 0)
                            return true;
                        return false;
                    }
                    return !targetType.isNull();
                }
                var conditionValue2 = getPredicateValue(containerObject, conditionValue);
                if (conditionValue2.isNull()) continue;
                if (targetState.compareTo(conditionValue2) == 0)
                {
                    return true;
                }
            }
            return false;
        }
        protected static object getPredicateValue(object containerObject, object predicateValue)
        {
            if (predicateValue.isNull())
            {
                return default;
            }
            var val = NUtils.getValue(containerObject, predicateValue.ToString());
            return val ?? predicateValue;
        }

        public bool check(object containerObject)
        {
            var predicateValue = getPredicateValue(containerObject, predicateName);
            if (predicateValue.isNull())
            {
                if (conditionValues == null || conditionValues.Length == 0) return true;
                foreach (var conditionValue in conditionValues)
                {
                    if (conditionValue == null) return true;
                }
                return false;
            }
            return checkCondition(predicateValue, containerObject);
        }
    }
    public class NShowIfAllAttribute : NShowIfAttribute
    {
        public NShowIfAllAttribute(string predicateName, params object[] conditionValues) : base(predicateName, conditionValues)
        {
        }
        protected override bool checkCondition(object targetState, object containerObject)
        {
            if (conditionValues.Length == 0)
            {
                return false;
            }
            foreach (var conditionValue in conditionValues)
            {
                var pedicateState = getPredicateValue(containerObject, conditionValue);
                if (targetState.compareTo(pedicateState) != 0)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
