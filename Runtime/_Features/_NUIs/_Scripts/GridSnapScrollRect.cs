using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Nextension.UI
{
    public class GridSnapScrollRect : ScrollRect
    {
        [SerializeField] private GridLayoutGroup _gridLayoutGroup;
        [SerializeField] private float _timeIsSwipe = 0.2f;
        [SerializeField] private float _distanceIsSwipe = 10;
        [SerializeField] private float _animationTime = 0.25f;
        [SerializeField] protected Vector2Int _currentCellIndex;

        public Vector2Int CurrentCellIndex => _currentCellIndex;

        protected Vector2 _targetNormalizePosition;
        protected Vector2 _lastNormalizePosition;

        protected float _endAnimationTime;

        public Action<Vector2Int> onCellIndexChangedEvent;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            if (!Application.isPlaying)
            {
                var calNormalized = calculateTargetNormalizePosition(_currentCellIndex);
                if (calNormalized == Vector2.left || calNormalized == _currentCellIndex) return;
                forceView(_currentCellIndex, true);
            }
        }
#endif

        protected override void Start()
        {
            base.Start();
            _lastNormalizePosition = normalizedPosition;
            if (!_gridLayoutGroup)
            {
                _gridLayoutGroup = GetComponentInChildren<GridLayoutGroup>(true);
            }
        }

        protected override void LateUpdate()
        {
            if (_endAnimationTime >= Time.realtimeSinceStartup)
            {
                var dt = _animationTime - (_endAnimationTime - Time.realtimeSinceStartup);

                normalizedPosition = Vector2.Lerp(normalizedPosition, _targetNormalizePosition, dt / _animationTime);
            }
            if (content.anchorMin != Vector2.zero)
            {
                content.anchorMin = Vector2.zero;
            }
            if (content.anchorMax != Vector2.one)
            {
                content.anchorMax = Vector2.one;
            }
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (_endAnimationTime >= Time.realtimeSinceStartup)
            {
                return;
            }
            base.OnEndDrag(eventData);
            if (_lastNormalizePosition != normalizedPosition)
            {
                _lastNormalizePosition = normalizedPosition;
                snap(eventData);
            }
        }

        protected Vector2 getCellCount()
        {
            var contentSize = content.rect.size + _gridLayoutGroup.spacing;
            var cellSize = _gridLayoutGroup.cellSize + _gridLayoutGroup.spacing;
            if (cellSize.hasZeroAxis())
            {
                return Vector2.zero;
            }
            var xCount = (int)(contentSize.x / cellSize.x);
            var yCount = (int)(contentSize.y / cellSize.y);
            return new Vector2(xCount, yCount);
        }
        protected Vector2 getNormalizePosition(Vector2 cellIndex)
        {
            var cellCount = getCellCount();
            Vector2 normalizePosition;
            if (cellCount.x - 1 <= 0)
            {
                normalizePosition.x = 0;
            }
            else
            {
                normalizePosition.x = cellIndex.x / (cellCount.x - 1);
            }
            if (cellCount.y - 1 <= 0)
            {
                normalizePosition.y = 0;
            }
            else
            {
                normalizePosition.y = cellIndex.y / (cellCount.y - 1);
            }
            normalizePosition.x = Mathf.Clamp01(normalizePosition.x);
            normalizePosition.y = Mathf.Clamp01(normalizePosition.y);
            return normalizePosition;
        }
        protected void snap(PointerEventData eventData)
        {
            if (!content || !_gridLayoutGroup)
            {
                return;
            }

            var cellCount = getCellCount();

            if (cellCount.hasZeroAxis())
            {
                return;
            }

            Vector2 targetCellIndex = _currentCellIndex;

            var dragTime = Time.realtimeSinceStartup - eventData.clickTime;
            var dragDelta = eventData.position - eventData.pressPosition;

            if (dragTime < _timeIsSwipe || Mathf.Abs(dragDelta.x) > Screen.width / 2 || Mathf.Abs(dragDelta.y) > Screen.height / 2)
            {
                // Calculate next index
                if (dragDelta.x > _distanceIsSwipe)
                {
                    targetCellIndex.x -= 1;
                }
                else if (dragDelta.x < -_distanceIsSwipe)
                {
                    targetCellIndex.x += 1;
                }

                if (dragDelta.y > _distanceIsSwipe)
                {
                    targetCellIndex.y -= 1;
                }
                else if (dragDelta.y < -_distanceIsSwipe)
                {
                    targetCellIndex.y += 1;
                }
            }
            else
            {
                targetCellIndex.x = Mathf.RoundToInt(normalizedPosition.x * (cellCount.x - 1));
                targetCellIndex.y = Mathf.RoundToInt(normalizedPosition.y * (cellCount.y - 1));
            }

            targetCellIndex.x = Mathf.Min(cellCount.x - 1, targetCellIndex.x);
            targetCellIndex.x = Mathf.Max(0, targetCellIndex.x);
            targetCellIndex.y = Mathf.Max(0, targetCellIndex.y);

            forceView(targetCellIndex);
        }

        private Vector2 calculateTargetNormalizePosition(Vector2 cellIndex)
        {
            var cellCount = getCellCount();
            if (cellCount.hasZeroAxis())
            {
                return Vector2.left;
            }

            if (cellIndex.x < 0)
            {
                cellIndex.x = 0;
            }
            else if (cellIndex.x > cellCount.x)
            {
                cellIndex.x = cellCount.x - 1;
            }

            if (cellIndex.y < 0)
            {
                cellIndex.y = 0;
            }
            else if (cellIndex.y > cellCount.y)
            {
                cellIndex.y = cellCount.y - 1;
            }
            return getNormalizePosition(cellIndex);
        }

        public void forceView(Vector2 cellIndex, bool isImmediate = false)
        {
            _targetNormalizePosition = calculateTargetNormalizePosition(cellIndex);

            if (_targetNormalizePosition != Vector2.left)
            {
                if (_animationTime > 0 && !isImmediate)
                {
                    _endAnimationTime = Time.realtimeSinceStartup + _animationTime;
                }
                else
                {
                    normalizedPosition = _targetNormalizePosition;
                }

                _currentCellIndex = new Vector2Int((int)cellIndex.x, (int)cellIndex.y);
                onCellIndexChangedEvent?.Invoke(_currentCellIndex);
            }
        }
    }
}
