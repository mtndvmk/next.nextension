using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace Nextension
{
    public abstract class OneDInfiniteScrollRect : InfiniteScrollRect
    {
        [SerializeField] protected float _spacing;
        [SerializeField] protected float _extendVisibleRange;
        [SerializeField] protected bool _isReverse;

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
        protected NList<FTAnchor> _cellFTAnchorList = new NList<FTAnchor>();
        protected FTIndex _visibleIndices = new FTIndex(-1, -1);

        protected int getFromVisibleIndex_1To0(float topMostY, int fromIndex, int toIndex)
        {
            var middleIndex = (toIndex + fromIndex) >> 1;
            var midFTAnchor = _cellFTAnchorList[middleIndex];
            if (middleIndex == fromIndex)
            {
                if (midFTAnchor.to > topMostY) return toIndex;
                return fromIndex;
            }
            if (midFTAnchor.to > topMostY)
            {
                return getFromVisibleIndex_1To0(topMostY, middleIndex + 1, toIndex);
            }
            else
            {
                return getFromVisibleIndex_1To0(topMostY, fromIndex, middleIndex);
            }
        }
        protected int getToVisibleIndex_1To0(float bottomMostY, int fromIndex, int toIndex)
        {
            var middleIndex = (toIndex + fromIndex) >> 1;
            var midFTAnchor = _cellFTAnchorList[middleIndex];
            if (middleIndex == fromIndex)
            {
                if (midFTAnchor.from < bottomMostY) return fromIndex;
                return toIndex;
            }
            if (midFTAnchor.from < bottomMostY)
            {
                return getToVisibleIndex_1To0(bottomMostY, fromIndex, middleIndex - 1);
            }
            else
            {
                return getToVisibleIndex_1To0(bottomMostY, middleIndex, toIndex);
            }
        }
        protected int getFromVisibleIndex_0To1(float topMostY, int fromIndex, int toIndex)
        {
            var middleIndex = (toIndex + fromIndex) >> 1;
            var midFTAnchor = _cellFTAnchorList[middleIndex];
            if (middleIndex == fromIndex)
            {
                if (midFTAnchor.to > topMostY) return toIndex;
                return fromIndex;
            }
            if (midFTAnchor.from > topMostY)
            {
                return getFromVisibleIndex_0To1(topMostY, middleIndex, toIndex);
            }
            else
            {
                return getFromVisibleIndex_0To1(topMostY, fromIndex, middleIndex - 1);
            }
        }
        protected int getToVisibleIndex_0To1(float bottomMostY, int fromIndex, int toIndex)
        {
            var middleIndex = (toIndex + fromIndex) >> 1;
            var midFTAnchor = _cellFTAnchorList[middleIndex];
            if (middleIndex == fromIndex)
            {
                if (midFTAnchor.to < bottomMostY) return fromIndex;
                return toIndex;
            }
            if (midFTAnchor.to < bottomMostY)
            {
                return getToVisibleIndex_0To1(bottomMostY, fromIndex, middleIndex);
            }
            else
            {
                return getToVisibleIndex_0To1(bottomMostY, middleIndex + 1, toIndex);
            }
        }
        protected override unsafe void exeCalculateCellPositions(int startIndex = 1)
        {
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

                if (startIndex <= _visibleIndices.fromIndex || _cellFTAnchorList[0].from != 0)
                {

                    _cellFTAnchorList[0] = new FTAnchor(0, _cellFTAnchorList[0].size);
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
                if (startIndex <= 0)
                {
                    startIndex = 1;
                }
            }

            int cellCount = DataList.Count;
            if (cellCount <= startIndex)
            {
                return;
            }

            fixed (FTAnchor* cellFTAnchorListStartPtr = &_cellFTAnchorList.i_Items[startIndex])
            {
                var job = new CalculateCellPositionJob(cellFTAnchorListStartPtr, cellCount - startIndex, _spacing, _isReverse);
                job.RunByRef();
            }
        }
        public override void clear()
        {
            base.clear();
            _visibleIndices = new FTIndex(-1, -1);
            _cellFTAnchorList.Clear();
        }

        [BurstCompile]
        protected unsafe struct CalculateCellPositionJob : IJob
        {
            [NativeDisableUnsafePtrRestriction] private FTAnchor* _cellFTAnchorList;
            private float _spacing;
            private int _count;
            private bool _isReverse;

            public CalculateCellPositionJob(FTAnchor* cellFTAnchorList, int count, float spacing, bool isReverse)
            {
                _cellFTAnchorList = cellFTAnchorList;
                _count = count;
                _spacing = spacing;
                _isReverse = isReverse;
            }
            public void Execute()
            {
                if (_isReverse)
                {
                    var index = _count - 1;
                    _cellFTAnchorList[index] = new FTAnchor(0, _cellFTAnchorList[index].size);
                    for (int i = index - 1; i >= 0; i--)
                    {
                        var prevFTAnchorTo = _cellFTAnchorList[i + 1].to;
                        _cellFTAnchorList[i] = new FTAnchor(prevFTAnchorTo - _spacing, _cellFTAnchorList[i].size);
                    }
                }
                else
                {
                    for (int i = 0; i < _count; i++)
                    {
                        var prevFTAnchorTo = _cellFTAnchorList[i - 1].to;
                        _cellFTAnchorList[i] = new FTAnchor(prevFTAnchorTo - _spacing, _cellFTAnchorList[i].size);
                    }
                }
            }
        }
    }
}
