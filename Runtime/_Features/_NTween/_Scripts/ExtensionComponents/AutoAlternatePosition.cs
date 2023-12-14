using Nextension.Tween;
using Unity.Mathematics;
using UnityEngine;

namespace Nextension
{
    public class AutoAlternatePosition : MonoBehaviour
    {
        [SerializeField] private float3 _fromPosition;
        [SerializeField] private float3 _toPosition;
        [SerializeField] private float _timePerHalfCycle = 0.5f;
        [SerializeField] private bool _isLocalSpace = true;
        [SerializeField] private bool _isStartOnEnable = true;
        [SerializeField] private bool _isResetToFromPositionOnEnable = true;

        private NTweener _tweener;

        private void OnEnable()
        {
            if (_isResetToFromPositionOnEnable)
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
            stop();
        }

#if UNITY_EDITOR
        [ContextMenu("Caputure FromPosition")]
        private void caputureFromPosition()
        {
            _fromPosition = _isLocalSpace ? transform.localPosition : transform.position;
            NAssetUtils.setDirty(this);
        }
        [ContextMenu("Caputure ToPosition")]
        private void caputureToPosition()
        {
            _toPosition = _isLocalSpace ? transform.localPosition : transform.position; 
            NAssetUtils.setDirty(this);
        }
#endif

        public void start()
        {
            if (_tweener != null)
            {
                _tweener.cancel();
            }
            moveUp();
        }
        public void stop()
        {
            if (_tweener != null)
            {
                _tweener.cancel();
                _tweener = null;
            }
        }

        private void moveUp()
        {
            _tweener = NTween.moveTo(transform, _toPosition, _timePerHalfCycle, _isLocalSpace).onCompleted(moveDown);
        }
        private void moveDown()
        {
            _tweener = NTween.moveTo(transform, _fromPosition, _timePerHalfCycle, _isLocalSpace).onCompleted(moveUp);
        }
    }
}
