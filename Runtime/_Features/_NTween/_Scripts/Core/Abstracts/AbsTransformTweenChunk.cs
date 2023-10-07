using System.Runtime.CompilerServices;
using Unity.Jobs;
using UnityEngine.Jobs;

namespace Nextension.Tween
{
    internal abstract class AbsTransformTweenChunk<TTweener, TJob, TJobData> : GenericTweenChunk<TTweener, TJob, TJobData>
        where TTweener : GenericNRunnableTweener<TJobData>, ITransformTweener
        where TJob : struct, IJobParallelForTransform
        where TJobData : struct
    {
        protected TransformAccessArray _transformAccessArray;

        public AbsTransformTweenChunk() : base()
        {
            _transformAccessArray = new TransformAccessArray(ChunkSize);
        }
        protected override void onAddNewTweener(TTweener tweener)
        {
            base.onAddNewTweener(tweener);
            var maskIndex = tweener.chunkIndex.maskIndex;
            if (maskIndex >= _transformAccessArray.length)
            {
                _transformAccessArray.Add(tweener.Target);
            }
            else
            {
                _transformAccessArray[maskIndex] = tweener.Target;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected sealed override JobHandle onScheduleJob()
        {
            return _job.Schedule(_transformAccessArray);
        }
        public override void dispose()
        {
            base.dispose();
            _transformAccessArray.Dispose();
        }
    }
}