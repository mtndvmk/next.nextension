using System;

namespace Nextension.Tween
{
    internal abstract class AbsShakeTweener<T, TData> : AbsValueTweener<T, TData> where T : unmanaged where TData : struct
    {
        public T origin;
        public float range;

        public AbsShakeTweener(T origin, float range, Action<T> onValueChanged) : base(onValueChanged)
        {
            this.origin = origin;
            this.range = range;
        }
    }
}