using Unity.Collections;
using Unity.Jobs;

namespace Nextension.Tween
{
    internal abstract class AbsValueTweenChunk<TValue, TJob, TJobData> : GenericTweenChunk<TJob, TJobData>
        where TValue : unmanaged
        where TJob : struct, IJobFor
        where TJobData : struct
    {
        protected NativeArray<TValue> _results;

        public AbsValueTweenChunk() : base()
        {
            _results = new NativeArray<TValue>(CHUNK_SIZE, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        }

        protected unsafe override void onTweenerUpdated(int index)
        {
            base.onTweenerUpdated(index);
            _tweeners[index].invokeValueChanged(_results.getUnsafePtr(index));
        }

        public sealed override void dispose()
        {
            base.dispose();
            _results.Dispose();
        }
        protected sealed override JobHandle onScheduleJob()
        {
            return _job.ScheduleParallelByRef(CHUNK_SIZE, 64, default);
        }
    }
}