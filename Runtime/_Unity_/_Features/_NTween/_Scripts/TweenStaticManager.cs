using Unity.Burst;

namespace Nextension.Tween
{
    internal readonly struct TweenStaticManager
    {
        public readonly static SharedStatic<float> currentTimeInJob = SharedStatic<float>.GetOrCreateUnsafe(0, BurstRuntime.GetHashCode64<TweenStaticManager>(), 0);
        public readonly static SharedStatic<float> currentUnscaledTimeInJob = SharedStatic<float>.GetOrCreateUnsafe(0, BurstRuntime.GetHashCode64<TweenStaticManager>(), 1);
        public static float currentTime;
        public static float currentUnscaledTime;
        public static uint runningTweenerCount;
    }
}