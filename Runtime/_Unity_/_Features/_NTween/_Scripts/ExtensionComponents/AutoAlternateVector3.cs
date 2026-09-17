using Unity.Mathematics;
using UnityEngine;

namespace Nextension.Tween
{
    public class AutoAlternateVector3 : AbsAutoAlternate<Vector3>
    {
        private Vector3 _value;

        private static void __setValueF3_Static(object target, float3 value)
        {
            ((AutoAlternateVector3)target).setValueF3(value);
        }

        protected unsafe override NTweener onFromTo()
        {
            return NTween.fromToUnsafe((float3)_fromValue, (float3)_toValue, FromToDuration, this, &__setValueF3_Static);
        }

        protected unsafe override NTweener onToFrom()
        {
            return NTween.fromToUnsafe((float3)_toValue, (float3)_fromValue, FromToDuration, this, &__setValueF3_Static);
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