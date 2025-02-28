using System.Runtime.InteropServices;
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

        public float Spacing
        {
            get => _spacing;
            set
            {
                if (_spacing != value)
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
        protected override unsafe void recalculateCellPositions(int startIndex = 1)
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
            if (startIndex <= 0)
            {
                startIndex = 1;
            }

            int cellCount = DataList.Count;
            if (cellCount <= startIndex)
            {
                return;
            }

            var gcHandle = GCHandle.Alloc(_cellFTAnchorList.i_Items, GCHandleType.Pinned);
            var cellFTAnchorListStartPtr = ((FTAnchor*)gcHandle.AddrOfPinnedObject()) + startIndex;
            var job = new CalculateCellPositionJob(cellFTAnchorListStartPtr, cellCount - startIndex, _spacing);
            var jobHandle = job.ScheduleByRef();

            jobHandle.Complete();
            gcHandle.Free();
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

            public CalculateCellPositionJob(FTAnchor* cellFTAnchorList, int count, float spacing)
            {
                _cellFTAnchorList = cellFTAnchorList;
                _count = count;
                _spacing = spacing;
            }
            public void Execute()
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
