using Nextension.Tween;
using UnityEngine;

namespace Nextension
{
    [DisallowMultipleComponent]
    public class NButtonZoomEffect : AbsNButtonEffect
    {
        [SerializeField] private Transform _target;
        [SerializeField] private float _zoomRatio = 1.1f;
        [SerializeField] private float _zoomTime = 0.1f;
        [SerializeField] private NTweener.UpdateMode _updateMode;
        [SerializeField] private bool _isResetScaleOnInteractable = true;

        private NRunnableTweener _effectTweener;
        private Vector3 _originScale;
        private bool _isDown;

        public float ZoomRatio { get => _zoomRatio; set => _zoomRatio = value; }
        public float ZoomTime { get => _zoomTime; set => _zoomTime = value; }

        public override void onButtonDown()
        {
            if (!enabled) return;
            if (_effectTweener == null || _effectTweener.isFinalized)
            {
                if (!_isDown)
                {
                    _isDown = true;
                    _originScale = transform.localScale;
                }
            }
            else
            {
                _effectTweener.cancel();
            }
            _effectTweener = NTween.scaleTo(_target, _originScale * _zoomRatio, _zoomTime);
        }

        public override void onButtonUp()
        {
            if (!enabled) return;
            if (_effectTweener != null && !_effectTweener.isFinalized)
            {
                _effectTweener.cancel();
            }
            _effectTweener = NTween.scaleTo(_target, _originScale, _zoomTime).setUpdateMode(_updateMode);
            _isDown = false;
        }

        public override void onInteractableChanged(bool isInteractable)
        {
            if (isInteractable && _isResetScaleOnInteractable)
            {
                if (_isDown)
                {
                    _effectTweener.cancel();
                    _isDown = false;
                    transform.localScale = _originScale;
                }
            }
        }

        private void Reset()
        {
            _target = transform;
        }

        private void OnDisable()
        {
            if (_effectTweener != null)
            {
                _effectTweener.cancel();
                _effectTweener = null;
                _target.localScale = _originScale;
            }
        }
    }
}