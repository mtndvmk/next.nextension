using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Jobs;
using Random = Unity.Mathematics.Random;

namespace Nextension.Tween
{
    internal class TransformFloat3Chunk : AbsTransformChunk<float3, TransformFloat3Tweener, TransformFloat3Chunk.Job>
    {
        protected override void onAddNewTweener(TransformFloat3Tweener tweener)
        {
            base.onAddNewTweener(tweener);
            var maskIndex = tweener.chunkIndex.maskIndex;
            jobDatas[maskIndex] = tweener.getJobData();
        }
        protected override Job createNewJob()
        {
            return new Job(this);
        }

        [BurstCompile]
        public struct Job : IJobParallelForTransform
        {
            [ReadOnly] private NativeArray<byte> _mask;
            [ReadOnly] private NativeArray<JobData<float3>> _jobDatas;

            internal Job(TransformFloat3Chunk chunk)
            {
                _mask = chunk.mask;
                _jobDatas = chunk.jobDatas;
            }
            public void Execute(int index, TransformAccess transform)
            {
                if (NUtils.checkBitMask(_mask, index))
                {
                    var currentTime = TweenStaticManager.currentTimeInJob.Data;
                    var data = _jobDatas[index];
                    var deltaTime = currentTime - data.startTime;
                    if (deltaTime >= 0)
                    {
                        float3 result;
                        if (data.tweenLoopType == TweenLoopType.Shake)
                        {
                            if (deltaTime < data.duration)
                            {
                                var randValue = new Random(NConverter.bitConvert<float, uint>(deltaTime) ^ 0x6E624EB7u).NextFloat3() - 0.5f;
                                result = randValue * data.to.x + data.from;
                            }
                            else
                            {
                                result = data.from;
                            }
                        }
                        else
                        {
                            var t = deltaTime / data.duration;
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
                            BurstEaseUtils.ease(data.from, data.to, t, data.easeType, out result);
                        }
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