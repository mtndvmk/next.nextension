using Unity.Jobs;
using UnityEngine.Jobs;

namespace Nextension.Tween
{
    internal abstract class AbsTransformTweenChunk<TJob, TJobData> : GenericTweenChunk<TJob, TJobData>
        where TJob : struct, IJobParallelForTransform
        where TJobData : struct
    {
        protected TransformAccessArray _transformAccessArray;

        public AbsTransformTweenChunk() : base()
        {
            _transformAccessArray = new TransformAccessArray(CHUNK_SIZE);
        }
        protected override void onAddNewTweener(NTweener tweener)
        {
            base.onAddNewTweener(tweener);
            var maskIndex = tweener.chunkIndex.maskIndex;
            var tfTweener = (ITransformTweener)tweener;
            if (maskIndex >= _transformAccessArray.length)
            {
                _transformAccessArray.Add(tfTweener.Target);
            }
            else
            {
                _transformAccessArray[maskIndex] = tfTweener.Target;
            }
        }

        protected sealed override JobHandle onScheduleJob()
        {
            return _job.ScheduleByRef(_transformAccessArray);
        }
        public override void dispose()
        {
            base.dispose();
            _transformAccessArray.Dispose();
        }
    }
}