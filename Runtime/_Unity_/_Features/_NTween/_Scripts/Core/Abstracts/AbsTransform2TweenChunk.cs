using Unity.Jobs;
using UnityEngine.Jobs;

namespace Nextension.Tween
{
    internal abstract class AbsTransform2TweenChunk<TTweener, TJob, TJobData> : GenericTweenChunk<TTweener, TJob, TJobData>
        where TTweener : GenericNRunnableTweener<TJobData>, ITransform2Tweener
        where TJob : struct, IJobParallelForTransform
        where TJobData : struct
    {
        protected TransformAccessArray _transformAccessArray;
        protected TransformAccessArray _destinationAccessArray;

        public AbsTransform2TweenChunk() : base()
        {
            _transformAccessArray = new TransformAccessArray(CHUNK_SIZE);
            _destinationAccessArray = new TransformAccessArray(CHUNK_SIZE);
        }

        protected override void onAddNewTweener(TTweener tweener)
        {
            base.onAddNewTweener(tweener);
            var maskIndex = tweener.chunkIndex.maskIndex;
            if (maskIndex >= _transformAccessArray.length)
            {
                _transformAccessArray.Add(tweener.Target);
                _destinationAccessArray.Add(tweener.Destination);
            }
            else
            {
                _transformAccessArray[maskIndex] = tweener.Target;
                _destinationAccessArray[maskIndex] = tweener.Destination;
            }
        }

        protected sealed override JobHandle onScheduleJob()
        {
            // Job 1: read destination transforms → write directly into _jobDataNativeArr (Burst)
            var readHandle = onScheduleReadJob();
            // Job 2: run tween using updated _jobDataNativeArr, depends on Job 1
            return _job.ScheduleByRef(_transformAccessArray, readHandle);
        }

        /// <summary>
        /// Schedule a Burst <see cref="IJobParallelForTransform"/> on <see cref="_destinationAccessArray"/>
        /// that writes live destination values directly into <see cref="_jobDataNativeArr"/>.
        /// </summary>
        protected abstract JobHandle onScheduleReadJob();

        public override void dispose()
        {
            base.dispose();
            _transformAccessArray.Dispose();
            _destinationAccessArray.Dispose();
        }
    }
}
