using UnityEngine;

namespace Nextension
{
    public class GridVerticalInfiniteScrollRect : InfiniteScrollRect
    {
        public enum Direction
        {
            TOP_BOTTOM,
            BOTTOM_TOP,
        }

        [SerializeField] private Direction _direction;
        [SerializeField] private Vector2 _spacing = new Vector2(10, 10);
        [SerializeField] private Vector2 _cellSize = new Vector2(100, 100);
        [SerializeField] private float _extendVisibleRange;
        public Direction direction
        {
            get => _direction;
            set
            {
                if (value != _direction)
                {
                    _direction = value;
                    updateContentAnchorAndPivot();
                    recalculateCellPositions();
                }
            }
        }
        public Vector2 spacing
        {
            get => _spacing;
            set
            {
                if (value != _spacing)
                {
                    _spacing = value;
                    recalculateCellPositions();
                }
            }
        }
        public float extendVisibleRange
        {
            get => _extendVisibleRange;
            set
            {
                if (value != _extendVisibleRange)
                {
                    _extendVisibleRange = value;
                    setDirty();
                }
            }
        }
        public Vector2 CellSize
        {
            get => _cellSize;
            set
            {
                if (_cellSize != value)
                {
                    _cellSize = value;
                    foreach (var data in DataList)
                    {
                        data.CellSize = _cellSize;
                    }
                    calculateColumns();
                    updateContentSize();
                    recalculateCellPositions();
                }
            }
        }

        public int Columns { get; private set; }

        protected NList<FTAnchor> _cellFTAnchorList = new NList<FTAnchor>();
        protected FTIndex _visibleIndices = new FTIndex(-1, -1);

        protected override void Awake()
        {
            base.Awake();
            updateContentAnchorAndPivot();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            foreach (var data in DataList)
            {
                data.CellSize = _cellSize;
            }
            calculateColumns();
            updateContentSize();
            recalculateCellPositions(0);
            base.OnValidate();
        }
#endif

        protected override void onAddedNewItem(InfiniteCellData data)
        {
            if (_cellFTAnchorList.Count == 0)
            {
                calculateColumns();
            }

            updateContentSize();
            FTAnchor ftAnchor = calculateCellAnchor(data.Index);
            data.CellSize = _cellSize;
            _cellFTAnchorList.Add(ftAnchor);
        }

        private void calculateColumns()
        {
            var scrollRectViewport = scrollRect.viewport;
            if (scrollRectViewport.rect.width == 0)
            {
                transform.asRectTransform().markLayoutForRebuild(true);
            }
            var cellWidth = _cellSize.x;
            Columns = (int)((scrollRectViewport.rect.width + _spacing.x) / (cellWidth + _spacing.x));
        }

        protected override void onLayoutUpdated()
        {
            if (DataList.Count == 0)
                return;

            var newVisibleIndices = getVisibleIndices();

            var fromVisibleIndex = newVisibleIndices.fromIndex;
            var toVisibleIndex = newVisibleIndices.toIndex;

            if (fromVisibleIndex == _visibleIndices.fromIndex && toVisibleIndex == _visibleIndices.toIndex)
                return;

            updateVisibleCells(newVisibleIndices);
            _visibleIndices = newVisibleIndices;
        }

        private void updateVisibleCells(FTIndex newVisibleIndices)
        {
            float anchorY = _direction == Direction.TOP_BOTTOM ? 1 : 0;
            int fromVisibleIndex = newVisibleIndices.fromIndex;
            int toVisibleIndex = newVisibleIndices.toIndex;

            // Hide cells that are no longer visible
            if (_visibleIndices.fromIndex >= 0)
            {
                int fromMinIndex = Mathf.Min(_visibleIndices.fromIndex, fromVisibleIndex);
                int toMaxIndex = Mathf.Max(_visibleIndices.toIndex, toVisibleIndex);

                for (int i = fromMinIndex; i < fromVisibleIndex; i++) hideCell(i);
                for (int i = toVisibleIndex + 1; i <= toMaxIndex; i++) hideCell(i);
            }

            // Show visible cells

            var cellRectAnchor = new Vector2(0, anchorY);

            for (int i = fromVisibleIndex; i <= toVisibleIndex; i++)
            {
                var ftAnchor = _cellFTAnchorList[i];
                Vector2 position = calculateCellPosition(i, ftAnchor);
                var cell = showCell(i, position);
                var cellRectTransform = cell.rectTransform();

                cellRectTransform.anchorMin = cellRectAnchor;
                cellRectTransform.anchorMax = cellRectAnchor;
                cellRectTransform.pivot = cellRectAnchor;
                cell.transform.SetAsLastSibling();
            }
        }

        private void updateContentSize()
        {
            RectTransform scrollContent = scrollRect.content;
            int rowCount = Mathf.CeilToInt((float)DataList.Count / Columns);
            Vector2 contentSize = calculateContentSize(rowCount);
            scrollContent.sizeDelta = contentSize;
        }

