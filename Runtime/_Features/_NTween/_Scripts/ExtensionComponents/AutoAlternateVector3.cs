using Unity.Mathematics;
using UnityEngine;

namespace Nextension.Tween
{
    public class AutoAlternateVector3 : AbsAutoAlternate<Vector3>
    {
        private Vector3 _value;

        protected override NRunnableTweener onFromTo()
        {
            return NTween.fromTo((float3)_fromValue, (float3)_toValue, FromToDuration, setValueF3);
        }

        protected override NRunnableTweener onToFrom()
        {
            return NTween.fromTo((float3)_toValue, (float3)_fromValue, FromToDuration, setValueF3);
        }
        protected void setValueF3(float3 value)
        {
            setValue((Vector3)value);
        }
        protected override void setValue(Vector3 value)
        {
            _value = value;
            onValueChanged?.Invoke(_value);
        }

        protected override Vector3 getValueFromNormalizedTime(float normalizedTime)
        {
            return EaseUtils.ease((float3)_fromValue, (float3)_toValue, normalizedTime, _easeType);
        }
    }
}