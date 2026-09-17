using Unity.Jobs;
using UnityEngine.Jobs;

namespace Nextension.Tween
{
    internal abstract class AbsTransform2TweenChunk<TJob, TJobData> : GenericTweenChunk<TJob, TJobData>
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

        protected override void onAddNewTweener(NTweener tweener)
        {
            base.onAddNewTweener(tweener);
            var maskIndex = tweener.chunkIndex.maskIndex;

            var tfTweener = (ITransform2Tweener)tweener;
            if (maskIndex >= _transformAccessArray.length)
            {
                _transformAccessArray.Add(tfTweener.Target);
                _destinationAccessArray.Add(tfTweener.Dst);
            }
            else
            {
                _transformAccessArray[maskIndex] = tfTweener.Target;
                _destinationAccessArray[maskIndex] = tfTweener.Dst;
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
