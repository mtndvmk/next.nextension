using Unity.Collections.LowLevel.Unsafe;

namespace Nextension.Tween
{
    internal abstract class AbsFromToTweener<T> : NTweener where T : unmanaged
    {
        public T from;
        public T destination;

        public AbsFromToTweener(T from, T to)
        {
            this.from = from;
            this.destination = to;
        }
        internal unsafe override void forceComplete()
        {
            invokeValueChanged(UnsafeUtility.AddressOf(ref destination));
            invokeOnUpdate();
            invokeOnComplete();
        }
        public void updateDestination(T value)
        {
            destination = value;
        }
    }
}