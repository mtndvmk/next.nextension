using System;
using System.Runtime.CompilerServices;
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
            internal override void forceComplete()
            {
                switch (_transformTweenType)
                {
                    case TransformTweenType.Local_Position:
                        _target.localPosition = NConverter.bitConvert<TValue, Vector3>(destination);
                        break;
                    case TransformTweenType.World_Position:
                        _target.position = NConverter.bitConvert<TValue, Vector3>(destination);
                        break;
                    case TransformTweenType.Local_Scale:
                        _target.localScale = NConverter.bitConvert<TValue, Vector3>(destination);
                        break;

                    case TransformTweenType.Local_Rotation:
                        _target.localRotation = NConverter.bitConvert<TValue, Quaternion>(destination);
                        break;
                    case TransformTweenType.World_Rotation:
                        _target.rotation = NConverter.bitConvert<TValue, Quaternion>(destination);
                        break;
                    default:
                        throw new NotImplementedException(_transformTweenType.ToString());
                }
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
                    TransformTweenType.Local_Position => jobData.fromToData.from = NConverter.bitConvert<Vector3, TValue>(_target.localPosition),
                    TransformTweenType.World_Position => jobData.fromToData.from = NConverter.bitConvert<Vector3, TValue>(_target.position),
                    TransformTweenType.Local_Scale => jobData.fromToData.from = NConverter.bitConvert<Vector3, TValue>(_target.localScale),
                    TransformTweenType.Local_Rotation => jobData.fromToData.from = NConverter.bitConvert<Quaternion, TValue>(_target.localRotation),
                    TransformTweenType.World_Rotation => jobData.fromToData.from = NConverter.bitConvert<Quaternion, TValue>(_target.rotation),
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
            internal override Type getRunnerType()
            {
                return typeof(TweenRunner<Chunk>);
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

            internal Job(NativeArray<TransformFromToData<TValue>> jobDataNativeArr, NativeArray<byte> mask)
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
                    var data = transformData.fromToData;
                    var common = data.common;
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