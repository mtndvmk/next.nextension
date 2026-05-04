using UnityEngine;

namespace Nextension
{
    public abstract class OneDInfiniteScrollRect : InfiniteScrollRect
    {
        [SerializeField] protected float _spacing;
        [SerializeField] protected float _extendVisibleRange;
        [SerializeField] protected bool _isReverse;

        protected abstract float getCellMainSize(InfiniteCellData data);

        public float Spacing
        {
            get => _spacing;
            set
            {
                if (_spacing != value)
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

        public bool IsReverse
        {
            get => _isReverse;
            set
            {
                if (_isReverse != value)
                {
                    _isReverse = value;
                    setDirtyPosition(0);
                }
            }
        }
        protected NativeSegmentTree _segmentTree;
        protected FTIndex _visibleIndices = new FTIndex(-1, -1);

        protected override void Awake()
        {
            base.Awake();
            _segmentTree = new NativeSegmentTree(16, Unity.Collections.Allocator.Persistent);
        }

        protected virtual void OnDestroy()
        {
            _segmentTree.Dispose();
        }

        protected FTAnchor getCellFTAnchor(int index)
        {
            if (index < 0 || index >= _dataList.Count) return new FTAnchor(0, 0);

            float size = _segmentTree.Get(index) - _spacing;
            if (_isReverse)
            {
                float sumAfter = _segmentTree.GetSum(index + 1, _dataList.Count - 1);
                float from = -_headOffset - sumAfter;
                return new FTAnchor(from, size);
            }
            else
            {
                float sumBefore = _segmentTree.GetSum(index);
                float from = -_headOffset - sumBefore;
                return new FTAnchor(from, size);
            }
        }

        protected int findIndexByCoordinate(float coord)
        {
            if (_dataList.Count == 0) return -1;
            if (_isReverse)
            {
                float targetSum = _segmentTree.TotalSum + coord + _headOffset;
                int index = _segmentTree.FindIndex(targetSum) - 1;
                return Mathf.Clamp(index, 0, _dataList.Count - 1);
            }
            else
            {
                float targetSum = -coord - _headOffset;
                int index = _segmentTree.FindIndex(targetSum);
                return Mathf.Clamp(index, 0, _dataList.Count - 1);
            }
        }

        protected int getFromVisibleIndex_1To0(float topMostY, int fromIndex, int toIndex)
        {
            return findIndexByCoordinate(topMostY);
        }

        protected int getToVisibleIndex_1To0(float bottomMostY, int fromIndex, int toIndex)
        {
            return findIndexByCoordinate(bottomMostY);
        }

        protected int getFromVisibleIndex_0To1(float topMostY, int fromIndex, int toIndex)
        {
            return findIndexByCoordinate(topMostY);
        }

        protected int getToVisibleIndex_0To1(float bottomMostY, int fromIndex, int toIndex)
        {
            return findIndexByCoordinate(bottomMostY);
        }

        protected int getNearestCellIndex(float targetFrom, int fromIndex, int toIndex)
        {
            return findIndexByCoordinate(targetFrom);
        }

        protected override void exeCalculateCellPositions(int startIndex = 1)
        {
            if (_dataList.Count <= 0) return;

            setDirtyLayout();
            if (_isReverse)
            {
                for (int i = _visibleIndices.fromIndex; i <= _visibleIndices.toIndex; i++)
                {
                    hideCell(i);
                }
                _visibleIndices = new FTIndex(-1, -1);
                startIndex = 0;
            }
            else
            {
                if (startIndex <= _visibleIndices.fromIndex || getCellFTAnchor(0).from != -_headOffset)
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
                if (startIndex <= 0)
                {
                    startIndex = 0;
                }
            }

            _segmentTree.SetSize(_dataList.Count);

            for (int i = startIndex; i < _dataList.Count; i++)
            {
                _segmentTree.Set(i, getCellMainSize(_dataList[i]) + _spacing);
            }
        }
        public override void clear()
        {
            base.clear();
            _visibleIndices = new FTIndex(-1, -1);
            _segmentTree.SetSize(0);
        }
    }
}
