using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace Nextension.Tween
{
    internal static class TransformShakeTween<TValue> where TValue : unmanaged
    {
        internal sealed class Tweener : AbsShakeTweener<TValue>, ITransformTweener
        {
            internal Transform targetTf;
            public Transform Target => targetTf;
            private readonly TransformTweenType _transformTweenType;
            public Tweener(Transform target, float4 range, TransformTweenType transformTweenType) : base(default, range)
            {
                targetTf = target;
                _transformTweenType = transformTweenType;
            }
            internal override unsafe void invokeValueChanged(void* src)
            {
            }
            protected override void onResetState()
            {
                base.onResetState();
                NTweenUtils.applyValue(targetTf, _transformTweenType, origin);
            }
            internal unsafe override void writeJobDataToAddr(void* dst)
            {
                origin = NTweenUtils.readValue<TValue>(targetTf, _transformTweenType);
                var data = new TransformShakeData<TValue>(_transformTweenType, new ShakeData<TValue>(getCommonJobData(), range, origin));
                Unsafe.Write(dst, data);
            }
            internal override ushort getRunnerId()
            {
                return TweenRunnerId<Chunk>.id;
            }
        }
        internal class Chunk : AbsTransformTweenChunk<Job, TransformShakeData<TValue>>
        {
            protected override Job createNewJob()
            {
                return new Job(_jobDataNativeArr, _mask);
            }
        }
        internal struct Job : IJobParallelForTransform
        {
            [ReadOnly] private NativeArray<byte> _mask;
            [ReadOnly] private NativeArray<TransformShakeData<TValue>> _jobDataNativeArr;

            internal Job(in NativeArray<TransformShakeData<TValue>> jobDataNativeArr, in NativeArray<byte> mask)
            {
                _jobDataNativeArr = jobDataNativeArr;
                _mask = mask;
            }

            [BurstCompile]
            public void Execute(int index, TransformAccess transform)
            {
                if (NUtils.checkBitMask(_mask, index))
                {
                    var transformData = _jobDataNativeArr[index];
                    var data = transformData.shakeData;
                    var common = data.common;
                    var currentTime = common.updateMode == NUpdateMode.ScaleTime ? TweenStaticManager.currentTimeInJob.Data : TweenStaticManager.currentUnscaledTimeInJob.Data;
                    if (currentTime >= data.common.startTime)
                    {
                        TValue result;
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
                        NTweenUtils.applyTransformAccessJobData(transform, transformData.transformTweenType, result);
                    }
                }
            }
        }
    }
}