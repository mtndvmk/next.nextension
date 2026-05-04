using UnityEngine;

namespace Nextension
{
    public class InfiniteCell : MonoBehaviour
    {
        public int CellIndex { get; private set; } = -1;
        public InfiniteScrollRect InfiniteScrollRect {get; private set; }
        public bool HasData => CellIndex != -1 && InfiniteScrollRect != null;
        public InfiniteCellData CellData => !HasData ? default : InfiniteScrollRect.DataList[CellIndex];

        public void refreshCellData()
        {
            onUpdateCellData(CellIndex, CellData);
        }

        internal void releaseCellData()
        {
            CellIndex = -1;
        }

        internal void internalUpdateCellData(int index, in InfiniteCellData cellData)
        {
            CellIndex = index;
            InfiniteScrollRect = cellData.infiniteScrollRect;
            onUpdateCellData(index, in cellData);
        }

        protected virtual void onUpdateCellData(int index, in InfiniteCellData cellData)
        {

        }

        protected void setScale(float scale)
        {
            setScale(new Vector3(scale, scale, scale));
        }

        protected void setScale(Vector3 scale)
        {
            if (InfiniteScrollRect.isNull()) return;
            InfiniteScrollRect.updateCellScale(CellIndex, scale);
        }

        protected void setSize(float x, float y)
        {
            setSize(new Vector2(x, y));
        }

        protected void setSize(Vector2 size)
        {
            if (InfiniteScrollRect.isNull()) return;
            InfiniteScrollRect.updateCellSize(CellIndex, size);
        }

    }
    public class InfiniteCell<T> : InfiniteCell
    {
        public T Data => (T)CellData.exData;

        protected sealed override void onUpdateCellData(int index, in InfiniteCellData cellData)
        {
            onUpdateCellData(index, (T)cellData.exData);
        }
        protected virtual void onUpdateCellData(int index, T cellData)
        {

        }
    }
}
