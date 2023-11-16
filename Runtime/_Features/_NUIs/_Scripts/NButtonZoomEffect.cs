using Nextension.Tween;
using UnityEngine;

namespace Nextension.UI
{
    [RequireComponent(typeof(NButton)), DisallowMultipleComponent]
    public class NButtonZoomEffect : MonoBehaviour, INButtonListener
    {
        [SerializeField] private Vector3 _originScale = Vector3.one;
        [SerializeField] private float _zoomRatio = 1.1f;
        [SerializeField] private float _zoomTime = 0.1f;

        private NRunnableTweener _effectTweener;

        private void OnValidate()
        {
            _originScale = transform.localScale;
        }

        public Vector3 OriginScale { get => _originScale; set => _originScale = value; }
        public float ZoomRatio { get => _zoomRatio; set => _zoomRatio = value; }
        public float ZoomTime { get => _zoomTime; set => _zoomTime = value; }

        void INButtonListener.onButtonDown()
        {
            _effectTweener?.cancel();
            _effectTweener = NTween.scaleTo(transform, _originScale * _zoomRatio, _zoomTime);
        }

        void INButtonListener.onButtonUp()
        {
            _effectTweener?.cancel();
            _effectTweener = NTween.scaleTo(transform, _originScale, _zoomTime);
        }

        private void OnDisable()
        {
            if (_effectTweener != null)
            {
                _effectTweener.cancel();
                _effectTweener = null;
            }
        }
    }
}