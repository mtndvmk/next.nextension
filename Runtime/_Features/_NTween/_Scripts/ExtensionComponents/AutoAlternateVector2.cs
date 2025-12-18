using Unity.Mathematics;
using UnityEngine;

namespace Nextension.Tween
{
    public class AutoAlternateVector2 : AbsAutoAlternate<Vector2>
    {
        private Vector2 _value;

        protected override NRunnableTweener onFromTo()
        {
            return NTween.fromTo((float2)_fromValue, (float2)_toValue, FromToDuration, setValueF2);
        }

        protected override NRunnableTweener onToFrom()
        {
            return NTween.fromTo((float2)_toValue, (float2)_fromValue, FromToDuration, setValueF2);
        }
        protected void setValueF2(float2 value)
        {
            setValue((Vector2)value);
        }
        protected override void setValue(Vector2 value)
        {
            _value = value;
            onValueChanged?.Invoke(_value);
        }

        protected override Vector2 getValueFromNormalizedTime(float normalizedTime)
        {
            return EaseUtils.ease((float2)_fromValue, _toValue, normalizedTime, _easeType);
        }
    }
}