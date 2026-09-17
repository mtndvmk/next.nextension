using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

namespace Nextension.Tween
{
    internal sealed class TransformJumpTween<TValue> where TValue : unmanaged
    {
        internal sealed class Tweener : AbsJumpTweener<TValue>, ITransformTweener
        {
            internal Transform targetTf;
            public Transform Target => targetTf;
            private readonly TransformTweenType _transformTweenType;
            public Tweener(Transform target, TValue destination, TValue jumpHeight, TransformTweenType transformTweenType) : base(default, destination, jumpHeight)
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
                var data = new TransformJumpData<TValue>(_transformTweenType, new JumpData<TValue>(getCommonJobData(), origin, destination, jumpHeight));
                Unsafe.Write(dst, data);
            }
            internal override ushort getRunnerId()
            {
                return TweenRunnerId<Chunk>.id;
            }
        }
        internal class Chunk : AbsTransformTweenChunk<Job, TransformJumpData<TValue>>
        {
            protected override Job createNewJob()
            {
                return new Job(_jobDataNativeArr, _mask);
            }
        }
        internal struct Job : IJobParallelForTransform
        {
            [ReadOnly] private NativeArray<byte> _mask;
            [ReadOnly] private NativeArray<TransformJumpData<TValue>> _jobDataNativeArr;

            internal Job(in NativeArray<TransformJumpData<TValue>> jobDataNativeArr, in NativeArray<byte> mask)
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
                        NTweenUtils.ease(default(TValue), data.jumpHeight, 4f * t * (1f - t), EaseType.Linear, out var jumpOffset);
                        NTweenUtils.applyTransformAccessJobData(transform, transformData.transformTweenType, NTweenUtils.addValue(baseResult, jumpOffset));
                    }
                }
            }
        }
    }
}
