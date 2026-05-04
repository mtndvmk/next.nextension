using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Nextension.UI
{
    [RequireComponent(typeof(ScrollRect))]
    public class NScrollRectIndicator : MonoBehaviour
    {
        [System.Serializable]
        public struct DirectionValue<T>
        {
            public T left;
            public T right;
            public T bottom;
            public T top;
        }
        [SerializeField] private ScrollRect _scrollRect;

        [SerializeField] private DirectionValue<GameObject> _indicators;
        [SerializeField] private DirectionValue<float> _padding = new DirectionValue<float>() { left = 10f, right = 10f, bottom = 10f, top = 10f };
        [SerializeField] private float _distanceToUpdate = 10f;

        [Header("Utility")]
        [SerializeField] private bool _resetHPositionOnEnable = false;
        [SerializeField] private bool _resetVPositionOnEnable = false;
        [NShowIf(nameof(_resetHPositionOnEnable)), SerializeField] private float _normalizedHPositionOnEnable = 0f;
        [NShowIf(nameof(_resetVPositionOnEnable)), SerializeField] private float _normalizedVPositionOnEnable = 0f;

        private Vector2 _lastNormalizedPosition;

        public void setPadding(float left, float right, float bottom, float top)
        {
            _padding.left = left;
            _padding.right = right;
            _padding.bottom = bottom;
            _padding.top = top;

            if (_scrollRect != null)
            {
                forceUpdate();
            }
        }

        private UnityAction<Vector2> _onScrollValueChanged;

        private void Reset()
        {
            _scrollRect = GetComponent<ScrollRect>();
        }

        private void OnValidate()
        {
            if (!NStartRunner.IsPlaying) return;
            if (_scrollRect != null)
            {
                forceUpdate();
            }
        }

        private void Awake()
        {
            _onScrollValueChanged = __onScrollValueChanged;
            if (_scrollRect == null)
            {
                _scrollRect = GetComponent<ScrollRect>();
            }
        }

        private void OnEnable()
        {
            if (_scrollRect != null)
            {
                _scrollRect.onValueChanged.AddListener(_onScrollValueChanged);
                _lastNormalizedPosition = _scrollRect.normalizedPosition;
                if (_resetHPositionOnEnable)
                {
                    _scrollRect.horizontalNormalizedPosition = _normalizedHPositionOnEnable;
                }
                if (_resetVPositionOnEnable)
                {
                    _scrollRect.verticalNormalizedPosition = _normalizedVPositionOnEnable;
                }
                forceUpdate();
            }
        }

        void OnRectTransformDimensionsChange()
        {
            forceUpdate();
        }

        private void OnDisable()
        {
            if (_scrollRect != null)
            {
                _scrollRect.onValueChanged.RemoveListener(_onScrollValueChanged);
            }
        }

        private void __onScrollValueChanged(Vector2 normalizedPosition)
        {
            var delta = normalizedPosition - _lastNormalizedPosition;
            var viewportRect = _scrollRect.viewport.rect;
            delta.x *= viewportRect.width;
            delta.y *= viewportRect.height;
            var sqrDistance = delta.sqrMagnitude;
            if (sqrDistance >= _distanceToUpdate * _distanceToUpdate)
            {
                _lastNormalizedPosition = normalizedPosition;
                forceUpdate();
            }
        }

        public void forceUpdate()
        {
            var viewport = _scrollRect.viewport;
            var viewportInContentRect = NUtils.getRectInRootSpace(viewport, _scrollRect.content);

            var contentRect = _scrollRect.content.rect;

            if (_indicators.left != null)
            {
                var x = viewportInContentRect.xMin - _padding.left;
                _indicators.left.setActive(x > 0);
            }
            if (_indicators.right != null)
            {
                var x = viewportInContentRect.xMax + _padding.right;
                _indicators.right.setActive(x < contentRect.width);
            }
            if (_indicators.top != null)
            {
                var y = viewportInContentRect.yMax + _padding.top;
                _indicators.top.setActive(y < contentRect.height);
            }
            if (_indicators.bottom != null)
            {
                var y = viewportInContentRect.yMin - _padding.bottom;
                _indicators.bottom.setActive(y > 0);
            }
        }
    }
}
