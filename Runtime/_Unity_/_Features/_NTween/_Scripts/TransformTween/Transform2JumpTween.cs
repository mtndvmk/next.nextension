using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

namespace Nextension.Tween
{
    internal static class Transform2JumpTween<TValue> where TValue : unmanaged
    {
        internal sealed class Tweener : AbsJumpTweener<TValue>, ITransform2Tweener
        {
            internal Transform targetTf;
            internal Transform dstTf;
            public Transform Target => targetTf;
            public Transform Dst => dstTf;
            private readonly TransformTweenType _transformTweenType;

            public Tweener(Transform target, Transform destination, TValue jumpHeight, TransformTweenType transformTweenType) : base(default, default, jumpHeight)
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
            internal unsafe override void writeJobDataToAddr(void* dst)
            {
                origin = NTweenUtils.readValue<TValue>(targetTf, _transformTweenType);
                // destination is written each frame by ReadJob from the destination TransformAccessArray
                var data = new TransformJumpData<TValue>(_transformTweenType, new JumpData<TValue>(getCommonJobData(), origin, default, jumpHeight));
                Unsafe.Write(dst, data);
            }

            internal override ushort getRunnerId()
            {
                return TweenRunnerId<Chunk>.id;
            }

            internal override unsafe void invokeValueChanged(void* src)
            {
            }

        }

        internal class Chunk : AbsTransform2TweenChunk<TweenJob, TransformJumpData<TValue>>
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

        // Reads live destination transform values → writes destination directly into _jobDataNativeArr
        internal struct ReadJob : IJobParallelForTransform
        {
            [ReadOnly] private NativeArray<byte> _mask;
            [NativeDisableParallelForRestriction] private NativeArray<TransformJumpData<TValue>> _jobDataNativeArr;

            internal ReadJob(in NativeArray<TransformJumpData<TValue>> jobDataNativeArr, in NativeArray<byte> mask)
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
                    var dest = NTweenUtils.readTransformAccessValue<TValue>(destTransform, current.transformTweenType);
                    _jobDataNativeArr[index] = new TransformJumpData<TValue>(
                        current.transformTweenType,
                        new JumpData<TValue>(current.jumpData.common, current.jumpData.origin, dest, current.jumpData.jumpHeight));
                }
            }
        }

        internal struct TweenJob : IJobParallelForTransform
        {
            [ReadOnly] private NativeArray<byte> _mask;
            [ReadOnly] private NativeArray<TransformJumpData<TValue>> _jobDataNativeArr;

            internal TweenJob(in NativeArray<TransformJumpData<TValue>> jobDataNativeArr, in NativeArray<byte> mask)
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
                    var data = transformData.jumpData;
                    var common = data.common;
                    var currentTime = common.updateMode == NUpdateMode.ScaleTime ? TweenStaticManager.currentTimeInJob.Data : TweenStaticManager.currentUnscaledTimeInJob.Data;
                    if (currentTime >= common.startTime)
                    {
                        var deltaTime = currentTime - common.startTime;
                        var t = deltaTime / common.duration;
                        if (t >= 1f)
                        {
                            t = 1f;
                        }
                        NTweenUtils.ease(data.origin, data.destination, t, common.easeType, out var baseResult);
                        NTweenUtils.ease(default, data.jumpHeight, 4f * t * (1f - t), EaseType.Linear, out var jumpOffset);
                        NTweenUtils.applyTransformAccessJobData(transform, transformData.transformTweenType, NTweenUtils.addValue(baseResult, jumpOffset));
                    }
                }
            }
        }
    }
}
