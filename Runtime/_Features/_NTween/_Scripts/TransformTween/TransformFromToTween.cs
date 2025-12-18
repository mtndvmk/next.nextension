using System;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

namespace Nextension.Tween
{
    internal class TransformFromToTween<TValue> where TValue : unmanaged
    {
        internal sealed class Tweener : AbsFromToTweener<TValue, TransformFromToData<TValue>>, ITransformTweener
        {
            private readonly Transform _target;
            public Transform Target => _target;
            private readonly TransformTweenType _transformTweenType;

            public Tweener(Transform target, TValue destination, TransformTweenType transformTweenType) : base(default, destination, null)
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
                applyValue(from);
            }
            internal override void forceComplete()
            {
                applyValue(destination);
                invokeOnUpdate();
                invokeOnComplete();
            }
            public override TransformFromToData<TValue> getJobData()
            {
                var jobData = new TransformFromToData<TValue>()
                {
                    transformTweenType = _transformTweenType,
                    fromToData = new FromToData<TValue>()
                    {
                        common = getCommonJobData(),
                        to = destination,
                    }
                };
                from = _transformTweenType switch
                {
                    TransformTweenType.Local_Position => jobData.fromToData.from = NConverter.bitConvertWithoutChecks<Vector3, TValue>(_target.localPosition),
                    TransformTweenType.World_Position => jobData.fromToData.from = NConverter.bitConvertWithoutChecks<Vector3, TValue>(_target.position),
                    TransformTweenType.Local_Scale => jobData.fromToData.from = NConverter.bitConvertWithoutChecks<Vector3, TValue>(_target.localScale),
                    TransformTweenType.Uniform_Local_Scale => jobData.fromToData.from = NConverter.bitConvertWithoutChecks<float, TValue>(_target.localScale.x),
                    TransformTweenType.Local_Rotation => jobData.fromToData.from = NConverter.bitConvertWithoutChecks<Quaternion, TValue>(_target.localRotation),
                    TransformTweenType.World_Rotation => jobData.fromToData.from = NConverter.bitConvertWithoutChecks<Quaternion, TValue>(_target.rotation),
                    _ => throw new NotImplementedException(_transformTweenType.ToString()),
                };
                return jobData;
            }
            internal override AbsTweenRunner createRunner()
            {
                return new TweenRunner<Chunk>();
            }
            internal override ushort getRunnerId()
            {
                return TweenRunnerIdCache<TweenRunner<Chunk>>.id;
            }
        }
        internal class Chunk : AbsTransformTweenChunk<Tweener, Job, TransformFromToData<TValue>>
        {
            protected override Job createNewJob()
            {
                return new(_jobDataNativeArr, _mask);
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
                    var currentTime = common.updateMode == NTweener.UpdateMode.ScaleTime ? TweenStaticManager.currentTimeInJob.Data : TweenStaticManager.currentUnscaledTimeInJob.Data;
                    if (currentTime >= common.startTime)
                    {
                        var deltaTime = currentTime - common.startTime;
                        var t = deltaTime / common.duration;
                        NTweenUtils.ease(data.from, data.to, t, common.easeType, out var result);
                        NTweenUtils.applyTransformAccessJobData(transformData.transformTweenType, transform, result);
                    }
                }
            }
        }
    }
}