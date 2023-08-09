﻿using System;
using Unity.Mathematics;
using UnityEngine;

namespace Nextension.Tween
{
    internal class TransformFloat3Tweener : AbsTransformTweener<float3>
    {
        public TransformFloat3Tweener(Transform target, float3 destination, TweenType tweenType) : base(target, destination, tweenType)
        {
            this.destination = destination;
        }
        internal override void doCompleteOnStart()
        {
            if (tweenLoopType == TweenLoopType.Normal)
            {
                switch (tweenType)
                {
                    case TweenType.Transform_Local_Move:
                        target.localPosition = destination;
                        break;
                    case TweenType.Transform_World_Move:
                        target.position = destination;
                        break;
                    case TweenType.Transform_Local_Scale:
                        target.localScale = destination;
                        break;
                    default:
                        throw new NotImplementedException(tweenType.ToString());
                }
            }
        }
        public override JobData<float3> toJobData()
        {
            var jobData = base.toJobData();
            switch (tweenType)
            {
                case TweenType.Transform_Local_Move:
                    jobData.from = target.localPosition;
                    break;
                case TweenType.Transform_World_Move:
                    jobData.from = target.position;
                    break;
                case TweenType.Transform_Local_Scale:
                    jobData.from = target.localScale;
                    break;
                default:
                    throw new NotImplementedException(tweenType.ToString());
            }
            return jobData;
        }
    }
}