using Unity.Mathematics;
using UnityEngine;

namespace Nextension.Tween
{
    [DisallowMultipleComponent]
    public sealed class AutoAlternateRotation : AbsAutoAlternate<float3>
    {
        [SerializeField] private bool _isLocalSpace = true;

        protected override float3 getCurrentValue()
        {
            if (_isLocalSpace)
            {
                return transform.localEulerAngles;
            }
            else
            {
                return transform.eulerAngles;
            }
        }
        protected override void setValue(float3 value)
        {
            if (_isLocalSpace)
            {
                transform.localEulerAngles = value;
            }
            else
            {
                transform.eulerAngles = value;
            }
        }
        protected override NTweener onFromTo()
        {
            return NTween.rotateTo(transform, quaternion.EulerXYZ(Mathf.Deg2Rad * _toValue).value, _timePerHalfCycle, _isLocalSpace);
        }
        protected override NTweener onToFrom()
        {
            return NTween.rotateTo(transform, quaternion.EulerXYZ(Mathf.Deg2Rad * _fromValue).value, _timePerHalfCycle, _isLocalSpace);
        }
    }
}
