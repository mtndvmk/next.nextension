using UnityEngine;

namespace Nextension
{
    [ExecuteAlways, RequireComponent(typeof(RectTransform))]
    public class RectTransformConstraint : MonoBehaviour
    {
        [SerializeField] private RectTransform _source;
        [SerializeField] private bool _freezeWidth;
        [SerializeField] private bool _freezeHeight;
        [SerializeField] private Vector2 _sizeOffset;

        public RectTransform source
        {
            get => _source;
            set
            {
                _source = value;
            }
        }
        public bool freezeWidth
        {
            get => _freezeWidth;
            set
            {
                _freezeWidth = value;
                updateDimensions();
            }
        }
        public bool freezeHeight
        {
            get => _freezeHeight;
            set
            {
                _freezeHeight = value;
                updateDimensions();
            }
        }
        public Vector2 sizeOffset
        {
            get => _sizeOffset;
            set
            {
                _sizeOffset = value;
                updateDimensions();
            }
        }

#if UNITY_EDITOR
        private bool _editor_freezeWidth;
        private bool _editor_freezeHeight;
        private Vector2 _editor_sizeOffset;
        private void OnValidate()
        {
            if (_editor_freezeWidth != _freezeWidth || _editor_freezeHeight != _freezeHeight || _editor_sizeOffset != _sizeOffset)
            {
                _editor_freezeWidth = _freezeWidth;
                _editor_freezeHeight = _freezeHeight;
                _editor_sizeOffset = _sizeOffset;
                updateDimensions();
            }
        }
#endif
        private void LateUpdate()
        {
            updateDimensions();
        }
        private void updateDimensions()
        {
            var rectTf = transform.asRectTransform();
            var srcRectSize = _source.rect.size;
            if (_source.IsChildOf(rectTf))
            {
                if (_source.anchorMin != _source.anchorMax)
                {
                    _source.anchorMin = _source.anchorMax = (_source.anchorMin + _source.anchorMax) * 0.5f;
                    _source.setSizeWithCurrentAnchors(srcRectSize);
                }
            }
            if (!_freezeWidth)
            {
                var newSize = srcRectSize.x + _sizeOffset.x;
                if (newSize != rectTf.rect.width)
                {
                    rectTf.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newSize);
                }
            }
            if (!_freezeHeight)
            {
                var newSize = srcRectSize.y + _sizeOffset.y;
                if (newSize != rectTf.rect.height)
                {
                    rectTf.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newSize);
                }
            }
        }
    }
}
