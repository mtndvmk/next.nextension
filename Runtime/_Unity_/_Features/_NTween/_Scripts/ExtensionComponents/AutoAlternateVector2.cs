using System;
using Unity.Mathematics;
using UnityEngine;

namespace Nextension.Tween
{
    public class AutoAlternateVector2 : AbsAutoAlternate<Vector2>
    {
        private Vector2 _value;
        private Action<float2> _setValueF2Action;

        protected override void onStart()
        {
            base.onStart();
            _setValueF2Action ??= setValueF2;
        }

        protected override NRunnableTweener onFromTo()
        {
            return NTween.fromTo((float2)_fromValue, (float2)_toValue, FromToDuration, _setValueF2Action);
        }

        protected override NRunnableTweener onToFrom()
        {
            return NTween.fromTo((float2)_toValue, (float2)_fromValue, FromToDuration, _setValueF2Action);
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