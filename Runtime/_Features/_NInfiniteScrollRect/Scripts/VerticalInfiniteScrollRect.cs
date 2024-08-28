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
        [field: SerializeField] public Direction direction { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            updateContentAnchorAndPivot();
        }
        protected override void OnValidate()
        {
            updateContentAnchorAndPivot();
            recalculateCellPositions();
        }
        protected override void onAddedNewItem(InfiniteCellData data)
        {
            RectTransform scrollContent = scrollRect.content;
            var contentHeight = scrollContent.sizeDelta.y;
            var cellHeight = data.CellSize.y;
            FTAnchor ftAnchor = default;
            if (data.Index == 0)
            {
                ftAnchor = new FTAnchor(0, cellHeight);
                contentHeight = cellHeight;
            }
            else
            {
                var prevFTAnchor = _cellFTAnchorList[data.Index - 1];
                ftAnchor = new FTAnchor(prevFTAnchor.to - spacing, cellHeight);
                contentHeight += cellHeight + spacing;
            }
            _cellFTAnchorList.Add(ftAnchor);
            scrollContent.sizeDelta = new Vector2(scrollContent.sizeDelta.x, contentHeight);
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

            float anchorY = direction == Direction.TOP_BOTTOM ? 1 : 0;
            for (int i = fromVisibleIndex; i <= toVisibleIndex; i++)
            {
                var cellFTAnchor = _cellFTAnchorList[i];
                var posY = direction == Direction.TOP_BOTTOM ? cellFTAnchor.from : -cellFTAnchor.to;
                var cell = showCell(i, new Vector2(0, posY));
                var cellRectTransform = cell.rectTransform();
                cellRectTransform.anchorMin = cellRectTransform.anchorMin.setY(anchorY);
                cellRectTransform.anchorMax = cellRectTransform.anchorMax.setY(anchorY);
                cellRectTransform.pivot = cellRectTransform.pivot.setY(1);
                cell.transform.SetAsLastSibling();
            }
            _visibleIndices = newVisibleIndices;
        }

        private FTAnchor getViewportFTAnchor()
        {
            float viewportHeight = scrollRect.viewport.rect.height;
            if (direction == Direction.TOP_BOTTOM)
            {
                float contentTop = scrollRect.content.anchoredPosition.y;
                return new FTAnchor(extendVisibleRange - contentTop, viewportHeight + extendVisibleRange + extendVisibleRange);
            }
            else
            {
                float contentBottom = scrollRect.content.anchoredPosition.y;
                return new FTAnchor(extendVisibleRange - contentBottom + viewportHeight, viewportHeight + extendVisibleRange + extendVisibleRange);
            }
        }
        private FTIndex getVisibleIndices()
        {
            int latestIndex = DataList.Count - 1;

            if (scrollRect.viewport.rect.height == 0)
            {
                transform.asRectTransform().markLayoutForRebuild(true);
            }

            if (scrollRect.content.rect.height < scrollRect.viewport.rect.height)
            {
                return new FTIndex(0, latestIndex);
            }

            FTAnchor viewportFTAnchor = getViewportFTAnchor();

            if (direction == Direction.TOP_BOTTOM)
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
            float cellPosY = -_cellFTAnchorList[index].from;
            cellPosY = Mathf.Min(scrollRect.content.rect.height - scrollRect.viewport.rect.height, cellPosY);
            if (direction == Direction.BOTTOM_TOP)
            {
                cellPosY = -cellPosY;
            }
            if (scrollRect.content.anchoredPosition.y != cellPosY)
            {
                innerSnap(new Vector2(0, cellPosY), duration);
            }
        }
        public override void remove(int index)
        {
            var removeCell = DataList[index];
            base.remove(index);

            RectTransform scrollContent = scrollRect.content;
            var contentHeight = scrollContent.sizeDelta.y;
            var deltaHeight = removeCell.CellSize.y + spacing;

            contentHeight -= deltaHeight;
            scrollContent.sizeDelta = new Vector2(scrollContent.sizeDelta.x, contentHeight);
            
            scrollRect.content.anchoredPosition -= new Vector2(0, deltaHeight);
        }

        protected override void onCellSizeUpdated(int index, Vector2 oldSize, Vector2 newSize)
        {
            var contentHeight = scrollRect.content.sizeDelta.y;
            contentHeight -= oldSize.y;
            contentHeight += newSize.y;
            scrollRect.content.sizeDelta = new Vector2(scrollRect.content.sizeDelta.x, contentHeight);
            _cellFTAnchorList[index] = new FTAnchor(_cellFTAnchorList[index].from, newSize.y);
            recalculateCellPositions(index);
        }
        protected void updateContentAnchorAndPivot()
        {
            RectTransform scrollContent = scrollRect.content;
            if (direction == Direction.TOP_BOTTOM)
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
