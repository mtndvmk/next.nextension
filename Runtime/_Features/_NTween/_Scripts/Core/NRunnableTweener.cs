namespace Nextension.Tween
{
    public abstract class NRunnableTweener : GenericNTweener<NRunnableTweener>
    {
        internal ChunkIndex chunkIndex;
        internal EaseType easeType;

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
            return new CommonJobData(updateMode, easeType, startTime, duration);
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
