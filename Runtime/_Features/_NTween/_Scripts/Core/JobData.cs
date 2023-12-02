using Unity.Burst;

namespace Nextension.Tween
{
    [BurstCompile]
    internal struct CommonJobData
    {
        public EaseType easeType;
        public float startTime;
        public float duration;
    }
    [BurstCompile]
    internal struct FromToData<T> where T : unmanaged
    {
        public CommonJobData common;
        public T from;
        public T to;
    }
    [BurstCompile]
    internal struct PunchData<T> where T : unmanaged
    {
        public CommonJobData common;
        public T origin;
        public T punchDestination;
    }
    [BurstCompile]
    internal struct ShakeData<T> where T : unmanaged
    {
        public CommonJobData common;
        public float range;
        public T origin;
    }

    [BurstCompile]
    internal struct TransformFromToData<T> where T : unmanaged
    {
        public TransformTweenType transformTweenType;
        public FromToData<T> fromToData;
    }
    [BurstCompile]
    internal struct TransformPunchData<T> where T : unmanaged
    {
        public TransformTweenType transformTweenType;
        public PunchData<T> punchData;
    }
    [BurstCompile]
    internal struct TransformShakeData<T> where T : unmanaged
    {
        public TransformTweenType transformTweenType;
        public ShakeData<T> shakeData;
    }
}
