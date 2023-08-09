using Nextension.Tween;
using UnityEngine;

namespace Nextension.UI
{
    public class NButtonZoomEffect : MonoBehaviour, INButtonListener
    {
        [SerializeField] private Vector3 _originScale;
        [SerializeField] private float _zoomRatio = 1.1f;
        [SerializeField] private float _zoomTime = 0.2f;

        private NTweener _effectTweener;

        private void OnValidate()
        {
            _originScale = transform.localScale;
        }

        void INButtonListener.onButtonDown()
        {
            if (_effectTweener != null)
            {
                _effectTweener.cancel();
            }
            _effectTweener = NTweenManager.scaleTo(transform, _originScale * _zoomRatio, _zoomTime);
        }

        void INButtonListener.onButtonUp()
        {
            if (_effectTweener != null)
            {
                _effectTweener.cancel();
            }
            _effectTweener = NTweenManager.scaleTo(transform, _originScale, _zoomTime);
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