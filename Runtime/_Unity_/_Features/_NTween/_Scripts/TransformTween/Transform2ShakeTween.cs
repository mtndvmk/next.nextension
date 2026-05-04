using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

namespace Nextension.Tween
{
    internal sealed class Transform2ShakeTween<TValue> where TValue : unmanaged
    {
        internal sealed class Tweener : AbsShakeTweener<TValue, TransformShakeData<TValue>>, ITransform2Tweener
        {
            private readonly Transform _target;
            private readonly Transform _destination;
            public Transform Target => _target;
            public Transform Destination => _destination;
            private readonly TransformTweenType _transformTweenType;

            public Tweener(Transform target, Transform destination, float range, TransformTweenType transformTweenType) : base(default, range, null)
            {
                _target = target;
                _destination = destination;
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
                        _target.localScale = new Vector3(x, x, x);
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
                // origin is written each frame by ReadJob from the destination TransformAccessArray
                return new TransformShakeData<TValue>(_transformTweenType, new ShakeData<TValue>(getCommonJobData(), range, default));
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

        internal class Chunk : AbsTransform2TweenChunk<Tweener, TweenJob, TransformShakeData<TValue>>
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
                    var liveOrigin = NTweenUtils.readTransformAccessValue<TValue>(current.transformTweenType, destTransform);
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
