using Nextension.Tween;
using Unity.Mathematics;
using UnityEngine;

namespace Nextension
{
    [DisallowMultipleComponent]
    public class AutoAlternatePosition : MonoBehaviour
    {
        [SerializeField] private float3 _fromPosition;
        [SerializeField] private float3 _toPosition;
        [SerializeField] private float _timePerHalfCycle = 0.5f;
        [SerializeField] private bool _isLocalSpace = true;
        [SerializeField] private bool _isStartOnEnable = true;
        [SerializeField] private bool _isResetFromPositionOnEnable = true;

        private NTweener _tweener;

        private void OnEnable()
        {
            if (_isResetFromPositionOnEnable)
            {
                if (_isLocalSpace)
                {
                    transform.localPosition = _fromPosition;
                }
                else
                {
                    transform.position = _fromPosition;
                }
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
        [ContextMenu("Capture FromPosition")]
        private void captureFromPosition()
        {
            _fromPosition = _isLocalSpace ? transform.localPosition : transform.position;
            NAssetUtils.setDirty(this);
        }
        [ContextMenu("Capture ToPosition")]
        private void captureToPosition()
        {
            _toPosition = _isLocalSpace ? transform.localPosition : transform.position; 
            NAssetUtils.setDirty(this);
        }
#endif

        public void start()
        {
            _tweener?.cancel();
            moveToTo();
        }
        public void stop()
        {
            if (_tweener != null)
            {
                _tweener.cancel();
                _tweener = null;
            }
        }

        private void moveToTo()
        {
            _tweener = NTween.moveTo(transform, _toPosition, _timePerHalfCycle, _isLocalSpace).onCompleted(moveToFrom);
        }
        private void moveToFrom()
        {
            _tweener = NTween.moveTo(transform, _fromPosition, _timePerHalfCycle, _isLocalSpace).onCompleted(moveToTo);
        }
    }
}
