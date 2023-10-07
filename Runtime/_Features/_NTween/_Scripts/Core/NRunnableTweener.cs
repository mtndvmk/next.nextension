using System;
using System.Runtime.CompilerServices;

namespace Nextension.Tween
{
    public abstract class NRunnableTweener : GenericNTweener<NRunnableTweener>
    {
        internal ChunkIndex chunkIndex;
        internal EaseType easeType;
        internal float duration;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal uint createRunnerId() => NConverter.bitConvert<int, uint>(getRunnerType().GetHashCode());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NRunnableTweener setDuration(float duration)
        {
            this.duration = duration;
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NRunnableTweener setEase(EaseType easeType)
        {
            this.easeType = easeType;
            return this;
        }

        internal CommonJobData getCommonJobData()
        {
            return new()
            {
                easeType = easeType,
                startTime = startTime,
                duration = duration,
            };
        }

        internal abstract AbsTweenRunner createRunner();
        internal abstract Type getRunnerType();
    }
    internal abstract class GenericNRunnableTweener<TJobData> : NRunnableTweener
        where TJobData : struct
    {
        public abstract TJobData getJobData();
    }
}
