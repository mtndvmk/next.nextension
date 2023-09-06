using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Nextension.Tween
{
    internal class BasicFloat2TweenChunk : AbsBasicTweenChunk<float2, BasicFloat2TweenChunk.Job>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override Job createNewJob()
        {
            return new Job(this);
        }

        [BurstCompile]
        public struct Job : IJobFor
        {
            [ReadOnly] private NativeArray<JobData<float2>> _jobDatas;
            [ReadOnly] private NativeArray<byte> _mask;
            [WriteOnly] private NativeArray<float2> _results;

            public Job(BasicFloat2TweenChunk chunk)
            {
                _jobDatas = chunk._jobDatas;
                _results = chunk._results;
                _mask = chunk.mask;
            }

            public void Execute(int index)
            {
                if (NUtils.checkBitMask(_mask, index))
                {
                    var currentTime = TweenStaticManager.currentTimeInJob.Data;
                    var data = _jobDatas[index];
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
