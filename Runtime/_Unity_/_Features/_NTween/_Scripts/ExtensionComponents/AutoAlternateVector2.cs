using Unity.Mathematics;
using UnityEngine;

namespace Nextension.Tween
{
    public class AutoAlternateVector2 : AbsAutoAlternate<Vector2>
    {
        private Vector2 _value;

        private static void __setValueF2_Static(object target, float2 value)
        {
            ((AutoAlternateVector2)target).setValueF2(value);
        }

        protected unsafe override NTweener onFromTo()
        {
            return NTween.fromToUnsafe((float2)_fromValue, (float2)_toValue, FromToDuration, this, &__setValueF2_Static);
        }

        protected unsafe override NTweener onToFrom()
        {
            return NTween.fromToUnsafe((float2)_toValue, (float2)_fromValue, FromToDuration, this, &__setValueF2_Static);
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