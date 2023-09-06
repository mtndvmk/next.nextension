using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Jobs;

namespace Nextension.Tween
{
    internal abstract class AbsBasicTweenChunk<T, TJob> : AbsTweenChunk<BasicTweener<T>, TJob>
        where T : struct
        where TJob : struct, IJobFor
    {
        protected NativeArray<JobData<T>> _jobDatas;
        protected NativeArray<T> _results;

        public AbsBasicTweenChunk() : base()
        {
            _jobDatas = new NativeArray<JobData<T>>(ChunkSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            _results = new NativeArray<T>(ChunkSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        }
        protected sealed override void onAddNewTweener(BasicTweener<T> tweener)
        {
            _jobDatas[tweener.chunkIndex.maskIndex] = tweener.getJobData();
        }
        protected sealed override void onTweenerUpdated(int maskIndex)
        {
            var tweener = tweeners[maskIndex];
            tweener.invokeValueChanged(_results[maskIndex]);
        }
        public sealed override void dispose()
        {
            base.dispose();
            _jobDatas.Dispose();
            _results.Dispose();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected sealed override JobHandle onScheduleJob()
        {
            return _job.ScheduleParallel(ChunkSize, 64, default);
        }
    }
}