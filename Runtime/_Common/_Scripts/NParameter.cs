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

        public readonly ParameterType Type => type;
        public readonly object Value
        {
            get
            {
                return type switch
                {
                    ParameterType.Bool => boolValue,
                    ParameterType.Number => numberValue,
                    ParameterType.String => stringValue,
                    ParameterType.Component => componentValue,
                    _ => null,
                };
            }
        }
        public readonly bool BoolValue
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
        public readonly float NumberValue
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
        public readonly string StringValue
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
        public readonly Object ComponentValue
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
        public readonly T getComponent<T>() where T : Object
        {
            return ComponentValue as T;
        }
        private readonly object getParameter(Type type)
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
        public readonly T getParameter<T>()
        {
            var result = getParameter(typeof(T));
            return (T)result;
        }
        public readonly bool tryGetParameter<T>(out T result)
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

        public void setBool(bool value)
        {
            type = ParameterType.Bool;
            boolValue = value;
        }
        public void setNumber(float number)
        {
            type = ParameterType.Number;
            numberValue = number;
        }
        public void setString(string value)
        {
            type = ParameterType.String;
            stringValue = value;
        }
        public void setComponent<T>(T component) where T : Object
        {
            type = ParameterType.Component;
            componentValue = component;
        }
    }
}
