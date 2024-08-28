using Nextension.Tween;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Nextension
{
    public abstract class InfiniteScrollRect : UIBehaviour
    {
        public InfiniteCell cellPrefab;
        public ScrollRect scrollRect;
        public List<InfiniteCellData> DataList { get; internal set; } = new List<InfiniteCellData>();
        protected Dictionary<int, InfiniteCell> _cellTable = new Dictionary<int, InfiniteCell>();
        protected Queue<InfiniteCell> cellPool = new Queue<InfiniteCell>();

        private NTweener _snapAnimation;
        private bool _isDirtyLayout;

        private Func<InfiniteCell> _instantiateCellFunc;
        private Action<InfiniteCell> _destroyCellItemFunc;

        protected override void Reset()
        {
            base.Reset();
            scrollRect = GetComponent<ScrollRect>();
        }
        protected override void Awake()
        {
            base.Awake();
            scrollRect.onValueChanged.AddListener((_) => onLayoutUpdated());
        }
        protected virtual void LateUpdate()
        {
            if (_isDirtyLayout)
            {
                forceUpdateLayout();
            }
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            stopSnapping();
        }

        protected virtual InfiniteCell getCell()
        {
            InfiniteCell cell;
            if (_instantiateCellFunc != null && _destroyCellItemFunc != null)
            {
                cell = _instantiateCellFunc();
                cell.transform.SetParent(scrollRect.content, false);
                return cell;
            }
            else
            {
                if (cellPool.Count > 0)
                {
                    cell = cellPool.Dequeue();
                }
                else
                {
                    cell = Instantiate(cellPrefab, scrollRect.content);
                }

                cell.gameObject.SetActive(true);
                return cell;
            }
        }
        protected void releaseCell(InfiniteCell cell)
        {
            cell.CellData = null;
            if (_instantiateCellFunc != null && _destroyCellItemFunc != null)
            {
                _destroyCellItemFunc(cell);
            }
            else
            {
                cell.gameObject.SetActive(false);
                cellPool.Enqueue(cell);
            }
        }
        protected void hideCell(int cellIndex)
        {
            if (_cellTable.tryTakeAndRemove(cellIndex, out var cell))
            {
                releaseCell(cell);
            }
        }
        protected void setDirty()
        {
            _isDirtyLayout = true;
        }

        public void forceUpdateLayout()
        {
            onLayoutUpdated();
            _isDirtyLayout = false;
        }

        protected abstract void onLayoutUpdated();

        public virtual void add(InfiniteCellData data)
        {
            data.Index = DataList.Count;
            data.InfiniteScrollRect = this;
            DataList.Add(data);
            onAddedNewItem(data);
            setDirty();
        }
        protected virtual void onAddedNewItem(InfiniteCellData data)
        {

        }
        public virtual void remove(int index)
        {
            if (DataList.Count == 0 || index >= DataList.Count) return;
            DataList.RemoveAt(index);
            hideCell(index);
            recalculateCellPositions(index);
            setDirty();
        }

        public void insert(int index, InfiniteCellData data)
        {
            for (int i = index; i < DataList.Count; i++)
            {
                DataList[i].Index++;
            };

            data.Index = index;
            data.InfiniteScrollRect = this;
            DataList.Insert(index, data);
            onAddedNewItem(data);
            recalculateCellPositions(index);
            setDirty();
        }

        public void addRange(IEnumerable<InfiniteCellData> dataEnumerable)
        {
            foreach (var d in dataEnumerable)
            {
                add(d);
            }
        }

        public abstract void snap(int index, float duration = 0);
        public void snapToFirst(float duration = 0)
        {
            if (DataList.Count == 0) return;
            snap(0, duration);
        }
        public void snapToLast(float duration = 0)
        {
            if (DataList.Count == 0) return;
            snap(DataList.Count - 1, duration);
        }

        protected void innerSnap(Vector2 contentAnchorPosition, float duration)
        {
            stopSnapping();
            if (!gameObject.activeInHierarchy || duration <= 0)
            {
                scrollRect.content.anchoredPosition = contentAnchorPosition;
                setDirty();
            }
            else
            {
                exeSnapAnimation(contentAnchorPosition, duration);
            }
        }
        public void stopSnapping()
        {
            if (_snapAnimation != null)
            {
                scrollRect.velocity = Vector2.zero;
                _snapAnimation.cancel();
                _snapAnimation = null;
            }
        }
        private void exeSnapAnimation(Vector2 contentAnchorPosition, float duration)
        {
            _snapAnimation = NTween.fromTo(scrollRect.content.anchoredPosition, contentAnchorPosition, duration, tPos =>
            {
                scrollRect.content.anchoredPosition = tPos;
            });
        }

        protected InfiniteCell showCell(int index, Vector2 cellAnchoredPosition)
        {
            if (_cellTable.ContainsKey(index))
            {
                return _cellTable[index];
            }
            else
            {
                var cell = getCell();
                var cellData = DataList[index];
                cell.internalUpdateCellData(index, cellData);

                var cellRectTf = cell.rectTransform();
                cellRectTf.anchoredPosition = cellAnchoredPosition;
                cellRectTf.localScale = cellData.CellScale;
                cellRectTf.setSizeWithCurrentAnchors(cellData.CellSize);
                _cellTable.Add(index, cell);
                return cell;
            }
        }

        public virtual void clear()
        {
            stopSnapping();
            scrollRect.content.anchoredPosition = Vector2.zero;
            DataList.Clear();
            foreach (var cell in _cellTable.Values)
            {
                releaseCell(cell);
            }
            _cellTable.Clear();
            setDirty();
        }

        public void updateCellSize(int index, Vector2 newSize)
        {
            var cellData = DataList[index];
            var oldSize = cellData.CellSize;
            cellData.CellSize = newSize;
            hideCell(index);
            onCellSizeUpdated(index, oldSize, newSize);
            setDirty();
        }
        public void updateCellScale(int index, Vector3 newScale)
        {
            var cellData = DataList[index];
            cellData.CellScale = newScale;
            _cellTable[index].transform.localScale = newScale;
        }
        protected virtual void onCellSizeUpdated(int index, Vector2 oldSize, Vector2 newSize) { }
        protected abstract void recalculateCellPositions(int startIndex = 1);

        public void overrideCellInstantiation(Func<InfiniteCell> instantiateCellFunc, Action<InfiniteCell> destroyCellItemFunc)
        {
            _instantiateCellFunc = instantiateCellFunc;
            _destroyCellItemFunc = destroyCellItemFunc;
        }
        public void resetCellInstantiationFunc()
        {
            overrideCellInstantiation(default, default);
        }
        public InfiniteCell getShowingCell(int index)
        {
            if (_cellTable.TryGetValue(index, out var cell)) return cell;
            return null;
        }
    }
}
