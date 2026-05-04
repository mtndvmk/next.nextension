using UnityEngine;

namespace Nextension.Tween
{
    [DisallowMultipleComponent]
    public sealed class AutoAlternatePosition : AbsAutoAlternate<Vector3>
    {
        [SerializeField] private bool _isLocalSpace = true;

        public override bool EnableOffset => true;

        protected override void setValueWithoutOffset(Vector3 value)
        {
            setValue(_computeWithOffset(value));
        }

        protected override void setValue(Vector3 value)
        {
            if (_isLocalSpace)
            {
                transform.localPosition = value;
            }
            else
            {
                transform.position = value;
            }
            onValueChanged?.Invoke(value);
        }

        private Vector3 _computeWithOffset(Vector3 baseValue)
        {
            if (_useOffset)
            {
                return baseValue + _offset;
            }
            return baseValue;
        }

        protected override NRunnableTweener onFromTo()
        {
            var targetPosition = _computeWithOffset(_toValue);
            return NTween.moveTo(transform, targetPosition, FromToDuration, _isLocalSpace);
        }
        protected override NRunnableTweener onToFrom()
        {
            var targetPosition = _computeWithOffset(_fromValue);
            return NTween.moveTo(transform, targetPosition, FromToDuration, _isLocalSpace);
        }

        protected override Vector3 getValueFromNormalizedTime(float normalizedTime)
        {
            var fromPos = _computeWithOffset(_fromValue);
            var toPos = _computeWithOffset(_toValue);
            return EaseUtils.ease(fromPos, toPos, normalizedTime, _easeType);
        }

        protected override void onCaptureFromValue()
        {
            base.onCaptureFromValue();
            if (_isLocalSpace)
            {
                _fromValue = transform.localPosition;
            }
            else
            {
                _fromValue = transform.position;
            }
        }
        protected override void onCaptureToValue()
        {
            base.onCaptureToValue();
            if (_isLocalSpace)
            {
                _toValue = transform.localPosition;
            }
            else
            {
                _toValue = transform.position;
            }
        }
        protected override void onCaptureOffsetValue()
        {
            base.onCaptureOffsetValue();
            if (_isLocalSpace)
            {
                _offset = transform.localPosition;
            }
            else
            {
                _offset = transform.position;
            }
        }
    }
}
