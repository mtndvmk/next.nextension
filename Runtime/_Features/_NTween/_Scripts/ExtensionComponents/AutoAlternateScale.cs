﻿using Unity.Mathematics;
using UnityEngine;

namespace Nextension.Tween
{
    [DisallowMultipleComponent]
    public class AutoAlternateScale : AbsAutoAlternate<float3>
    {
        protected override float3 getCurrentValue()
        {
            return transform.localScale;
        }
        protected override void setValue(float3 value)
        {
            transform.localScale = value;
        }
        protected override NTweener onFromTo()
        {
            return NTween.scaleTo(transform, _toValue, _timePerHalfCycle);
        }
        protected override NTweener onToFrom()
        {
            return NTween.scaleTo(transform, _fromValue, _timePerHalfCycle);
        }
    }
}