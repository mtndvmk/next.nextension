using Nextension.Tween;
using UnityEngine;

namespace Nextension.UI
{
    [DisallowMultipleComponent]
    public class NButtonZoomEffect : MonoBehaviour, INButtonListener
    {
        [SerializeField] private Transform _target;
        [SerializeField] private float _zoomRatio = 1.1f;
        [SerializeField] private float _zoomTime = 0.1f;

        private NRunnableTweener _effectTweener;
        private Vector3 _originScale;

        public float ZoomRatio { get => _zoomRatio; set => _zoomRatio = value; }
        public float ZoomTime { get => _zoomTime; set => _zoomTime = value; }

        void INButtonListener.onButtonDown()
        {
            if (!enabled) return;
            if (_effectTweener == null || _effectTweener.isFinalized)
            {
                _originScale = transform.localScale;
            }
            else
            {
                _effectTweener.cancel();
            }
            _effectTweener = NTween.scaleTo(_target, _originScale * _zoomRatio, _zoomTime);
        }

        void INButtonListener.onButtonUp()
        {
            if (!enabled) return;
            if (_effectTweener != null && !_effectTweener.isFinalized)
            {
                _effectTweener.cancel();
            }
            _effectTweener = NTween.scaleTo(_target, _originScale, _zoomTime);
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

        private void OnDestroy()
        {
            var button = GetComponent<NButton>();
            if (button)
            {
                button.removeNButtonListener(this);
            }
        }
    }
}