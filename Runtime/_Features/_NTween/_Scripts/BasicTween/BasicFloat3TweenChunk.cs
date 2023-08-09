﻿using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Nextension.Tween
{
    internal class BasicFloat3TweenChunk : AbsBasicTweenChunk<float3, BasicFloat3TweenChunk.Job>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override Job createNewJob()
        {
            return new Job(this);
        }

        [BurstCompile]
        public struct Job : IJobFor
        {
            [ReadOnly] private NativeArray<JobData<float3>> _jobDatas;
            [ReadOnly] private NativeArray<NBitMask256> _mask;
            [WriteOnly] private NativeArray<float3> _results;

            public Job(BasicFloat3TweenChunk chunk)
            {
                _jobDatas = chunk._jobDatas;
                _results = chunk._results;
                _mask = chunk.mask;
            }

            public void Execute(int index)
            {
                var currentTime = TweenStaticManager.currentTimeInJob.Data;
                var data = _jobDatas[index];
                if (currentTime >= data.startTime)
                {
                    if (NUtils.checkBitMask(_mask[0], index))
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