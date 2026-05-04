using UnityEngine;

namespace Nextension
{
    public class VerticalInfiniteScrollRect : OneDInfiniteScrollRect
    {
        public enum Direction
        {
            TOP_BOTTOM,
            BOTTOM_TOP,
        }
        [SerializeField] private Direction _direction;
        public Direction direction
        {
            get => _direction;
            set
            {
                if (value != _direction)
                {
                    _direction = value;
                    updateContentAnchorAndPivot();
                    setDirtyPosition(0);
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();
            updateContentAnchorAndPivot();
        }

        protected override float getCellMainSize(InfiniteCellData data)
        {
            return data.cellSize.y;
        }

        protected override void onInheritedAddedNewItem(ref InfiniteCellData data)
        {
            _segmentTree.SetSize(_dataList.Count);
            _segmentTree.Set(data.index, data.cellSize.y + _spacing);

            RectTransform scrollContent = scrollRect.content;
            var contentHeight = scrollContent.sizeDelta.y;
            var cellHeight = data.cellSize.y;
            if (data.index == 0)
            {
                contentHeight = _headOffset + cellHeight + _tailOffset;
            }
            else
            {
                contentHeight += cellHeight + _spacing;
            }
            scrollContent.sizeDelta = new Vector2(scrollContent.sizeDelta.x, contentHeight);
            if (_isReverse)
            {
                setDirtyPosition(0);
            }
        }
        protected override void onLayoutUpdated()
        {
            if (_dataList.Count == 0)
                return;

            var newVisibleIndices = getVisibleIndices();

            var fromVisibleIndex = newVisibleIndices.fromIndex;
            var toVisibleIndex = newVisibleIndices.toIndex;

            if (fromVisibleIndex == _visibleIndices.fromIndex && toVisibleIndex == _visibleIndices.toIndex)
            {
                return;
            }

            if (_visibleIndices.fromIndex >= 0)
            {
                var fromMinIndex = _visibleIndices.fromIndex < fromVisibleIndex ? _visibleIndices.fromIndex : fromVisibleIndex;
                var toMaxIndex = _visibleIndices.toIndex > toVisibleIndex ? _visibleIndices.toIndex : toVisibleIndex;
                for (int i = fromMinIndex; i < fromVisibleIndex; i++)
                {
                    hideCell(i);
                }
                for (int i = toVisibleIndex + 1; i <= toMaxIndex; i++)
                {
                    hideCell(i);
                }
            }

            float anchorY = _direction == Direction.TOP_BOTTOM ? 1 : 0;
            for (int i = fromVisibleIndex; i <= toVisibleIndex; i++)
            {
                var cell = showCell(i);
                cell.transform.SetAsLastSibling();
                var cellFTAnchor = getCellFTAnchor(i);
                var posY = _direction == Direction.TOP_BOTTOM ? cellFTAnchor.from : -cellFTAnchor.to;
                var cellRectTransform = cell.rectTransform();
                var originPivot = cellRectTransform.pivot;
                cellRectTransform.anchorMin = cellRectTransform.anchorMin.setY(anchorY);
                cellRectTransform.anchorMax = cellRectTransform.anchorMax.setY(anchorY);
                cellRectTransform.pivot = cellRectTransform.pivot.setY(1);
                cellRectTransform.anchoredPosition = new Vector2(0, posY);
                cellRectTransform.setPivotWithoutChangePosition(originPivot);
            }

            _visibleIndices = newVisibleIndices;
        }

        private FTAnchor getViewportFTAnchor()
        {
            float viewportHeight = scrollRect.viewport.rect.height;
            if (_direction == Direction.TOP_BOTTOM)
            {
                float contentTop = scrollRect.content.anchoredPosition.y;
                return new FTAnchor(_extendVisibleRange - contentTop, viewportHeight + _extendVisibleRange + _extendVisibleRange);
            }
            else
            {
                float contentBottom = scrollRect.content.anchoredPosition.y;
                return new FTAnchor(_extendVisibleRange - contentBottom + viewportHeight, viewportHeight + _extendVisibleRange + _extendVisibleRange);
            }
        }
        private FTIndex getVisibleIndices()
        {
            int latestIndex = _dataList.Count - 1;

            if (scrollRect.viewport.rect.height == 0)
            {
                transform.asRectTransform().markLayoutForRebuild(true);
            }

            if (scrollRect.content.rect.height < scrollRect.viewport.rect.height)
            {
                return new FTIndex(0, latestIndex);
            }

            FTAnchor viewportFTAnchor = getViewportFTAnchor();

            if (_direction == Direction.TOP_BOTTOM)
            {
                var fromVisibleIndex = getFromVisibleIndex_1To0(viewportFTAnchor.from, 0, latestIndex);
                var toVisibleIndex = getToVisibleIndex_1To0(viewportFTAnchor.to, fromVisibleIndex, latestIndex);
                return new FTIndex(fromVisibleIndex, toVisibleIndex);
            }
            else
            {
                var fromVisibleIndex = getFromVisibleIndex_0To1(-viewportFTAnchor.from, 0, latestIndex);
                var toVisibleIndex = getToVisibleIndex_0To1(-viewportFTAnchor.to, 0, fromVisibleIndex);
                return new FTIndex(toVisibleIndex, fromVisibleIndex);
            }
        }

        public override void snap(int index, float duration = 0)
        {
            if (index >= _dataList.Count)
            {
                return;
            }
            if (scrollRect.content.rect.height < scrollRect.viewport.rect.height)
            {
                return;
            }
            float cellPosY = -getCellFTAnchor(index).from;
            cellPosY = Mathf.Min(scrollRect.content.rect.height - scrollRect.viewport.rect.height, cellPosY);
            if (_direction == Direction.BOTTOM_TOP)
            {
                cellPosY = -cellPosY;
            }
            if (scrollRect.content.anchoredPosition.y != cellPosY)
            {
                innerSnap(new Vector2(0, cellPosY), duration);
            }
        }

        public override int GetCellIndexAtViewportPosition(float normalizedPosition)
        {
            if (_dataList.Count == 0) return -1;
            int latestIndex = _dataList.Count - 1;

            float viewportHeight = scrollRect.viewport.rect.height;
            if (_direction == Direction.TOP_BOTTOM)
            {
                float contentTop = scrollRect.content.anchoredPosition.y;
                float posY = -contentTop - (1 - normalizedPosition) * viewportHeight;
                return getNearestCellIndex(posY, 0, latestIndex);
            }
            else
            {
                float contentBottom = scrollRect.content.anchoredPosition.y;
                float posY = -contentBottom + normalizedPosition * viewportHeight;
                return getNearestCellIndex(-posY, 0, latestIndex);
            }
        }

        public override void snapCellIndexToNormalizePosition(int index, float normalizedPosition, float duration = 0)
        {
            if (index < 0 || index >= _dataList.Count || scrollRect.content.rect.height < scrollRect.viewport.rect.height)
            {
                return;
            }

            float viewportHeight = scrollRect.viewport.rect.height;
            float from = getCellFTAnchor(index).from;
            float cellSize = _dataList[index].cellSize.y;
            float maxScroll = scrollRect.content.rect.height - viewportHeight;
            float targetY;

            if (_direction == Direction.TOP_BOTTOM)
            {
                targetY = -from - (1 - normalizedPosition) * (viewportHeight - cellSize);
                targetY = Mathf.Clamp(targetY, 0, maxScroll);
            }
            else
            {
                targetY = from + normalizedPosition * (viewportHeight - cellSize);
                targetY = Mathf.Clamp(targetY, -maxScroll, 0);
            }

            if (scrollRect.content.anchoredPosition.y != targetY)
            {
                innerSnap(new Vector2(0, targetY), duration);
            }
        }

        protected override void onRemovedItem(int index, in InfiniteCellData removedCell)
        {
            RectTransform scrollContent = scrollRect.content;
            var contentHeight = scrollContent.sizeDelta.y;
            var deltaHeight = removedCell.cellSize.y + _spacing;

            contentHeight -= deltaHeight;
            if (_dataList.Count == 0) contentHeight = 0;
            scrollContent.sizeDelta = new Vector2(scrollContent.sizeDelta.x, contentHeight);

            scrollRect.content.anchoredPosition -= new Vector2(0, deltaHeight);
        }

        protected override void onCellSizeUpdated(int index, Vector2 oldSize, Vector2 newSize)
        {
            if (oldSize == newSize) return;
            var contentHeight = scrollRect.content.sizeDelta.y;
            contentHeight -= oldSize.y;
            contentHeight += newSize.y;
            scrollRect.content.sizeDelta = new Vector2(scrollRect.content.sizeDelta.x, contentHeight);
            _segmentTree.Set(index, newSize.y + _spacing);
            setDirtyLayout();
        }
        protected override void updateContentAnchorAndPivot()
        {
            RectTransform scrollContent = scrollRect.content;
            if (scrollContent == null) return;
            if (_direction == Direction.TOP_BOTTOM)
            {
                scrollContent.anchorMin = scrollContent.anchorMin.setY(1);
                scrollContent.anchorMax = scrollContent.anchorMax.setY(1);
                scrollContent.pivot = scrollContent.pivot.setY(1);
            }
            else
            {
                scrollContent.anchorMin = scrollContent.anchorMin.setY(0);
                scrollContent.anchorMax = scrollContent.anchorMax.setY(0);
                scrollContent.pivot = scrollContent.pivot.setY(0);
            }
        }
    }
}
