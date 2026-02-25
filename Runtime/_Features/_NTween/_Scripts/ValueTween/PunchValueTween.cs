using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Nextension.Tween
{

    internal class PunchValueTween<TValue> where TValue : unmanaged
    {
        internal sealed class Tweener : AbsPunchTweener<TValue, PunchData<TValue>>
        {
            public Tweener(TValue origin, TValue punchDestination, Action<TValue> onValueChanged) : base(origin, punchDestination, onValueChanged)
            {
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override PunchData<TValue> getJobData()
            {
                return new PunchData<TValue>(getCommonJobData(), origin, punchDestination);
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
        internal sealed class Chunk : AbsValueTweenChunk<TValue, Tweener, Job, PunchData<TValue>>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected override Job createNewJob()
            {
                return new(_jobDataNativeArr, _mask, _results);
            }
        }
        internal struct Job : IJobFor
        {
            [ReadOnly] private NativeArray<PunchData<TValue>> _jobDataNativeArr;
            [ReadOnly] private NativeArray<byte> _mask;
            [WriteOnly] private NativeArray<TValue> _results;

            internal Job(NativeArray<PunchData<TValue>> jobDataNativeArr, NativeArray<byte> mask, NativeArray<TValue> results)
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
                        if (t < 0.5f)
                        {
                            t /= 0.5f;
                        }
                        else
                        {
                            t = (1 - t) / 0.5f;
                        }
                        NTweenUtils.ease(data.origin, data.punchDestination, t, common.easeType, out var result);
                        _results[index] = result;
                    }
                }
            }
        }
    }
}