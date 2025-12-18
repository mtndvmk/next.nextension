using UnityEngine;

namespace Nextension.Tween
{
    [DisallowMultipleComponent]
    public sealed class AutoAlternatePosition : AbsAutoAlternate<Vector3>
    {
        [SerializeField] private bool _isLocalSpace = true;

        public override bool EnableOffset => true;

        protected override void OnValidate()
        {
            base.OnValidate();
            if (!IsRunning && _useOffset) _offset = _isLocalSpace ? transform.localPosition : transform.position;
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
            return baseValue + (_useOffset ? _offset : Vector3.zero);
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
            var fromPos = _computeWithOffset(_toValue);
            var toPos = _computeWithOffset(_toValue);
            return EaseUtils.ease(fromPos, toPos, normalizedTime, _easeType);
        }
    }
}
