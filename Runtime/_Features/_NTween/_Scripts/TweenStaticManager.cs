using Unity.Burst;

namespace Nextension.Tween
{
    internal readonly struct TweenStaticManager
    {
        public readonly static SharedStatic<float> currentTimeInJob = SharedStatic<float>.GetOrCreate<TweenStaticManager>();
        public static float currentTime;
        public static uint runningTweenerCount;
    }
}