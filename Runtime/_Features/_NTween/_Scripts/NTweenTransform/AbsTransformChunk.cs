using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Jobs;

namespace Nextension.Tween
{
    internal abstract class AbsTransformChunk<T, TNTweener, TJob> : AbsTweenChunk<TNTweener, TJob>
        where T : struct
        where TNTweener : AbsTransformTweener<T>
        where TJob : struct, IJobParallelForTransform
    {
        public TransformAccessArray transformAccessArray;
        public NativeArray<JobData<T>> jobDatas;

        public AbsTransformChunk() : base()
        {
            transformAccessArray = new TransformAccessArray(ChunkSize);
            jobDatas = new NativeArray<JobData<T>>(transformAccessArray.capacity, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        }
        protected override void onAddNewTweener(TNTweener tweener)
        {
            var maskIndex = tweener.chunkIndex.maskIndex;
            if (maskIndex >= transformAccessArray.length)
            {
                transformAccessArray.Add(tweener.target);
            }
            else
            {
                transformAccessArray[maskIndex] = tweener.target;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override JobHandle onScheduleJob()
        {
            return _job.Schedule(transformAccessArray);
        }
        public override void dispose()
        {
            base.dispose();
            transformAccessArray.Dispose();
            jobDatas.Dispose();
        }
    }
}