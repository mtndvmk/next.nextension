using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

namespace Nextension.Tween
{
    internal static class Transform2FromToTween<TValue> where TValue : unmanaged
    {
        internal sealed class Tweener : AbsFromToTweener<TValue>, ITransform2Tweener
        {
            internal Transform targetTf;
            internal Transform dstTf;
            public Transform Target => targetTf;
            public Transform Dst => dstTf;

            private readonly TransformTweenType _transformTweenType;

            public Tweener(Transform target, Transform destination, TransformTweenType transformTweenType) : base(default, default)
            {
                targetTf = target;
                dstTf = destination;
                _transformTweenType = transformTweenType;
            }

            internal override unsafe void invokeValueChanged(void* src)
            {

            }

            internal override void forceComplete()
            {
                NTweenUtils.applyValue(targetTf, _transformTweenType, destination);
                invokeOnUpdate();
                invokeOnComplete();
            }

            internal unsafe override void writeJobDataToAddr(void* dst)
            {
                from = NTweenUtils.readValue<TValue>(targetTf, _transformTweenType);

                // 'to' is written each frame by ReadJob from the destination TransformAccessArray
                var data = new TransformFromToData<TValue>()
                {
                    transformTweenType = _transformTweenType,
                    fromToData = new FromToData<TValue>(getCommonJobData(), from, default),
                };

                Unsafe.Write(dst, data);
            }

            internal override ushort getRunnerId()
            {
                return TweenRunnerId<Chunk>.id;
            }
        }

        internal class Chunk : AbsTransform2TweenChunk<TweenJob, TransformFromToData<TValue>>
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

        // Reads live destination transform values → writes 'to' directly into _jobDataNativeArr
        internal struct ReadJob : IJobParallelForTransform
        {
            [ReadOnly] private NativeArray<byte> _mask;
            [NativeDisableParallelForRestriction] private NativeArray<TransformFromToData<TValue>> _jobDataNativeArr;

            internal ReadJob(in NativeArray<TransformFromToData<TValue>> jobDataNativeArr, in NativeArray<byte> mask)
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
                    var to = NTweenUtils.readTransformAccessValue<TValue>(destTransform, current.transformTweenType);
                    _jobDataNativeArr[index] = new TransformFromToData<TValue>()
                    {
                        transformTweenType = current.transformTweenType,
                        fromToData = new FromToData<TValue>(current.fromToData.common, current.fromToData.from, to),
                    };
                }
            }
        }

        internal struct TweenJob : IJobParallelForTransform
        {
            [ReadOnly] private NativeArray<byte> _mask;
            [ReadOnly] private NativeArray<TransformFromToData<TValue>> _jobDataNativeArr;

            internal TweenJob(in NativeArray<TransformFromToData<TValue>> jobDataNativeArr, in NativeArray<byte> mask)
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
                    var data = transformData.fromToData;
                    var common = data.common;
                    var currentTime = common.updateMode == NUpdateMode.ScaleTime ? TweenStaticManager.currentTimeInJob.Data : TweenStaticManager.currentUnscaledTimeInJob.Data;
                    if (currentTime >= common.startTime)
                    {
                        var deltaTime = currentTime - common.startTime;
                        var t = deltaTime / common.duration;
                        NTweenUtils.ease(data.from, data.to, t, common.easeType, out var result);
                        NTweenUtils.applyTransformAccessJobData(transform, transformData.transformTweenType, result);
                    }
                }
            }
        }
    }
}
