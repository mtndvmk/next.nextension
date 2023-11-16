using Nextension.Tween;
using Unity.Mathematics;
using UnityEngine;

namespace Nextension
{
    public class AutoOscillatePosition : MonoBehaviour
    {
        [SerializeField] private float3 _fromPosition;
        [SerializeField] private float3 _toPosition;
        [SerializeField] private bool _isLocalSpace = true;
        [SerializeField] private float _timePerHalfCycle = 0.5f;
        [SerializeField] private bool _isStartOnEnable = true;

        private NTweener _tweener;

        private void OnEnable()
        {
            if (_isStartOnEnable)
            {
                start();
            }
        }

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
