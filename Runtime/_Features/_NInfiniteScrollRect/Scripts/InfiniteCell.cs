using System;
using UnityEngine;

namespace Nextension
{
    public class InfiniteCell : MonoBehaviour
    {
        public int CellIndex => CellData == null ? -1 : CellData.Index;
        public InfiniteScrollRect InfiniteScrollRect => CellData?.InfiniteScrollRect;
        public InfiniteCellData CellData { get; internal set; }

        public void refreshCellData()
        {
            onUpdateCellData(CellIndex, CellData);
        }

        internal void internalUpdateCellData(int index, InfiniteCellData cellData)
        {
            CellData = cellData;
            onUpdateCellData(index, cellData);
        }

        protected virtual void onUpdateCellData(int index, InfiniteCellData cellData)
        {

        }
    }
    public class InfiniteCell<T> : InfiniteCell where T : InfiniteCellData
    {
        protected sealed override void onUpdateCellData(int index, InfiniteCellData cellData)
        {
            onUpdateCellData(index, cellData as T);
        }
        protected virtual void onUpdateCellData(int index, T cellData)
        {

        }
    }
}
