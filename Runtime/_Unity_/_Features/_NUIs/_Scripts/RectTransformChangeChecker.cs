using UnityEngine;

namespace Nextension
{
    internal struct RectTransformChangeChecker
    {
        private RectTransform _target;
        private bool _targetActived;
        private Vector2 _size;
        private Vector2 _pos;

        public bool isChanged(RectTransform target)
        {
            if (_target != target)
                return true;
            if (target == null && _target == null)
            {
                return false;
            }
            if (target == null)
            {
                return true;
            }
            if (_targetActived != target.gameObject.activeSelf)
            {
                return true;
            }
            if ((Vector2)_target.localPosition != _pos)
            {
                return true;
            }
            if (_target.rect.size != _size)
            {
                return true;
            }
            return false;
        }
        public void applyData(RectTransform target)
        {
            _target = target;
            if (target)
            {
                _pos = (Vector2)target.localPosition;
                _size = target.rect.size;
                _targetActived = _target.gameObject.activeSelf;
            }
        }
    }
}
