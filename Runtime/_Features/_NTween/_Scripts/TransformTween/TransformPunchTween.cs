using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

namespace Nextension.Tween
{
    internal sealed class TransformPunchTween<TValue> where TValue : unmanaged
    {
        internal sealed class Tweener : AbsPunchTweener<TValue, TransformPunchData<TValue>>, ITransformTweener
        {
            private readonly Transform _target;
            public Transform Target => _target;
            private readonly TransformTweenType _transformTweenType;
            public Tweener(Transform target, TValue punchDestination, TransformTweenType transformTweenType) : base(default, punchDestination, null)
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
            public override TransformPunchData<TValue> getJobData()
            {
                var jobData = new TransformPunchData<TValue>()
                {
                    transformTweenType = _transformTweenType,
                    punchData = new PunchData<TValue>()
                    {
                        common = getCommonJobData(),
                        punchDestination = punchDestination,
                    }
                };
                origin = _transformTweenType switch
                {
                    TransformTweenType.Local_Position => jobData.punchData.origin = NConverter.bitConvertWithoutChecks<Vector3, TValue>(_target.localPosition),
                    TransformTweenType.World_Position => jobData.punchData.origin = NConverter.bitConvertWithoutChecks<Vector3, TValue>(_target.position),
                    TransformTweenType.Local_Scale => jobData.punchData.origin = NConverter.bitConvertWithoutChecks<Vector3, TValue>(_target.localScale),
                    TransformTweenType.Uniform_Local_Scale => jobData.punchData.origin = NConverter.bitConvertWithoutChecks<float, TValue>(_target.localScale.x),
                    TransformTweenType.Local_Rotation => jobData.punchData.origin = NConverter.bitConvertWithoutChecks<Quaternion, TValue>(_target.localRotation),
                    TransformTweenType.World_Rotation => jobData.punchData.origin = NConverter.bitConvertWithoutChecks<Quaternion, TValue>(_target.rotation),
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
        internal class Chunk : AbsTransformTweenChunk<Tweener, Job, TransformPunchData<TValue>>
        {
            protected override Job createNewJob()
            {
                return new(_jobDataNativeArr, _mask);
            }
        }
        internal struct Job : IJobParallelForTransform
        {
            [ReadOnly] private NativeArray<byte> _mask;
            [ReadOnly] private NativeArray<TransformPunchData<TValue>> _jobDataNativeArr;

            internal Job(in NativeArray<TransformPunchData<TValue>> jobDataNativeArr, in NativeArray<byte> mask)
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
                    var currentTime = common.updateMode == NTweener.UpdateMode.ScaleTime ? TweenStaticManager.currentTimeInJob.Data : TweenStaticManager.currentUnscaledTimeInJob.Data;
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
                        NTweenUtils.applyTransformAccessJobData(transformData.transformTweenType, transform, result);
                    }
                }
            }
        }
    }
}