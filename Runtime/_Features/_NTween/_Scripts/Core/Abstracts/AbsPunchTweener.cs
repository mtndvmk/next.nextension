using System;

namespace Nextension.Tween
{
    internal abstract class AbsPunchTweener<T, TData> : AbsValueTweener<T, TData>
        where T : unmanaged
        where TData : struct
    {
        public T origin;
        public T punchDestination;

        public AbsPunchTweener(T origin, T punchDestination, Action<T> onValueChanged) : base(onValueChanged)
        {
            this.origin = origin;
            this.punchDestination = punchDestination;
        }

        public void updateDestination(T value)
        {
            punchDestination = value;
        }
    }
}