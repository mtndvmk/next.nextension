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
            if (!HasData) return;
            onBeforeShowCell(CellIndex, in InfiniteScrollRect.getRefCellData(CellIndex));
        }

        internal void internalOnBeforeShowCell(int index, in InfiniteCellData cellData)
        {
            CellIndex = index;
            InfiniteScrollRect = cellData.infiniteScrollRect;
            onBeforeShowCell(index, in cellData);
        }

        internal void internalOnLayoutUpdated()
        {
            onLayoutUpdated();
        }

        internal void internalOnBeforeCellHide()
        {
            onBeforeHideCell();
            CellIndex = -1;
        }

        protected virtual void onBeforeShowCell(int index, in InfiniteCellData cellData)
        {

        }

        protected virtual void onLayoutUpdated()
        {
            
        }

        protected virtual void onBeforeHideCell()
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
    }
}
