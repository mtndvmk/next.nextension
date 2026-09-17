using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Nextension.Tween
{

    internal class PunchValueTween<TValue> where TValue : unmanaged
    {
        internal sealed class Tweener : AbsPunchTweener<TValue>
        {
            private Action<TValue> _onValueChanged;
            public Tweener(TValue origin, TValue punchDestination, Action<TValue> onValueChanged) : base(origin, punchDestination)
            {
                _onValueChanged = onValueChanged;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal unsafe override void writeJobDataToAddr(void* dst)
            {
                var data = new PunchData<TValue>(getCommonJobData(), origin, punchDestination);
                Unsafe.Write(dst, data);
            }
            internal override ushort getRunnerId()
            {
                return TweenRunnerId<Chunk>.id;
            }

            internal override unsafe void invokeValueChanged(void* src)
            {
                _onValueChanged?.Invoke(Unsafe.Read<TValue>(src));
            }
        }
        internal sealed class Chunk : AbsValueTweenChunk<TValue, Job, PunchData<TValue>>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected override Job createNewJob()
            {
                return new Job(_jobDataNativeArr, _mask, _results);
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
                    var currentTime = common.updateMode == NUpdateMode.ScaleTime ? TweenStaticManager.currentTimeInJob.Data : TweenStaticManager.currentUnscaledTimeInJob.Data;
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