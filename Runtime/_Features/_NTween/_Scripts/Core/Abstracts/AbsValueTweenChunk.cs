using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Jobs;

namespace Nextension.Tween
{
    internal abstract class AbsValueTweenChunk<TValue, TTweener, TJob, TJobData> : GenericTweenChunk<TTweener, TJob, TJobData>
        where TValue : unmanaged
        where TTweener : AbsValueTweener<TValue, TJobData>
        where TJob : struct, IJobFor
        where TJobData : struct
    {
        protected NativeArray<TValue> _results;

        public AbsValueTweenChunk() : base()
        {
            _results = new NativeArray<TValue>(ChunkSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        }

        protected override void onTweenerUpdated(int maskIndex)
        {
            base.onTweenerUpdated(maskIndex);
            _tweeners[maskIndex].invokeValueChanged(_results[maskIndex]);
        }

        public sealed override void dispose()
        {
            base.dispose();
            _results.Dispose();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected sealed override JobHandle onScheduleJob()
        {
            return _job.ScheduleParallelByRef(ChunkSize, 64, default);
        }
    }
}