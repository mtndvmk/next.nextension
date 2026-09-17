namespace Nextension.Tween
{
    public class AutoAlternateFloat : AbsAutoAlternate<float>
    {
        private float _value;

        private static void __setValue_Static(object target, float value)
        {
            ((AutoAlternateFloat)target).setValue(value);
        }

        protected unsafe override NTweener onFromTo()
        {
            return NTween.fromToUnsafe(_fromValue, _toValue, FromToDuration, this, &__setValue_Static);
        }

        protected unsafe override NTweener onToFrom()
        {
            return NTween.fromToUnsafe(_toValue, _fromValue, FromToDuration, this, &__setValue_Static);
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