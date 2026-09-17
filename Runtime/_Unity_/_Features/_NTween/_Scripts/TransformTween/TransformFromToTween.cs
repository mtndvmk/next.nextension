using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

namespace Nextension.Tween
{
    internal static class TransformFromToTween<TValue> where TValue : unmanaged
    {
        internal sealed class Tweener : AbsFromToTweener<TValue>, ITransformTweener
        {
            internal Transform targetTf;
            public Transform Target => targetTf;
            private readonly TransformTweenType _transformTweenType;

            public Tweener(Transform target, TValue destination, TransformTweenType transformTweenType) : base(default, destination)
            {
                this.targetTf = target;
                _transformTweenType = transformTweenType;
            }

            internal override unsafe void invokeValueChanged(void* src)
            {

            }

            protected override void onResetState()
            {
                base.onResetState();
                NTweenUtils.applyValue(targetTf, _transformTweenType, from);
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
                var data = new TransformFromToData<TValue>()
                {
                    transformTweenType = _transformTweenType,
                    fromToData = new FromToData<TValue>(getCommonJobData(), from, destination),
                };

                Unsafe.Write(dst, data);
            }
            internal override ushort getRunnerId()
            {
                return TweenRunnerId<Chunk>.id;
            }
        }
        internal class Chunk : AbsTransformTweenChunk<Job, TransformFromToData<TValue>>
        {
            protected override Job createNewJob()
            {
                return new Job(_jobDataNativeArr, _mask);
            }
        }
        internal struct Job : IJobParallelForTransform
        {
            [ReadOnly] private NativeArray<byte> _mask;
            [ReadOnly] private NativeArray<TransformFromToData<TValue>> _jobDataNativeArr;

            internal Job(in NativeArray<TransformFromToData<TValue>> jobDataNativeArr, in NativeArray<byte> mask)
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