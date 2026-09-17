using Unity.Burst;
using Unity.Mathematics;

namespace Nextension.Tween
{
    [BurstCompile]
    internal readonly struct CommonJobData
    {
        public readonly NUpdateMode updateMode;
        public readonly EaseType easeType;
        public readonly float startTime;
        public readonly float duration;

        public CommonJobData(NUpdateMode updateMode, EaseType easeType, float startTime, float duration)
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
        public readonly float4 range;
        public readonly T origin;

        public ShakeData(CommonJobData common, float4 range, T origin)
        {
            this.common = common;
            this.range = range;
            this.origin = origin;
        }
    }
    [BurstCompile]
    internal readonly struct JumpData<T> where T : unmanaged
    {
        public readonly CommonJobData common;
        public readonly T origin;
        public readonly T destination;
        public readonly T jumpHeight;

        public JumpData(CommonJobData common, T origin, T destination, T jumpHeight)
        {
            this.common = common;
            this.origin = origin;
            this.destination = destination;
            this.jumpHeight = jumpHeight;
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
    internal readonly struct TransformShakeData<T> where T : unmanaged
    {
        public readonly TransformTweenType transformTweenType;
        public readonly ShakeData<T> shakeData;

        public TransformShakeData(TransformTweenType transformTweenType, ShakeData<T> shakeData)
        {
            this.transformTweenType = transformTweenType;
            this.shakeData = shakeData;
        }
    }
    [BurstCompile]
    internal readonly struct TransformJumpData<T> where T : unmanaged
    {
        public readonly TransformTweenType transformTweenType;
        public readonly JumpData<T> jumpData;

        public TransformJumpData(TransformTweenType transformTweenType, JumpData<T> jumpData)
        {
            this.transformTweenType = transformTweenType;
            this.jumpData = jumpData;
        }
    }
}
