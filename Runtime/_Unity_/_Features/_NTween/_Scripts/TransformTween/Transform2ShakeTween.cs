using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace Nextension.Tween
{
    internal static class Transform2ShakeTween<TValue> where TValue : unmanaged
    {
        internal sealed class Tweener : AbsShakeTweener<TValue>, ITransform2Tweener
        {
            internal Transform targetTf;
            internal Transform dstTf;
            public Transform Target => targetTf;
            public Transform Dst => dstTf;
            private readonly TransformTweenType _transformTweenType;

            public Tweener(Transform target, Transform destination, float4 range, TransformTweenType transformTweenType) : base(default, range)
            {
                targetTf = target;
                dstTf = destination;
                _transformTweenType = transformTweenType;
            }

            protected override void onResetState()
            {
                base.onResetState();
                NTweenUtils.applyValue(targetTf, _transformTweenType, origin);
            }
            internal override unsafe void invokeValueChanged(void* src)
            {
            }
            internal unsafe override void writeJobDataToAddr(void* dst)
            {
                origin = NTweenUtils.readValue<TValue>(targetTf, _transformTweenType);
                // origin is written each frame by ReadJob from the destination TransformAccessArray
                var data = new TransformShakeData<TValue>(_transformTweenType, new ShakeData<TValue>(getCommonJobData(), range, default));
                Unsafe.Write(dst, data);
            }
            internal override ushort getRunnerId()
            {
                return TweenRunnerId<Chunk>.id;
            }
        }

        internal class Chunk : AbsTransform2TweenChunk<TweenJob, TransformShakeData<TValue>>
        {
            private ReadJob _readJob;
            private bool _hasReadJob;

            protected override TweenJob createNewJob()
            {
                return new TweenJob(_jobDataNativeArr, _mask);
            }
            protected override JobHandle onScheduleReadJob()
            {
                if (!_hasReadJob)
                {
                    _readJob = new ReadJob(_jobDataNativeArr, _mask);
                    _hasReadJob = true;
                }
                return _readJob.ScheduleByRef(_destinationAccessArray);
            }
        }

        // Reads live destination transform values → writes origin directly into _jobDataNativeArr
        internal struct ReadJob : IJobParallelForTransform
        {
            [ReadOnly] private NativeArray<byte> _mask;
            [NativeDisableParallelForRestriction] private NativeArray<TransformShakeData<TValue>> _jobDataNativeArr;

            internal ReadJob(in NativeArray<TransformShakeData<TValue>> jobDataNativeArr, in NativeArray<byte> mask)
            {
                _jobDataNativeArr = jobDataNativeArr;
                _mask = mask;
            }

            [BurstCompile]
            public void Execute(int index, TransformAccess destTransform)
            {
                if (NUtils.checkBitMask(_mask, index))
                {
                    var current = _jobDataNativeArr[index];
                    var liveOrigin = NTweenUtils.readTransformAccessValue<TValue>(destTransform, current.transformTweenType);
                    _jobDataNativeArr[index] = new TransformShakeData<TValue>(
                        current.transformTweenType,
                        new ShakeData<TValue>(current.shakeData.common, current.shakeData.range, liveOrigin));
                }
            }
        }

        internal struct TweenJob : IJobParallelForTransform
        {
            [ReadOnly] private NativeArray<byte> _mask;
            [ReadOnly] private NativeArray<TransformShakeData<TValue>> _jobDataNativeArr;

            internal TweenJob(in NativeArray<TransformShakeData<TValue>> jobDataNativeArr, in NativeArray<byte> mask)
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
