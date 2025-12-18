using System;

namespace Nextension
{
    [Serializable]
    public class Ref<T>
    {
        public Ref(T value)
        {
            this.value = value;
        }
        public T value;

        public static implicit operator T(Ref<T> r)
        {
            return r.value;
        }
        public static implicit operator Ref<T>(T t)
        {
            return new Ref<T>(t);
        }
    }
}
