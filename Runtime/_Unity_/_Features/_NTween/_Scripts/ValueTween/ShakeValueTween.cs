using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Nextension.Tween
{
    internal class ShakeValueTween<TValue> where TValue : unmanaged
    {
        internal sealed class Tweener : AbsShakeTweener<TValue>
        {
            private Action<TValue> _onValueChanged;
            public Tweener(TValue origin, float4 range, Action<TValue> onValueChanged) : base(origin, range)
            {
                _onValueChanged = onValueChanged;
            }

            internal unsafe override void writeJobDataToAddr(void* dst)
            {
                var data = new ShakeData<TValue>(getCommonJobData(), range, origin);
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
        internal sealed class Chunk : AbsValueTweenChunk<TValue, Job, ShakeData<TValue>>
        {
            protected override Job createNewJob()
            {
                return new Job(_jobDataNativeArr, _mask, _results);
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
                    var data = _jobDataNativeArr[index];
                    var common = data.common;
                    TValue result;
                    var currentTime = common.updateMode == NUpdateMode.ScaleTime ? TweenStaticManager.currentTimeInJob.Data : TweenStaticManager.currentUnscaledTimeInJob.Data;
                    if (currentTime >= common.startTime)
                    {
                        var deltaTime = currentTime - common.startTime;
                        if (deltaTime < common.duration)
                        {
                            uint seed = NConverter.bitConvertWithoutChecks<float, uint>(deltaTime + index) ^ 0x6E624EB7u;
                            result = NTweenUtils.addValue(data.origin, NTweenUtils.randShakeValue<TValue>(seed, data.range));
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