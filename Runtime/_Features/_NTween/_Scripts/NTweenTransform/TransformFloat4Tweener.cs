using System;
using Unity.Mathematics;
using UnityEngine;

namespace Nextension.Tween
{
    internal class TransformFloat4Tweener : AbsTransformTweener<float4>
    {
        public TransformFloat4Tweener(Transform target, float4 destination, TweenType tweenType) : base(target, destination, tweenType)
        {
            this.destination = destination;
        }
        internal override void doCompleteOnStart()
        {
            switch (tweenType)
            {
                case TweenType.Transform_Local_Rotate:
                    target.localRotation = new quaternion(destination);
                    break;
                case TweenType.Transform_World_Rotate:
                    target.rotation = new quaternion(destination);
                    break;
                default:
                    throw new NotImplementedException(tweenType.ToString());
            }
        }
        public override JobData<float4> getJobData()
        {
            var jobData = base.getJobData();
            Quaternion quaternion;
            switch (tweenType)
            {
                case TweenType.Transform_Local_Rotate:
                    quaternion = target.localRotation;
                    jobData.from = quaternion.toFloat4();
                    break;
                case TweenType.Transform_World_Rotate:
                    quaternion = target.rotation;
                    jobData.from = quaternion.toFloat4();
                    break;
                default:
                    throw new NotImplementedException(tweenType.ToString());
            }
            return jobData;
        }
    }
}