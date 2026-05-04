using UnityEngine;

namespace Nextension
{
    // public class InfiniteCellData
    // {
    //     public static Vector2 PositiveInfinitySize => Vector2.positiveInfinity;
    //     public int Index { get; internal set; } = -1;
    //     public Vector2 CellSize { get; internal set; } = Vector2.positiveInfinity;
    //     public Vector3 CellScale { get; internal set; } = Vector3.one;
    //     public InfiniteScrollRect InfiniteScrollRect { get; internal set; }
    //     public InfiniteCell InfiniteCell => InfiniteScrollRect.isNull() ? null : InfiniteScrollRect.getShowingCell(Index);

    //     public InfiniteCellData()
    //     {
    //     }

    //     public InfiniteCellData(float cellSizeX, float cellSizeY)
    //     {
    //         this.CellSize = new Vector2(cellSizeX, cellSizeY);
    //     }

    //     public InfiniteCellData(Vector2 cellSize)
    //     {
    //         this.CellSize = cellSize;
    //     }

    //     public void setSize(Vector2 newSize)
    //     {
    //         if (CellSize != newSize)
    //         {
    //             if (InfiniteScrollRect)
    //             {
    //                 InfiniteScrollRect.updateCellSize(Index, newSize);
    //             }
    //             this.CellSize = newSize;
    //         }
    //     }
    //     public void setScale(Vector3 newScale)
    //     {
    //         if (CellScale != newScale)
    //         {
    //             this.CellScale = newScale;
    //             if (InfiniteScrollRect)
    //             {
    //                 InfiniteScrollRect.updateCellScale(Index, newScale);
    //             }
    //         }
    //     }
    //     public void setScale(float newScale)
    //     {
    //         setScale(new Vector3(newScale, newScale, newScale));
    //     }

    // }
    // public class InfiniteCellData<T> : InfiniteCellData
    // {
    //     public T Data { get; internal set; }
    //     public InfiniteCellData()
    //     {
    //     }
    //     public InfiniteCellData(float cellSizeX, float cellSizeY, T data)
    //     {
    //         this.CellSize = new Vector2(cellSizeX, cellSizeY);
    //         this.Data = data;
    //     }
    //     public InfiniteCellData(Vector2 cellSize, T data)
    //     {
    //         this.CellSize = cellSize;
    //         this.Data = data;
    //     }
    // }

    public struct InfiniteCellData
    {
        public static Vector2 PositiveInfinitySize => Vector2.positiveInfinity;
        
        private bool _isCreated;
        
        public int index;
        public Vector2 cellSize;
        public Vector3 cellScale;
        public InfiniteScrollRect infiniteScrollRect;
        public object exData;

        public readonly bool IsShown => _isCreated && index != -1;
        public readonly bool IsCreated => _isCreated;

        public InfiniteCellData(float x, float y, object exData, float scale = 1)
        {
            cellSize = new Vector2(x, y);
            index = -1;
            cellScale = new Vector3(scale, scale, scale);
            infiniteScrollRect = null;
            this.exData = exData;
            _isCreated = true;
        }

        public InfiniteCellData(Vector2 size, object exData, float scale = 1)
        {
            this.cellSize = size;
            index = -1;
            cellScale = new Vector3(scale, scale, scale);
            infiniteScrollRect = null;
            this.exData = exData;
            _isCreated = true;
        }
    }
}
