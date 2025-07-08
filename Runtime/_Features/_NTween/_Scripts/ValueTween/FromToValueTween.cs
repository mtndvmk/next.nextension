﻿using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Nextension.Tween
{
    internal class FromToValueTween<TValue> where TValue : unmanaged
    {
        internal class Tweener : AbsFromToTweener<TValue, FromToData<TValue>>
        {
            public Tweener(TValue from, TValue to, Action<TValue> onValueChanged) : base(from, to, onValueChanged)
            {
                this.from = from;
                this.destination = to;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override FromToData<TValue> getJobData()
            {
                return new FromToData<TValue>()
                {
                    common = getCommonJobData(),
                    from = this.from,
                    to = this.destination,
                };
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal override AbsTweenRunner createRunner()
            {
                return new TweenRunner<Chunk>();
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal override ushort getRunnerId()
            {
                return TweenRunnerIdCache<TweenRunner<Chunk>>.id;
            }
        }
        internal sealed class Chunk : AbsValueTweenChunk<TValue, Tweener, Job, FromToData<TValue>>
        {
            protected override Job createNewJob()
            {
                return new(_jobDataNativeArr, _mask, _results);
            }
        }
        internal struct Job : IJobFor
        {
            [ReadOnly] private NativeArray<FromToData<TValue>> _jobDataNativeArr;
            [ReadOnly] private NativeArray<byte> _mask;
            [WriteOnly] private NativeArray<TValue> _results;

            internal Job(NativeArray<FromToData<TValue>> jobDataNativeArr, NativeArray<byte> mask, NativeArray<TValue> results)
            {
                _jobDataNativeArr = jobDataNativeArr;
                _results = results;
                _mask = mask;
            }

            [BurstCompile]
            public void Execute(int index)
            {
                if (NUtils.checkBitMask(_mask, index))
                {
                    var data = _jobDataNativeArr[index];
                    var common = data.common;
                    var currentTime = common.updateMode == NTweener.UpdateMode.ScaleTime ? TweenStaticManager.currentTimeInJob.Data : TweenStaticManager.currentUnscaledTimeInJob.Data;
                    if (currentTime >= common.startTime)
                    {
                        var t = (currentTime - common.startTime) / common.duration;
                        NTweenUtils.ease(data.from, data.to, t, common.easeType, out var result);
                        _results[index] = result;
                    }
                }
            }
        }
    }
}