namespace Nextension
{
    public class Ref<T> where T : unmanaged
    {
        public Ref() { }
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
}
