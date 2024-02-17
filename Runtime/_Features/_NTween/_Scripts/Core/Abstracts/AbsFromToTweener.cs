using System;
using System.Runtime.CompilerServices;

namespace Nextension.Tween
{
    internal abstract class AbsFromToTweener<T, TData> : AbsValueTweener<T, TData> where T : unmanaged where TData : struct
    {
        public T from;
        public T destination;

        public AbsFromToTweener(T from, T to, Action<T> onValueChanged) : base(onValueChanged)
        {
            this.from = from;
            this.destination = to;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void forceComplete()
        {
            invokeValueChanged(destination);
        }
        public void updateDestination(T value)
        {
            destination = value;
        }
    }
}