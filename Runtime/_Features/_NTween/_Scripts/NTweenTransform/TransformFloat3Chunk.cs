using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Jobs;

namespace Nextension.Tween
{
    internal class TransformFloat3Chunk : AbsTransformChunk<float3, TransformFloat3Tweener, TransformFloat3Chunk.Job>
    {
        protected override void onAddNewTweener(TransformFloat3Tweener tweener)
        {
            base.onAddNewTweener(tweener);
            var maskIndex = tweener.chunkIndex.maskIndex;
            jobDatas[maskIndex] = tweener.toJobData();
        }
        protected override Job createNewJob()
        {
            return new Job(this);
        }

        [BurstCompile]
        internal struct Job : IJobParallelForTransform
        {
            [ReadOnly] private NativeArray<NBitMask256> _mask;
            [ReadOnly] private NativeArray<JobData<float3>> _jobDatas;

            internal Job(TransformFloat3Chunk chunk)
            {
                _mask = chunk.mask;
                _jobDatas = chunk.jobDatas;
            }
            public void Execute(int index, TransformAccess transform)
            {
                var currentTime = TweenStaticManager.currentTimeInJob.Data;
                var data = _jobDatas[index];
                if (currentTime >= data.startTime)
                {
                    if (NUtils.checkBitMask(_mask[0], index))
                    {
                        var t = (currentTime - data.startTime) / data.duration;
                        if (data.tweenLoopType == TweenLoopType.Punch)
                        {
                            if (t < 0.5f)
                            {
                                t /= 0.5f;
                            }
                            else
                            {
                                t = (1 - t) / 0.5f;
                            }
                        }
                        BurstEaseUtils.ease(data.from, data.to, t, data.easeType, out var result);
                        apply(data.tweenType, transform, result);
                    }
                }
            }
            private void apply(TweenType tweenType, TransformAccess transform, float3 result)
            {
                switch (tweenType)
                {
                    case TweenType.Transform_Local_Move:
                        transform.localPosition = result;
                        break;
                    case TweenType.Transform_World_Move:
                        transform.position = result;
                        break;
                    case TweenType.Transform_Local_Scale:
                        transform.localScale = result;
                        break;
                }
            }
        }
    }
}