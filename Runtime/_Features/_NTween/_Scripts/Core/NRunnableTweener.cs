using System;
using System.Runtime.CompilerServices;

namespace Nextension.Tween
{
    public abstract class NRunnableTweener : GenericNTweener<NRunnableTweener>
    {
        internal ChunkIndex chunkIndex;
        internal float duration;
        internal EaseType easeType;

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

        protected override void onInnerCanceled()
        {
            base.onInnerCanceled();
            if (chunkIndex.chunkId != 0)
            {
                NTweenManager.cancelFromTweener(this);
            }
        }
        protected override void onResetState()
        {
            base.onResetState();
            chunkIndex = default;
        }
        protected override void onRun()
        {
            NTweenManager.run(this);
        }
        protected override void onSchedule()
        {
            NTweenManager.schedule(this);
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
        internal abstract ushort getRunnerId();
    }
    internal abstract class GenericNRunnableTweener<TJobData> : NRunnableTweener
        where TJobData : struct
    {
        public abstract TJobData getJobData();
    }
}
