using Nextension.Tween;
using Unity.Mathematics;
using UnityEngine;

namespace Nextension
{
    [DisallowMultipleComponent]
    public class AutoAlternateScale : MonoBehaviour
    {
        [SerializeField] private float3 _fromScale;
        [SerializeField] private float3 _toScale;
        [SerializeField] private float _timePerHalfCycle = 0.5f;
        [SerializeField] private bool _isStartOnEnable = true;
        [SerializeField] private bool _isResetFromScaleOnEnable = true;

        private NTweener _tweener;

        private void OnEnable()
        {
            if (_isResetFromScaleOnEnable)
            {
                transform.localScale = _fromScale;
            }
            if (_isStartOnEnable)
            {
                start();
            }
        }
        private void OnDisable()
        {
#if UNITY_EDITOR
            if (NStartRunner.IsPlaying)
#endif
                stop();
        }

#if UNITY_EDITOR
        [ContextMenu("Capture FromScale")]
        private void captureFromScale()
        {
            _fromScale = transform.localScale;
            NAssetUtils.setDirty(this);
        }
        [ContextMenu("Capture ToScale")]
        private void captureToScale()
        {
            _toScale = transform.localScale;
            NAssetUtils.setDirty(this);
        }
#endif

        public void start()
        {
            _tweener?.cancel();
            scaleToTo();
        }
        public void stop()
        {
            if (_tweener != null)
            {
                _tweener.cancel();
                _tweener = null;
            }
        }

        private void scaleToTo()
        {
            _tweener = NTween.scaleTo(transform, _toScale, _timePerHalfCycle).onCompleted(scaleToFrom);
        }
        private void scaleToFrom()
        {
            _tweener = NTween.scaleTo(transform, _fromScale, _timePerHalfCycle).onCompleted(scaleToTo);
        }
    }
}
