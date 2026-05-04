using System;

namespace Nextension
{
    [Serializable]
    public class Ref<T>
    {
        public Ref()
        {
            value = default;
        }
        public Ref(T value)
        {
            this.value = value;
        }
        public T value;

        public static implicit operator T(Ref<T> r)
        {
            return r.value;
        }
    }

    public struct Nullable<T>
    {
        public bool HasValue { readonly get; private set; }
        private T _value;
        public T Value
        {
            readonly get
            {
                if (!HasValue)
                {
                    throw new InvalidOperationException("Nullable object must have a value.");
                }
                return _value;
            }
            set
            {
                _value = value;
                HasValue = true;
            }
        }

        public static implicit operator Nullable<T>(T value)
        {
            return new Nullable<T> { Value = value };
        }
        public static implicit operator T(Nullable<T> nullable)
        {
            return nullable.Value;
        }
    }
}
