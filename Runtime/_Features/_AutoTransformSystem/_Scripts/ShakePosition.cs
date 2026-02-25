using Nextension.Tween;
using UnityEngine;

namespace Nextension
{
    public class ShakePosition : MonoBehaviour
    {
        [SerializeField] private float _magnitude = 0.1f;
        [SerializeField] private bool _isLocalSpace = true;
        [SerializeField] private bool _isStartOnEnable = true;
        [SerializeField, Tooltip("LifeTime <= 0 is infinite")] private float _lifeTime = 0;

        private NTweener _tweener;
        private Vector3 _originPosition;

        public bool IsStarted => _tweener != null;

        private void OnValidate()
        {
            if (IsStarted)
            {
                updateMagnitude(_magnitude);
            }
        }
        private void OnEnable()
        {
            if (_isStartOnEnable)
            {
                start();
            }
        }
        private void OnDisable()
        {
            stop();
        }

        private void onStop()
        {
            _tweener = null;
            if (_isLocalSpace)
            {
                transform.localPosition = _originPosition;
            }
            else
            {
                transform.position = _originPosition;
            }
        }

        private void restart(float magnitude, float lifeTime)
        {
            _magnitude = magnitude;
            _lifeTime = lifeTime;
            if (IsStarted)
            {
                stop();
            }
            start();
        }

        public void start()
        {
            if (_tweener != null)
            {
                return;
            }
            _originPosition = _isLocalSpace ? transform.localPosition : transform.position;
            if (_lifeTime > 0)
            {
                _tweener = NTween.shakePosition(transform, _magnitude, _lifeTime, _isLocalSpace).onCompleted(onStop);
            }
            else
            {
                _tweener = NTween.shakePosition(transform, _magnitude, 9999, _isLocalSpace).setLoop(-1);
            }
        }
        public void start(float magnitude, float lifeTime)
        {
            restart(magnitude, lifeTime);
        }
        public void startMagnitude(float magnitude)
        {
            restart(magnitude, _lifeTime);
        }
        public void startLifeTime(float lifeTime)
        {
            restart(_magnitude, lifeTime);
        }
        public void updateMagnitude(float magnitude)
        {
            _magnitude = magnitude;
            if (_tweener != null)
            {
                stop();
                start();
            }
        }
        public void stop()
        {
            if (_tweener != null)
            {
                _tweener.cancel();
                onStop();
            }
        }
    }
}
