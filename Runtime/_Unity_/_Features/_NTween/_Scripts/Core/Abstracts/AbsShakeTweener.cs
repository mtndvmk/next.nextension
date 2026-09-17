using Unity.Mathematics;

namespace Nextension.Tween
{
    internal abstract class AbsShakeTweener<T> : NTweener where T : unmanaged
    {
        public T origin;
        public float4 range;

        public AbsShakeTweener(T origin, float4 range)
        {
            this.origin = origin;
            this.range = range;
        }
    }
}