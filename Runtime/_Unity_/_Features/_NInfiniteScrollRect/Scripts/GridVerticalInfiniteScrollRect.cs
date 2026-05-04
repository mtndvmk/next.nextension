using UnityEngine;
using Unity.Collections;

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
                    setDirtyPosition(0);
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
                    setDirtyPosition(0);
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
                    setDirtyLayout();
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
                    updateCellSize(value);
                }
            }
        }

        public int Columns { get; private set; }

        protected NativeSegmentTree _rowSegmentTree;
        protected FTIndex _visibleIndices = new FTIndex(-1, -1);

        protected override void Awake()
        {
            base.Awake();
            _rowSegmentTree = new NativeSegmentTree(16, Allocator.Persistent);
            updateContentAnchorAndPivot();
        }

        protected virtual void OnDestroy()
        {
            if (_rowSegmentTree.IsCreated)
            {
                _rowSegmentTree.Dispose();
            }
        }

        protected override void OnValidate()
        {
            if (NStartRunner.IsPlaying)
            {
                if (_dataList.Count > 0 && _dataList[0].cellSize != _cellSize)
                {
                    updateCellSize(_cellSize);
                }
            }
            base.OnValidate();
        }

        protected override void onInheritedAddedNewItem(ref InfiniteCellData data)
        {
            if (_dataList.Count == 1)
            {
                calculateColumns();
            }

            int rowCount = Mathf.CeilToInt((float)_dataList.Count / Columns);
            _rowSegmentTree.SetSize(rowCount);
            
            data.cellSize = _cellSize;
            int rowIndex = data.index / Columns;
            _rowSegmentTree.Set(rowIndex, _cellSize.y + _spacing.y);

            updateContentSize();
        }

        protected void updateCellSize(Vector2 cellSize)
        {
            _cellSize = cellSize;
            for (int i = 0; i < _dataList.Count; i++)
            {
                var cellData = _dataList[i];
                cellData.cellSize = cellSize;
                _dataList[i] = cellData;
            }
            calculateColumns();
            updateContentSize();
            setDirtyPosition(0);
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
            if (Columns <= 0) Columns = 1;
        }

        protected override void onLayoutUpdated()
        {
            if (_dataList.Count == 0)
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
                int row = i / Columns;
                var rowAnchor = getRowFTAnchor(row);
                Vector2 position = calculateCellPosition(i, rowAnchor);
                var cell = showCell(i);
                var cellRectTransform = cell.rectTransform();
                var originPivot = cellRectTransform.pivot;

                cellRectTransform.anchorMin = cellRectAnchor;
                cellRectTransform.anchorMax = cellRectAnchor;
                cellRectTransform.pivot = cellRectAnchor;
                cellRectTransform.anchoredPosition = position;
                cell.transform.SetAsLastSibling();

                cellRectTransform.setPivotWithoutChangePosition(originPivot);
            }
        }

        private void updateContentSize()
        {
            RectTransform scrollContent = scrollRect.content;
            int rowCount = Mathf.CeilToInt((float)_dataList.Count / Columns);
            Vector2 contentSize = calculateContentSize(rowCount);
            scrollContent.sizeDelta = contentSize;
        }

        private Vector2 calculateContentSize(int rowCount)
        {
            if (_dataList.Count == 0) return Vector2.zero;

            float width = (_cellSize.x * Columns) + (_spacing.x * (Columns - 1));
            float height = _headOffset + _rowSegmentTree.TotalSum - _spacing.y + _tailOffset;
            return new Vector2(width, height);
        }

        private FTAnchor getRowFTAnchor(int row)
        {
            if (row < 0 || row >= _rowSegmentTree.Size) return new FTAnchor(0, 0);
            
            float sumBefore = _rowSegmentTree.GetSum(row);
            float from = -_headOffset - sumBefore;
            float size = _rowSegmentTree.Get(row) - _spacing.y;
            return new FTAnchor(from, size);
        }

        private Vector2 calculateCellPosition(int index, FTAnchor rowAnchor)
        {
            int col = index % Columns;
            float posX = col * (_cellSize.x + _spacing.x);
            float posY = _direction == Direction.TOP_BOTTOM ? rowAnchor.from : -rowAnchor.from;
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

            int firstRow;
            int lastRow;
            
            if (_direction == Direction.TOP_BOTTOM)
            {
                float topTargetSum = -viewportFTAnchor.from - _headOffset;
                float bottomTargetSum = -viewportFTAnchor.to - _headOffset;
                
                firstRow = _rowSegmentTree.FindIndex(topTargetSum);
                lastRow = _rowSegmentTree.FindIndex(bottomTargetSum);
            }
            else
            {
                float topTargetSum = viewportFTAnchor.to - _headOffset;
                float bottomTargetSum = viewportFTAnchor.from - _headOffset;
                
                firstRow = _rowSegmentTree.FindIndex(topTargetSum);
                lastRow = _rowSegmentTree.FindIndex(bottomTargetSum);
            }

            firstRow = Mathf.Max(0, firstRow);
            lastRow = Mathf.Min(lastRow, Mathf.CeilToInt((float)_dataList.Count / Columns) - 1);
            
            int fromIndex = firstRow * Columns;
            int toIndex = Mathf.Min(lastRow * Columns + Columns - 1, latestIndex);

            return new FTIndex(fromIndex, toIndex);
        }

        public override void snap(int index, float duration = 0)
        {
            if (index >= _dataList.Count || scrollRect.content.rect.height < scrollRect.viewport.rect.height)
                return;

            int row = index / Columns;
            float cellPosY = -getRowFTAnchor(row).from;
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
            int row;

            if (_direction == Direction.TOP_BOTTOM)
            {
                float contentTop = scrollRect.content.anchoredPosition.y;
                float posY = -contentTop - (1 - normalizedPosition) * viewportHeight;
                float targetSum = -posY - _headOffset;
                row = _rowSegmentTree.FindIndex(targetSum);
            }
            else
            {
                float contentBottom = scrollRect.content.anchoredPosition.y;
                float posY = contentBottom + normalizedPosition * viewportHeight;
                float targetSum = posY - _headOffset;
                row = _rowSegmentTree.FindIndex(targetSum);
            }

            row = Mathf.Clamp(row, 0, Mathf.CeilToInt((float)_dataList.Count / Columns) - 1);
            return Mathf.Min(row * Columns, latestIndex);
        }

        public override void snapCellIndexToNormalizePosition(int index, float normalizedPosition, float duration = 0)
        {
            if (index < 0 || index >= _dataList.Count || scrollRect.content.rect.height < scrollRect.viewport.rect.height)
            {
                return;
            }

            int row = index / Columns;
            float from = getRowFTAnchor(row).from;
            float rowSize = _cellSize.y;
            float viewportHeight = scrollRect.viewport.rect.height;
            float maxScroll = scrollRect.content.rect.height - viewportHeight;
            float targetY;

            if (_direction == Direction.TOP_BOTTOM)
            {
                targetY = -from - (1 - normalizedPosition) * (viewportHeight - rowSize);
                targetY = Mathf.Clamp(targetY, 0, maxScroll);
            }
            else
            {
                targetY = from + normalizedPosition * (viewportHeight - rowSize);
                targetY = Mathf.Clamp(targetY, -maxScroll, 0);
            }

            if (scrollRect.content.anchoredPosition.y != targetY)
            {
                innerSnap(new Vector2(0, targetY), duration);
            }
        }

        protected override void onRemovedItem(int index, in InfiniteCellData data)
        {
            int rowCount = Mathf.CeilToInt((float)_dataList.Count / Columns);
            _rowSegmentTree.SetSize(rowCount);
            scrollRect.content.sizeDelta = calculateContentSize(rowCount);
        }

        protected override void onCellSizeUpdated(int index, Vector2 oldSize, Vector2 newSize)
        {
            int row = index / Columns;
            _rowSegmentTree.Set(row, newSize.y + _spacing.y);
            int rowCount = Mathf.CeilToInt((float)_dataList.Count / Columns);
            scrollRect.content.sizeDelta = calculateContentSize(rowCount);
            setDirtyPosition(index);
        }

        protected override void updateContentAnchorAndPivot()
        {
            RectTransform scrollContent = scrollRect.content;
            float anchorY = _direction == Direction.TOP_BOTTOM ? 1 : 0;
            var cellRectAnchor = new Vector2(0, anchorY);
            scrollContent.anchorMin = cellRectAnchor;
            scrollContent.anchorMax = cellRectAnchor;
            scrollContent.pivot = cellRectAnchor;
        }

        protected override void exeCalculateCellPositions(int startIndex = 1)
        {
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
                _visibleIndices = new FTIndex(_visibleIndices.fromIndex, startIndex - 1);
            }

            int rowCount = Mathf.CeilToInt((float)_dataList.Count / Columns);
            _rowSegmentTree.SetSize(rowCount);

            int startRow = Mathf.Max(0, startIndex / Columns);
            for (int i = startRow; i < rowCount; i++)
            {
                _rowSegmentTree.Set(i, _cellSize.y + _spacing.y);
            }
            setDirtyLayout();
        }
        
        public override void clear()
        {
            base.clear();
            _visibleIndices = new FTIndex(-1, -1);
            _rowSegmentTree.SetSize(0);
        }
    }
}