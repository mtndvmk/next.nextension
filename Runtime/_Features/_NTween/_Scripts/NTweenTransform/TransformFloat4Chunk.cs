using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Jobs;

namespace Nextension.Tween
{
    internal class TransformFloat4Chunk : AbsTransformChunk<float4, TransformFloat4Tweener, TransformFloat4Chunk.Job>
    {
        protected override void onAddNewTweener(TransformFloat4Tweener tweener)
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
            [ReadOnly] private NativeArray<JobData<float4>> _jobDatas;

            internal Job(TransformFloat4Chunk chunk)
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
                    if (currentTime >= data.startTime)
                    {
                        var t = (currentTime - data.startTime) / data.duration;
                        BurstEaseUtils.ease(data.from, data.to, t, data.easeType, out var result);
                        apply(data.tweenType, transform, result);
                    }
                }
            }
            private void apply(TweenType tweenType, TransformAccess transform, float4 result)
            {
                switch (tweenType)
                {
                    case TweenType.Transform_Local_Rotate:
                        transform.localRotation = new quaternion(result);
                        break;
                    case TweenType.Transform_World_Rotate:
                        transform.rotation = new quaternion(result);
                        break;
                }
            }
        }
    }
}