using UnityEngine;

namespace Nextension
{
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

        public InfiniteCellData(float x, float y)
        {
            this.cellSize = new Vector2(x, y);
            index = -1;
            cellScale = Vector3.one;
            infiniteScrollRect = null;
            this.exData = null;
            _isCreated = true;
        }
        public InfiniteCellData(float x, float y, object exData, float scale = 1)
        {
            this.cellSize = new Vector2(x, y);
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
