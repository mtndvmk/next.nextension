using Unity.Mathematics;
using UnityEngine;

namespace Nextension.Tween
{
    [DisallowMultipleComponent]
    public sealed class AutoAlternateRotation : AbsAutoAlternate<Vector3>
    {
        [SerializeField] private bool _isLocalSpace = true;

        protected override void setValue(Vector3 value)
        {
            if (_isLocalSpace)
            {
                transform.localEulerAngles = value;
            }
            else
            {
                transform.eulerAngles = value;
            }
            onValueChanged?.Invoke(value);
        }
        protected override NRunnableTweener onFromTo()
        {
            return NTween.rotateTo(transform, quaternion.EulerXYZ(Mathf.Deg2Rad * _toValue).value, FromToDuration, _isLocalSpace);
        }
        protected override NRunnableTweener onToFrom()
        {
            return NTween.rotateTo(transform, quaternion.EulerXYZ(Mathf.Deg2Rad * _fromValue).value, FromToDuration, _isLocalSpace);
        }
        protected override Vector3 getValueFromNormalizedTime(float normalizedTime)
        {
            var from = quaternion.EulerXYZ(Mathf.Deg2Rad * _fromValue).value;
            var to = quaternion.EulerXYZ(Mathf.Deg2Rad * _toValue).value;
            return EaseUtils.ease(from, to, normalizedTime, _easeType).toQuaternion().eulerAngles;
        }
    }
}
