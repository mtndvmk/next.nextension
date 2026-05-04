using System;

namespace Nextension.Tween
{
    public class AutoAlternateFloat : AbsAutoAlternate<float>
    {
        private float _value;
        private Action<float> _setValueAction;

        protected override void onStart()
        {
            base.onStart();
            _setValueAction ??= setValue;
        }

        protected override NRunnableTweener onFromTo()
        {
            return NTween.fromTo(_fromValue, _toValue, FromToDuration, _setValueAction);
        }

        protected override NRunnableTweener onToFrom()
        {
            return NTween.fromTo(_toValue, _fromValue, FromToDuration, _setValueAction);
        }

        protected override void setValue(float value)
        {
            _value = value;
            onValueChanged?.Invoke(_value);
        }

        protected override float getValueFromNormalizedTime(float normalizedTime)
        {
            return EaseUtils.ease(_fromValue, _toValue, normalizedTime, _easeType);
        }
    }
}