        private Vector2 calculateContentSize(int rowCount)
        {
            if (DataList.Count == 0) return Vector2.zero;

            float width = (_cellSize.x * Columns) + (_spacing.x * (Columns - 1));
            float height = (_cellSize.y * rowCount) + (_spacing.y * (rowCount - 1));
            return new Vector2(width, height);
        }

        private FTAnchor calculateCellAnchor(int index)
        {
            int row = index / Columns;
            float yFrom = row * (_cellSize.y + _spacing.y);
            float height = _cellSize.y;
            return new FTAnchor(-yFrom, height);
        }

        private Vector2 calculateCellPosition(int index, FTAnchor ftAnchor)
        {
            int col = index % Columns;
            float posX = col * (_cellSize.x + _spacing.x);
            float posY = _direction == Direction.TOP_BOTTOM ? ftAnchor.from : -ftAnchor.from;
            return new Vector2(posX, posY);
        }

        private FTAnchor getViewportFTAnchor()
        {
            float viewportHeight = scrollRect.viewport.rect.height;
            Vector2 contentPos = scrollRect.content.anchoredPosition;

            if (_direction == Direction.TOP_BOTTOM)
            {
                float contentTop = contentPos.y;
                return new FTAnchor(
                    _extendVisibleRange - contentTop,
                    viewportHeight + _extendVisibleRange * 2
                );
            }
            else
            {
                float contentBottom = contentPos.y;
                return new FTAnchor(
                    _extendVisibleRange - contentBottom + viewportHeight,
                    viewportHeight + _extendVisibleRange * 2
                );
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

            var itemAndSpacingSize = _cellSize.y + _spacing.y;
            int firstRow;
            int lastRow;
            if (_direction == Direction.TOP_BOTTOM)
            {
                firstRow = -Mathf.CeilToInt(viewportFTAnchor.from / itemAndSpacingSize);
                lastRow = -Mathf.CeilToInt(viewportFTAnchor.to / itemAndSpacingSize);

                firstRow = Mathf.Max(0, firstRow);
                lastRow = Mathf.Min(lastRow, Mathf.CeilToInt((float)DataList.Count / Columns) - 1);
            }
            else
            {
                firstRow = Mathf.FloorToInt(viewportFTAnchor.to / itemAndSpacingSize);
                lastRow = Mathf.FloorToInt(viewportFTAnchor.from / itemAndSpacingSize);

                firstRow = Mathf.Max(0, firstRow);
                lastRow = Mathf.Min(lastRow, Mathf.FloorToInt((float)DataList.Count / Columns) - 1);
            }
            int fromIndex = firstRow * Columns;
            int toIndex = Mathf.Min(lastRow * Columns + Columns - 1, latestIndex);

            return new FTIndex(fromIndex, toIndex);
        }

        public override void snap(int index, float duration = 0)
        {
            if (index >= DataList.Count || scrollRect.content.rect.height < scrollRect.viewport.rect.height)
                return;

            int row = index / Columns;
            float cellPosY = row * (_cellSize.y + _spacing.y);
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

        protected override void onRemovedItem(int index, InfiniteCellData data)
        {
            _cellFTAnchorList.removeLast();
            int rowCount = Mathf.CeilToInt((float)DataList.Count / Columns);
            scrollRect.content.sizeDelta = calculateContentSize(rowCount);
        }
        protected override void onCellSizeUpdated(int index, Vector2 oldSize, Vector2 newSize)
        {
            _cellFTAnchorList[index] = new FTAnchor(_cellFTAnchorList[index].from, newSize.y);
            int rowCount = Mathf.CeilToInt((float)DataList.Count / Columns);
            scrollRect.content.sizeDelta = calculateContentSize(rowCount);
            recalculateCellPositions(index);
        }
        protected override void updateContentAnchorAndPivot()
        {
            RectTransform scrollContent = scrollRect.content;
            float anchorY = _direction == Direction.TOP_BOTTOM ? 1 : 0;
            var cellRectAnchor = new Vector2(0, anchorY);
            if (_direction == Direction.TOP_BOTTOM)
            {
                scrollContent.anchorMin = cellRectAnchor;
                scrollContent.anchorMax = cellRectAnchor;
                scrollContent.pivot = cellRectAnchor;
            }
            else
            {
                scrollContent.anchorMin = cellRectAnchor;
                scrollContent.anchorMax = cellRectAnchor;
                scrollContent.pivot = cellRectAnchor;
            }
        }
        protected override void recalculateCellPositions(int startIndex = 1)
        {
            setDirty();
            if (startIndex <= _visibleIndices.fromIndex)
            {
                for (int i = _visibleIndices.fromIndex; i <= _visibleIndices.toIndex; i++)
                {
                    hideCell(i);
                }
                _visibleIndices = new FTIndex(-1, -1);
            }
            else if (_visibleIndices.fromIndex <= startIndex && startIndex <= _visibleIndices.toIndex)
            {
                for (int i = startIndex; i <= _visibleIndices.toIndex; i++)
                {
                    hideCell(i);
                }
                _visibleIndices.toIndex = startIndex - 1;
            }
            for (int i = startIndex; i < DataList.Count; i++)
            {
                _cellFTAnchorList[i] = calculateCellAnchor(i);
            }
        }
    }
}