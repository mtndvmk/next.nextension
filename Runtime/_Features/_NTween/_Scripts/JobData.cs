using Unity.Burst;

namespace Nextension.Tween
{
    [BurstCompile]
    public struct JobData<T> where T : struct
    {
        public TweenType tweenType;
        public TweenLoopType tweenLoopType;
        public EaseType easeType;
        public T from;
        public T to;
        public float startTime;
        public float duration;
    }
}
