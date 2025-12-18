using Unity.Mathematics;
using UnityEngine;

namespace Nextension.Tween
{
    [DisallowMultipleComponent]
    public class AutoAlternateScale : AbsAutoAlternate<Vector3>
    {
        private void Reset()
        {
            _fromValue = _toValue = transform.localScale;
        }
        protected override void setValue(Vector3 value)
        {
            transform.localScale = value;
            onValueChanged?.Invoke(value);
        }
        protected override NRunnableTweener onFromTo()
        {
            return NTween.scaleTo(transform, _toValue, FromToDuration);
        }
        protected override NRunnableTweener onToFrom()
        {
            return NTween.scaleTo(transform, _fromValue, FromToDuration);
        }

        protected override Vector3 getValueFromNormalizedTime(float normalizedTime)
        {
            return EaseUtils.ease((float3)_fromValue, (float3)_toValue, normalizedTime, _easeType);
        }
    }
}
