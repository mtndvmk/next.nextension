using Unity.Mathematics;
using UnityEngine;

namespace Nextension.Tween
{
    [DisallowMultipleComponent]
    public sealed class AutoAlternatePosition : AbsAutoAlternate<float3>
    {
        [SerializeField] private bool _isLocalSpace = true;

        protected override void setValue(float3 value)
        {
            if (_isLocalSpace)
            {
                transform.localPosition = value;
            }
            else
            {
                transform.position = value;
            }
        }
        protected override float3 getCurrentValue()
        {
            if (_isLocalSpace)
            {
                return transform.localPosition;
            }
            else
            {
                return transform.position;
            }
        }
        protected override NRunnableTweener onFromTo()
        {
            return NTween.moveTo(transform, _toValue, _timePerHalfCycle, _isLocalSpace);
        }
        protected override NRunnableTweener onToFrom()
        {
            return NTween.moveTo(transform, _fromValue, _timePerHalfCycle, _isLocalSpace);
        }
    }
}
