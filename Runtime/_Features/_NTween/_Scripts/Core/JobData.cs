using Unity.Burst;

namespace Nextension.Tween
{
    [BurstCompile]
    internal readonly struct CommonJobData
    {
        public readonly NTweener.UpdateMode updateMode;
        public readonly EaseType easeType;
        public readonly float startTime;
        public readonly float duration;

        public CommonJobData(NTweener.UpdateMode updateMode, EaseType easeType, float startTime, float duration)
        {
            this.updateMode = updateMode;
            this.easeType = easeType;
            this.startTime = startTime;
            this.duration = duration;
        }
    }
    [BurstCompile]
    internal readonly struct FromToData<T> where T : unmanaged
    {
        public readonly CommonJobData common;
        public readonly T from;
        public readonly T to;

        public FromToData(CommonJobData common, T from, T to)
        {
            this.common = common;
            this.from = from;
            this.to = to;
        }
    }
    [BurstCompile]
    internal readonly struct PunchData<T> where T : unmanaged
    {
        public readonly CommonJobData common;
        public readonly T origin;
        public readonly T punchDestination;

        public PunchData(CommonJobData common, T origin, T punchDestination)
        {
            this.common = common;
            this.origin = origin;
            this.punchDestination = punchDestination;
        }
    }
    [BurstCompile]
    internal readonly struct ShakeData<T> where T : unmanaged
    {
        public readonly CommonJobData common;
        public readonly float range;
        public readonly T origin;

        public ShakeData(CommonJobData common, float range, T origin)
        {
            this.common = common;
            this.range = range;
            this.origin = origin;
        }
    }

    [BurstCompile]
    internal struct TransformFromToData<T> where T : unmanaged
    {
        public TransformTweenType transformTweenType;
        public FromToData<T> fromToData;

        public TransformFromToData(TransformTweenType transformTweenType, FromToData<T> fromToData)
        {
            this.transformTweenType = transformTweenType;
            this.fromToData = fromToData;
        }
    }
    [BurstCompile]
    internal readonly struct TransformPunchData<T> where T : unmanaged
    {
        public readonly TransformTweenType transformTweenType;
        public readonly PunchData<T> punchData;

        public TransformPunchData(TransformTweenType transformTweenType, PunchData<T> punchData)
        {
            this.transformTweenType = transformTweenType;
            this.punchData = punchData;
        }
    }
    [BurstCompile]
    internal struct TransformShakeData<T> where T : unmanaged
    {
        public readonly TransformTweenType transformTweenType;
        public readonly ShakeData<T> shakeData;

        public TransformShakeData(TransformTweenType transformTweenType, ShakeData<T> shakeData)
        {
            this.transformTweenType = transformTweenType;
            this.shakeData = shakeData;
        }
    }
}
