using UnityEngine;

namespace Nextension
{
    public class InfiniteCellData
    {
        public int Index { get; internal set; } = -1;
        public Vector2 CellSize { get; internal set; }
        public Vector3 CellScale { get; internal set; } = Vector3.one;
        public InfiniteScrollRect InfiniteScrollRect { get; internal set; }
        public InfiniteCell InfiniteCell => InfiniteScrollRect?.getShowingCell(Index);

        public InfiniteCellData()
        {
        }

        public InfiniteCellData(Vector2 cellSize)
        {
            this.CellSize = cellSize;
        }

        public void setSize(Vector2 newSize)
        {
            if (CellSize != newSize)
            {
                this.CellSize = newSize;
                if (InfiniteScrollRect)
                {
                    InfiniteScrollRect.updateCellSize(Index, newSize);
                }
            }
        }
        public void setScale(Vector3 newScale)
        {
            if (CellScale != newScale)
            {
                this.CellScale = newScale;
                if (InfiniteScrollRect)
                {
                    InfiniteScrollRect.updateCellScale(Index, newScale);
                }
            }
        }
    }
    public class InfiniteCellData<T> : InfiniteCellData
    {
        public T Data { get; internal set; }
        public InfiniteCellData()
        {
        }
        public InfiniteCellData(Vector2 cellSize, T data)
        {
            this.CellSize = cellSize;
            this.Data = data;
        }
    }
}
