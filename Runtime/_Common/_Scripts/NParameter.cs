using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Nextension
{
    [Serializable]
    public struct NParameter
    {
        public enum ParameterType
        {
            Bool,
            Number,
            String,
            Component
        }
        [SerializeField] private ParameterType type;
        [SerializeField] private float numberValue;
        [SerializeField] private bool boolValue;
        [SerializeField] private string stringValue;
        [SerializeField] private Object componentValue;

        public ParameterType Type => type;
        public object Value
        {
            get
            {
                switch (type)
                {
                    case ParameterType.Bool:
                        return boolValue;
                    case ParameterType.Number:
                        return numberValue;
                    case ParameterType.String:
                        return stringValue;
                    case ParameterType.Component:
                        return componentValue;
                    default:
                        return null;
                }
            }
        }
        public bool BoolValue
        {
            get
            {
                if (type != ParameterType.Bool)
                {
                    throw new ArgumentException("RoveParameterType is not Boolean");
                }
                return boolValue;
            }
        }
        public float NumberValue
        {
            get
            {
                if (type != ParameterType.Number)
                {
                    throw new ArgumentException("RoveParameterType is not Float");
                }
                return numberValue;
            }
        }
        public string StringValue
        {
            get
            {
                if (type != ParameterType.String)
                {
                    throw new ArgumentException("RoveParameterType is not String");
                }
                return stringValue;
            }
        }
        public Object ComponentValue
        {
            get
            {
                if (type != ParameterType.Component)
                {
                    throw new ArgumentException("RoveParameterType is not Component");
                }
                return componentValue;
            }
        }
        private object getParameter(Type type)
        {
            if (type == Value.GetType())
            {
                return Value;
            }
            if (Type == ParameterType.Component && !type.IsPrimitive && type != typeof(string))
            {
                if (ComponentValue is Component)
                {
                    var component = (Component)ComponentValue;
                    if (component == null)
                    {
                        return null;
                    }

                    if (type == typeof(GameObject))
                    {
                        return component.gameObject;
                    }
                    return component.GetComponent(type);
                }
                else
                {
                    return ComponentValue;
                }
            }
            throw new Exception($"Type: `{Value.GetType()}` not match with input type: `{type}`");
        }
        public T getParameter<T>()
        {
            var result = getParameter(typeof(T));
            return (T)result;
        }
        public bool tryGetParameter<T>(out T result)
        {
            try
            {
                result = getParameter<T>();
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex);
                result = default;
                return false;
            }
        }
    }
}
