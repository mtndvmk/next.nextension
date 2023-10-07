using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Jobs;

namespace Nextension.Tween
{
    internal class ShakeValueTween<TValue> where TValue : unmanaged
    {
        internal sealed class Tweener : AbsShakeTweener<TValue, ShakeData<TValue>>
        {
            public Tweener(TValue origin, float range, Action<TValue> onValueChanged) : base(origin, range, onValueChanged)
            {
            }

            public override ShakeData<TValue> getJobData()
            {
                return new ShakeData<TValue>
                {
                    common = getCommonJobData(),
                    origin = origin,
                    range = range,
                };
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal override AbsTweenRunner createRunner()
            {
                return new TweenRunner<Chunk>();
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal override Type getRunnerType()
            {
                return typeof(TweenRunner<Chunk>);
            }
        }
        internal sealed class Chunk : AbsValueTweenChunk<TValue, Tweener, Job, ShakeData<TValue>>
        {
            protected override Job createNewJob()
            {
                return new(_jobDataNativeArr, _mask, _results);
            }
        }
        internal struct Job : IJobFor
        {
            [ReadOnly] private NativeArray<ShakeData<TValue>> _jobDataNativeArr;
            [ReadOnly] private NativeArray<byte> _mask;
            [WriteOnly] private NativeArray<TValue> _results;

            internal Job(NativeArray<ShakeData<TValue>> jobDataNativeArr, NativeArray<byte> mask, NativeArray<TValue> results)
            {
                _jobDataNativeArr = jobDataNativeArr;
                _results = results;
                _mask = mask;
            }

            public void Execute(int index)
            {
                if (NUtils.checkBitMask(_mask, index))
                {
                    var currentTime = TweenStaticManager.currentTimeInJob.Data;
                    var data = _jobDataNativeArr[index];
                    var common = data.common;
                    TValue result;
                    if (currentTime >= common.startTime)
                    {
                        var deltaTime = currentTime - common.startTime;
                        if (deltaTime < common.duration)
                        {
                            uint seed = NConverter.bitConvert<float, uint>(deltaTime + index) ^ 0x6E624EB7u;
                            result = NTweenUtils.randShakeValue<TValue>(seed, data.range);
                        }
                        else
                        {
                            result = data.origin;
                        }
                        _results[index] = result;
                    }
                }
            }
        }
    }
}