using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

namespace Nextension.Tween
{
    internal sealed class TransformShakeTween<TValue> where TValue : unmanaged
    {
        internal sealed class Tweener : AbsShakeTweener<TValue, TransformShakeData<TValue>>, ITransformTweener
        {
            private readonly Transform _target;
            public Transform Target => _target;
            private readonly TransformTweenType _transformTweenType;
            public Tweener(Transform target, float range, TransformTweenType transformTweenType) : base(default, range, null)
            {
                this._target = target;
                _transformTweenType = transformTweenType;
            }
            public override TransformShakeData<TValue> getJobData()
            {
                var jobData = new TransformShakeData<TValue>()
                {
                    transformTweenType = _transformTweenType,
                    shakeData = new ShakeData<TValue>()
                    {
                        common = getCommonJobData(),
                        range = range,
                    }
                };
                origin = _transformTweenType switch
                {
                    TransformTweenType.Local_Position => jobData.shakeData.origin = NConverter.bitConvert<Vector3, TValue>(_target.localPosition),
                    TransformTweenType.World_Position => jobData.shakeData.origin = NConverter.bitConvert<Vector3, TValue>(_target.position),
                    TransformTweenType.Local_Scale => jobData.shakeData.origin = NConverter.bitConvert<Vector3, TValue>(_target.localScale),
                    TransformTweenType.Local_Rotation => jobData.shakeData.origin = NConverter.bitConvert<Quaternion, TValue>(_target.localRotation),
                    TransformTweenType.World_Rotation => jobData.shakeData.origin = NConverter.bitConvert<Quaternion, TValue>(_target.rotation),
                    _ => throw new NotImplementedException(_transformTweenType.ToString()),
                };
                return jobData;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal override AbsTweenRunner createRunner()
            {
                return new TweenRunner<Chunk>();
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal override ushort getRunnerId()
            {
                return TweenRunnerIdCache<TweenRunner<Chunk>>.id;
            }
        }
        internal class Chunk : AbsTransformTweenChunk<Tweener, Job, TransformShakeData<TValue>>
        {
            protected override Job createNewJob()
            {
                return new(_jobDataNativeArr, _mask);
            }
        }
        internal struct Job : IJobParallelForTransform
        {
            [ReadOnly] private NativeArray<byte> _mask;
            [ReadOnly] private NativeArray<TransformShakeData<TValue>> _jobDataNativeArr;

            internal Job(NativeArray<TransformShakeData<TValue>> jobDataNativeArr, NativeArray<byte> mask)
            {
                _jobDataNativeArr = jobDataNativeArr;
                _mask = mask;
            }

            [BurstCompile]
            public void Execute(int index, TransformAccess transform)
            {
                if (NUtils.checkBitMask(_mask, index))
                {
                    var currentTime = TweenStaticManager.currentTimeInJob.Data;
                    var transformData = _jobDataNativeArr[index];
                    var data = transformData.shakeData;
                    var common = data.common;
                    if (currentTime >= data.common.startTime)
                    {
                        TValue result;
                        var deltaTime = currentTime - common.startTime;
                        if (deltaTime < common.duration)
                        {
                            uint seed = NConverter.bitConvert<float, uint>(deltaTime + index) ^ 0x6E624EB7u;
                            result = NTweenUtils.addValue(data.origin, NTweenUtils.randShakeValue<TValue>(seed, data.range));
                        }
                        else
                        {
                            result = data.origin;
                        }
                        NTweenUtils.applyTransformAccessJobData(transformData.transformTweenType, transform, result);
                    }
                }
            }
        }
    }
}