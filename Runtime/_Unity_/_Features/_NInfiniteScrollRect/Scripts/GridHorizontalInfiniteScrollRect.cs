using UnityEngine;
using Unity.Collections;

namespace Nextension
{
    public class GridHorizontalInfiniteScrollRect : InfiniteScrollRect
    {
        public enum Direction
        {
            LEFT_RIGHT,
            RIGHT_LEFT,
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

        public int Rows { get; private set; }

        protected NativeSegmentTree _colSegmentTree;
        protected FTIndex _visibleIndices = new FTIndex(-1, -1);

        protected override void Awake()
        {
            base.Awake();
            _colSegmentTree = new NativeSegmentTree(16, Allocator.Persistent);
            updateContentAnchorAndPivot();
        }

        protected virtual void OnDestroy()
        {
            if (_colSegmentTree.IsCreated)
            {
                _colSegmentTree.Dispose();
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

        protected void updateCellSize(Vector2 cellSize)
        {
            _cellSize = cellSize;
            for (int i = 0; i < _dataList.Count; i++)
            {
                var cellData = _dataList[i];
                cellData.cellSize = cellSize;
                _dataList[i] = cellData;
            }
            calculateRows();
            updateContentSize();
            setDirtyPosition(0);
        }

        protected override void onInheritedAddedNewItem(ref InfiniteCellData data)
        {
            if (_dataList.Count == 1)
            {
                calculateRows();
            }

            int colCount = Mathf.CeilToInt((float)_dataList.Count / Rows);
            _colSegmentTree.SetSize(colCount);
            
            data.cellSize = _cellSize;
            int colIndex = data.index / Rows;
            _colSegmentTree.Set(colIndex, _cellSize.x + _spacing.x);

            updateContentSize();
        }

        private void calculateRows()
        {
            var scrollRectViewport = scrollRect.viewport;
            if (scrollRectViewport.rect.height == 0)
            {
                transform.asRectTransform().markLayoutForRebuild(true);
            }
            var cellHeight = _cellSize.y;
            Rows = (int)((scrollRectViewport.rect.height + _spacing.y) / (cellHeight + _spacing.y));
            if (Rows <= 0) Rows = 1;
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
            float anchorX = _direction == Direction.RIGHT_LEFT ? 1 : 0;
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
            var cellRectAnchor = new Vector2(anchorX, 1);

            for (int i = fromVisibleIndex; i <= toVisibleIndex; i++)
            {
                int col = i / Rows;
                var colAnchor = getColFTAnchor(col);
                Vector2 position = calculateCellPosition(i, colAnchor);
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
            int colCount = Mathf.CeilToInt((float)_dataList.Count / Rows);
            Vector2 contentSize = calculateContentSize(colCount);
            scrollContent.sizeDelta = contentSize;
        }

        private Vector2 calculateContentSize(int colCount)
        {
            if (_dataList.Count == 0) return Vector2.zero;

            float height = (_cellSize.y * Rows) + (_spacing.y * (Rows - 1));
            float width = _headOffset + _colSegmentTree.TotalSum - _spacing.x + _tailOffset;
            return new Vector2(width, height);
        }

        private FTAnchor getColFTAnchor(int col)
        {
            if (col < 0 || col >= _colSegmentTree.Size) return new FTAnchor(0, 0);
            
            float sumBefore = _colSegmentTree.GetSum(col);
            float from = -_headOffset - sumBefore;
            float size = _colSegmentTree.Get(col) - _spacing.x;
            return new FTAnchor(from, size);
        }

        private Vector2 calculateCellPosition(int index, FTAnchor colAnchor)
        {
            int row = index % Rows;
            float posY = -row * (_cellSize.y + _spacing.y);
            float posX = _direction == Direction.RIGHT_LEFT ? colAnchor.from : -colAnchor.from;
            return new Vector2(posX, posY);
        }

        private FTAnchor getViewportFTAnchor()
        {
            float viewportWidth = scrollRect.viewport.rect.width;
            Vector2 contentPos = scrollRect.content.anchoredPosition;

            if (_direction == Direction.RIGHT_LEFT)
            {
                float contentRight = contentPos.x;
                return new FTAnchor(
                    _extendVisibleRange - contentRight,
                    viewportWidth + _extendVisibleRange * 2
                );
            }
            else
            {
                float contentLeft = contentPos.x;
                return new FTAnchor(
                    _extendVisibleRange - contentLeft + viewportWidth,
                    viewportWidth + _extendVisibleRange * 2
                );
            }
        }

        private FTIndex getVisibleIndices()
        {
            int latestIndex = _dataList.Count - 1;

            if (scrollRect.viewport.rect.width == 0)
            {
                transform.asRectTransform().markLayoutForRebuild(true);
            }

            if (scrollRect.content.rect.width < scrollRect.viewport.rect.width)
            {
                return new FTIndex(0, latestIndex);
            }

            FTAnchor viewportFTAnchor = getViewportFTAnchor();

            int firstCol;
            int lastCol;
            
            if (_direction == Direction.RIGHT_LEFT)
            {
                float topTargetSum = -viewportFTAnchor.from - _headOffset;
                float bottomTargetSum = -viewportFTAnchor.to - _headOffset;
                
                firstCol = _colSegmentTree.FindIndex(topTargetSum);
                lastCol = _colSegmentTree.FindIndex(bottomTargetSum);
            }
            else
            {
                float topTargetSum = viewportFTAnchor.to - _headOffset;
                float bottomTargetSum = viewportFTAnchor.from - _headOffset;
                
                firstCol = _colSegmentTree.FindIndex(topTargetSum);
                lastCol = _colSegmentTree.FindIndex(bottomTargetSum);
            }

            firstCol = Mathf.Max(0, firstCol);
            lastCol = Mathf.Min(lastCol, Mathf.CeilToInt((float)_dataList.Count / Rows) - 1);
            
            int fromIndex = firstCol * Rows;
            int toIndex = Mathf.Min(lastCol * Rows + Rows - 1, latestIndex);

            return new FTIndex(fromIndex, toIndex);
        }

        public override void snap(int index, float duration = 0)
        {
            if (index >= _dataList.Count || scrollRect.content.rect.width < scrollRect.viewport.rect.width)
                return;

            int col = index / Rows;
            float cellPosX = -getColFTAnchor(col).from;
            cellPosX = Mathf.Min(scrollRect.content.rect.width - scrollRect.viewport.rect.width, cellPosX);

            if (_direction == Direction.LEFT_RIGHT)
            {
                cellPosX = -cellPosX;
            }

            if (scrollRect.content.anchoredPosition.x != cellPosX)
            {
                innerSnap(new Vector2(cellPosX, 0), duration);
            }
        }

        public override int GetCellIndexAtViewportPosition(float normalizedPosition)
        {
            if (_dataList.Count == 0) return -1;
            int latestIndex = _dataList.Count - 1;

            float viewportWidth = scrollRect.viewport.rect.width;
            int col;

            if (_direction == Direction.RIGHT_LEFT)
            {
                float contentRight = scrollRect.content.anchoredPosition.x;
                float posX = -contentRight - (1 - normalizedPosition) * viewportWidth;
                float targetSum = -posX - _headOffset;
                col = _colSegmentTree.FindIndex(targetSum);
            }
            else
            {
                float contentLeft = scrollRect.content.anchoredPosition.x;
                float posX = contentLeft + normalizedPosition * viewportWidth;
                float targetSum = posX - _headOffset;
                col = _colSegmentTree.FindIndex(targetSum);
            }

            col = Mathf.Clamp(col, 0, Mathf.CeilToInt((float)_dataList.Count / Rows) - 1);
            return Mathf.Min(col * Rows, latestIndex);
        }

        public override void snapCellIndexToNormalizePosition(int index, float normalizedPosition, float duration = 0)
        {
            if (index < 0 || index >= _dataList.Count || scrollRect.content.rect.width < scrollRect.viewport.rect.width)
            {
                return;
            }

            int col = index / Rows;
            float from = getColFTAnchor(col).from;
            float colSize = _cellSize.x;
            float viewportWidth = scrollRect.viewport.rect.width;
            float maxScroll = scrollRect.content.rect.width - viewportWidth;
            float targetX;

            if (_direction == Direction.RIGHT_LEFT)
            {
                targetX = -from - (1 - normalizedPosition) * (viewportWidth - colSize);
                targetX = Mathf.Clamp(targetX, 0, maxScroll);
            }
            else
            {
                targetX = from + normalizedPosition * (viewportWidth - colSize);
                targetX = Mathf.Clamp(targetX, -maxScroll, 0);
            }

            if (scrollRect.content.anchoredPosition.x != targetX)
            {
                innerSnap(new Vector2(targetX, 0), duration);
            }
        }

        protected override void onRemovedItem(int index, in InfiniteCellData data)
        {
            int colCount = Mathf.CeilToInt((float)_dataList.Count / Rows);
            _colSegmentTree.SetSize(colCount);
            scrollRect.content.sizeDelta = calculateContentSize(colCount);
        }

        protected override void onCellSizeUpdated(int index, Vector2 oldSize, Vector2 newSize)
        {
            int col = index / Rows;
            _colSegmentTree.Set(col, newSize.x + _spacing.x);
            int colCount = Mathf.CeilToInt((float)_dataList.Count / Rows);
            scrollRect.content.sizeDelta = calculateContentSize(colCount);
            setDirtyPosition(index);
        }

        protected override void updateContentAnchorAndPivot()
        {
            RectTransform scrollContent = scrollRect.content;
            float anchorX = _direction == Direction.RIGHT_LEFT ? 1 : 0;
            var cellRectAnchor = new Vector2(anchorX, 1);
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

            int colCount = Mathf.CeilToInt((float)_dataList.Count / Rows);
            _colSegmentTree.SetSize(colCount);

            int startCol = Mathf.Max(0, startIndex / Rows);
            for (int i = startCol; i < colCount; i++)
            {
                _colSegmentTree.Set(i, _cellSize.x + _spacing.x);
            }
            setDirtyLayout();
        }
        
        public override void clear()
        {
            base.clear();
            _visibleIndices = new FTIndex(-1, -1);
            _colSegmentTree.SetSize(0);
        }
    }
}
