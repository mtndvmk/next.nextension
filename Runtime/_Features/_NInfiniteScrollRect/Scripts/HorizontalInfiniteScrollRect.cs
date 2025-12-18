using UnityEngine;

namespace Nextension
{
    public class HorizontalInfiniteScrollRect : OneDInfiniteScrollRect
    {
        public enum Direction
        {
            LEFT_RIGHT,
            RIGHT_LEFT,
        }
        
        [field: SerializeField] public Direction direction { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            updateContentAnchorAndPivot();
        }

        protected override void onAddedNewItem(InfiniteCellData data)
        {
            RectTransform scrollContent = scrollRect.content;
            var contentWidth = scrollContent.sizeDelta.x;
            var cellWidth = data.CellSize.x;
            FTAnchor ftAnchor;
            if (data.Index == 0)
            {
                ftAnchor = new FTAnchor(0, cellWidth);
                if (_cellFTAnchorList.Count == 0)
                {
                    contentWidth = cellWidth;
                }
                else
                {
                    contentWidth += cellWidth + _spacing;
                }
            }
            else
            {
                var prevFTAnchor = _cellFTAnchorList[data.Index - 1];
                ftAnchor = new FTAnchor(prevFTAnchor.to - _spacing, cellWidth);
                contentWidth += cellWidth + _spacing;
            }
            _cellFTAnchorList.Add(ftAnchor);
            scrollContent.sizeDelta = new Vector2(contentWidth, scrollContent.sizeDelta.y);
        }
        protected override void onLayoutUpdated()
        {
            if (DataList.Count == 0)
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

            float anchorX = direction == Direction.RIGHT_LEFT ? 1 : 0;
            for (int i = fromVisibleIndex; i <= toVisibleIndex; i++)
            {
                var cell = showCell(i);
                var cellTopFTAnchor = _cellFTAnchorList[i];
                var posX = direction == Direction.RIGHT_LEFT ? cellTopFTAnchor.from : -cellTopFTAnchor.to;
                var cellRectTransform = cell.rectTransform();
                cellRectTransform.anchorMin = cellRectTransform.anchorMin.setX(anchorX);
                cellRectTransform.anchorMax = cellRectTransform.anchorMax.setX(anchorX);
                cellRectTransform.pivot = cellRectTransform.pivot.setX(1);
                cellRectTransform.anchoredPosition = new Vector2(posX, 0);
                cell.transform.SetAsLastSibling();
            }
            _visibleIndices = newVisibleIndices;
        }

        private FTAnchor getViewportFTAnchor()
        {
            float viewportWidth = scrollRect.viewport.rect.width;
            if (direction == Direction.RIGHT_LEFT)
            {
                float contentRight = scrollRect.content.anchoredPosition.x;
                return new FTAnchor(_extendVisibleRange - contentRight, viewportWidth + _extendVisibleRange + _extendVisibleRange);
            }
            else
            {
                float contentLeft = scrollRect.content.anchoredPosition.x;
                return new FTAnchor(_extendVisibleRange - contentLeft + viewportWidth, viewportWidth + _extendVisibleRange + _extendVisibleRange);
            }
        }
        private FTIndex getVisibleIndices()
        {
            int latestIndex = DataList.Count - 1;

            if (scrollRect.viewport.rect.width == 0)
            {
                transform.asRectTransform().markLayoutForRebuild(true);
            }

            if (scrollRect.content.rect.width < scrollRect.viewport.rect.width)
            {
                return new FTIndex(0, latestIndex);
            }

            FTAnchor viewportFTAnchor = getViewportFTAnchor();

            if (direction == Direction.RIGHT_LEFT)
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
            if (index >= DataList.Count)
            {
                return;
            }
            if (scrollRect.content.rect.height < scrollRect.viewport.rect.height)
            {
                return;
            }
            float cellPosX = -_cellFTAnchorList[index].from;
            cellPosX = Mathf.Min(scrollRect.content.rect.width - scrollRect.viewport.rect.width, cellPosX);
            if (direction == Direction.LEFT_RIGHT)
            {
                cellPosX = -cellPosX;
            }
            if (scrollRect.content.anchoredPosition.x != cellPosX)
            {
                innerSnap(new Vector2(cellPosX, 0), duration);
            }
        }
        protected override void onRemovedItem(int index, InfiniteCellData removedCell)
        {
            _cellFTAnchorList.removeLast();

            RectTransform scrollContent = scrollRect.content;
            var contentWidth = scrollContent.sizeDelta.x;

            var deltaWidth = removedCell.CellSize.x + _spacing;
            contentWidth -= deltaWidth;

            scrollContent.sizeDelta = new Vector2(contentWidth, scrollContent.sizeDelta.y);
            scrollRect.content.anchoredPosition -= new Vector2(deltaWidth, 0);
        }

        protected override void onCellSizeUpdated(int index, Vector2 oldSize, Vector2 newSize)
        {
            var scrollRectSizeDelta = scrollRect.content.sizeDelta;
            var contentWidth = scrollRectSizeDelta.x;
            contentWidth -= oldSize.x;
            contentWidth += newSize.x;
            scrollRect.content.sizeDelta = new Vector2(contentWidth, scrollRectSizeDelta.y);
            _cellFTAnchorList[index] = new FTAnchor(_cellFTAnchorList[index].from, newSize.x);
            setDirtyPosition(index);
        }
        protected override void updateContentAnchorAndPivot()
        {
            RectTransform scrollContent = scrollRect.content;
            if (scrollContent == null) return;
            if (direction == Direction.RIGHT_LEFT)
            {
                scrollContent.anchorMin = scrollContent.anchorMin.setX(1);
                scrollContent.anchorMax = scrollContent.anchorMax.setX(1);
                scrollContent.pivot = scrollContent.pivot.setX(1);
            }
            else
            {
                scrollContent.anchorMin = scrollContent.anchorMin.setX(0);
                scrollContent.anchorMax = scrollContent.anchorMax.setX(0);
                scrollContent.pivot = scrollContent.pivot.setX(0);
            }
        }
    }
}
