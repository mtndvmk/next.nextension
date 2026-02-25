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
            private void applyValue(TValue value)
            {
                switch (_transformTweenType)
                {
                    case TransformTweenType.Local_Position:
                        _target.localPosition = NConverter.bitConvertWithoutChecks<TValue, Vector3>(value);
                        break;
                    case TransformTweenType.World_Position:
                        _target.position = NConverter.bitConvertWithoutChecks<TValue, Vector3>(value);
                        break;
                    case TransformTweenType.Local_Scale:
                        _target.localScale = NConverter.bitConvertWithoutChecks<TValue, Vector3>(value);
                        break;
                    case TransformTweenType.Uniform_Local_Scale:
                        var x = NConverter.bitConvertWithoutChecks<TValue, float>(value);
                        _target.localScale = new(x, x, x);
                        break;
                    case TransformTweenType.Local_Rotation:
                        _target.localRotation = NConverter.bitConvertWithoutChecks<TValue, Quaternion>(value);
                        break;
                    case TransformTweenType.World_Rotation:
                        _target.rotation = NConverter.bitConvertWithoutChecks<TValue, Quaternion>(value);
                        break;
                    default:
                        throw new NotImplementedException(_transformTweenType.ToString());
                }
            }
            protected override void onResetState()
            {
                base.onResetState();
                applyValue(origin);
            }
            public override TransformShakeData<TValue> getJobData()
            {
                origin = _transformTweenType switch
                {
                    TransformTweenType.Local_Position => NConverter.bitConvertWithoutChecks<Vector3, TValue>(_target.localPosition),
                    TransformTweenType.World_Position => NConverter.bitConvertWithoutChecks<Vector3, TValue>(_target.position),
                    TransformTweenType.Local_Scale => NConverter.bitConvertWithoutChecks<Vector3, TValue>(_target.localScale),
                    TransformTweenType.Uniform_Local_Scale => NConverter.bitConvertWithoutChecks<float, TValue>(_target.localScale.x),
                    TransformTweenType.Local_Rotation => NConverter.bitConvertWithoutChecks<Quaternion, TValue>(_target.localRotation),
                    TransformTweenType.World_Rotation => NConverter.bitConvertWithoutChecks<Quaternion, TValue>(_target.rotation),
                    _ => throw new NotImplementedException(_transformTweenType.ToString()),
                };
                var jobData = new TransformShakeData<TValue>(_transformTweenType, new ShakeData<TValue>(getCommonJobData(), range, origin));
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
                    var currentTime = common.updateMode == NTweener.UpdateMode.ScaleTime ? TweenStaticManager.currentTimeInJob.Data : TweenStaticManager.currentUnscaledTimeInJob.Data;
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
                        NTweenUtils.applyTransformAccessJobData(transformData.transformTweenType, transform, result);
                    }
                }
            }
        }
    }
}