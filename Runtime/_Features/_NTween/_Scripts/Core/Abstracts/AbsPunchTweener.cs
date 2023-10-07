using System;

namespace Nextension.Tween
{
    internal abstract class AbsPunchTweener<T, TData> : AbsValueTweener<T, TData>
        where T : unmanaged
        where TData : struct
    {
        public T origin;
        public T punchValue;

        public AbsPunchTweener(T origin, T punchValue, Action<T> onValueChanged) : base(onValueChanged)
        {
            this.origin = origin;
            this.punchValue = punchValue;
        }
    }
}