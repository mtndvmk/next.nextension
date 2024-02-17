using Nextension.Tween;
using Unity.Mathematics;
using UnityEngine;

namespace Nextension
{
    [DisallowMultipleComponent]
    public class AutoAlternateRotation : MonoBehaviour
    {
        [SerializeField] private float3 _fromEulerAngles;
        [SerializeField] private float3 _toEulerAngles;
        [SerializeField] private float _timePerHalfCycle = 0.5f;
        [SerializeField] private bool _isLocalSpace = true;
        [SerializeField] private bool _isStartOnEnable = true;
        [SerializeField] private bool _isResetFromEulerAnglesOnEnable = true;

        private NTweener _tweener;

        private void OnEnable()
        {
            if (_isResetFromEulerAnglesOnEnable)
            {
                if (_isLocalSpace)
                {
                    transform.localEulerAngles = _fromEulerAngles;
                }
                else
                {
                    transform.eulerAngles = _fromEulerAngles;
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
        [ContextMenu("Capture FromEulerAngles")]
        private void captureFromEulerAngles()
        {
            _fromEulerAngles = _isLocalSpace ? transform.localEulerAngles : transform.eulerAngles;
            NAssetUtils.setDirty(this);
        }
        [ContextMenu("Capture ToEulerAngles")]
        private void captureToEulerAngles()
        {
            _toEulerAngles = _isLocalSpace ? transform.localEulerAngles : transform.eulerAngles;
            NAssetUtils.setDirty(this);
        }
#endif

        public void start()
        {
            _tweener?.cancel();
            rotateToTo();
        }
        public void stop()
        {
            if (_tweener != null)
            {
                _tweener.cancel();
                _tweener = null;
            }
        }

        private void rotateToTo()
        {
            _tweener = NTween.rotateTo(transform, quaternion.Euler(_toEulerAngles).value, _timePerHalfCycle, _isLocalSpace).onCompleted(rotateToFrom);
        }
        private void rotateToFrom()
        {
            _tweener = NTween.rotateTo(transform, quaternion.Euler(_fromEulerAngles).value, _timePerHalfCycle, _isLocalSpace).onCompleted(rotateToTo);
        }
    }
}
