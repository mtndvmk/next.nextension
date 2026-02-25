using System;
using UnityEngine;

namespace Nextension
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class NShowIfAttribute : ApplyToCollectionPropertyAttribute
    {
        protected readonly string predicateName;
        protected readonly object[] conditionValues;

        static readonly object DEFAULT = new object();

        public NShowIfAttribute(string predicateName) : base()
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
        public NShowIfAttribute(string predicateName, object conditionValue) : base()
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
        public NShowIfAttribute(string predicateName, params object[] conditionValues) : base()
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
            if (conditionValues.isNull())
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
                var conditionValue2 = getPredicateValue(containerObject, conditionValue, true);
                if (conditionValue2.isNull()) continue;
                try
                {
                    if (targetState.GetType() == conditionValue2.GetType())
                    {
                        if (targetState.compareTo(conditionValue2) == 0)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (targetState.compareTo(conditionValue) == 0)
                        {
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning(ex);
                }
            }
            return false;
        }
        protected static object getPredicateValue(object containerObject, object predicateValue, bool returnInputIfNull)
        {
            if (predicateValue.isNull())
            {
                return default;
            }
            var val = NUtils.getValue(containerObject, predicateValue.ToString());


            if (val.isNull() && returnInputIfNull)
            {
                return predicateValue;
            }
            return val;
        }

        public bool check(object containerObject)
        {
            var predicateValue = getPredicateValue(containerObject, predicateName, false);
            if (predicateValue.isNull())
            {
                if (conditionValues.isNull() || conditionValues.Length == 0) return true;
                foreach (var conditionValue in conditionValues)
                {
                    if (conditionValue.isNull()) return true;
                }
                return false;
            }
            return checkCondition(predicateValue, containerObject);
        }
    }
}
