using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Nextension.Tween
{
    internal class BasicFloatTweenChunk : AbsBasicTweenChunk<float, BasicFloatTweenChunk.Job>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override Job createNewJob()
        {
            return new Job(this);
        }

        [BurstCompile]
        public struct Job : IJobFor
        {
            [ReadOnly] private NativeArray<JobData<float>> _jobDatas;
            [ReadOnly] private NativeArray<byte> _mask;
            [WriteOnly] private NativeArray<float> _results;

            public Job(BasicFloatTweenChunk chunk)
            {
                _jobDatas = chunk._jobDatas;
                _results = chunk._results;
                _mask = chunk.mask;
            }

            public void Execute(int index)
            {
                if (NUtils.checkBitMask(_mask, index))
                {
                    var data = _jobDatas[index];
                    var currentTime = TweenStaticManager.currentTimeInJob.Data;
                    if (currentTime >= data.startTime)
                    {
                        var t = (currentTime - data.startTime) / data.duration;
                        BurstEaseUtils.ease(data.from, data.to, t, data.easeType, out var result);
                        _results[index] = result;
                    }
                }
            }
        }
    }
}
