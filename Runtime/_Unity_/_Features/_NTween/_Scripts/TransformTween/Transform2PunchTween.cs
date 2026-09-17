using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

namespace Nextension.Tween
{
    internal static class Transform2PunchTween<TValue> where TValue : unmanaged
    {
        internal sealed class Tweener : AbsPunchTweener<TValue>, ITransform2Tweener
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
                // punchDestination is written each frame by ReadJob from the destination TransformAccessArray
                var data = new TransformPunchData<TValue>(_transformTweenType, new PunchData<TValue>(getCommonJobData(), origin, default));
                Unsafe.Write(dst, data);
            }
            internal override ushort getRunnerId()
            {
                return TweenRunnerId<Chunk>.id;
            }
        }

        internal class Chunk : AbsTransform2TweenChunk<TweenJob, TransformPunchData<TValue>>
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

        // Reads live destination transform values → writes punchDestination directly into _jobDataNativeArr
        internal struct ReadJob : IJobParallelForTransform
        {
            [ReadOnly] private NativeArray<byte> _mask;
            [NativeDisableParallelForRestriction] private NativeArray<TransformPunchData<TValue>> _jobDataNativeArr;

            internal ReadJob(in NativeArray<TransformPunchData<TValue>> jobDataNativeArr, in NativeArray<byte> mask)
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
                    var punchDest = NTweenUtils.readTransformAccessValue<TValue>(destTransform, current.transformTweenType);
                    _jobDataNativeArr[index] = new TransformPunchData<TValue>(
                        current.transformTweenType,
                        new PunchData<TValue>(current.punchData.common, current.punchData.origin, punchDest));
                }
            }
        }

        internal struct TweenJob : IJobParallelForTransform
        {
            [ReadOnly] private NativeArray<byte> _mask;
            [ReadOnly] private NativeArray<TransformPunchData<TValue>> _jobDataNativeArr;

            internal TweenJob(in NativeArray<TransformPunchData<TValue>> jobDataNativeArr, in NativeArray<byte> mask)
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
                    var data = transformData.punchData;
                    var common = data.common;
                    var currentTime = common.updateMode == NUpdateMode.ScaleTime ? TweenStaticManager.currentTimeInJob.Data : TweenStaticManager.currentUnscaledTimeInJob.Data;
                    if (currentTime >= data.common.startTime)
                    {
                        var deltaTime = currentTime - common.startTime;
                        var t = deltaTime / common.duration;
                        if (t < 0.5f)
                        {
                            t /= 0.5f;
                        }
                        else
                        {
                            t = (1 - t) / 0.5f;
                        }
                        NTweenUtils.ease(data.origin, data.punchDestination, t, common.easeType, out var result);
                        NTweenUtils.applyTransformAccessJobData(transform, transformData.transformTweenType, result);
                    }
                }
            }
        }
    }
}
