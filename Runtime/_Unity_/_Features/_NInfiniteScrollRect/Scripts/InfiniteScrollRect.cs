using Nextension.Tween;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Nextension
{
    public abstract class InfiniteScrollRect : MonoBehaviour, IBeginDragHandler, IEndDragHandler
    {
        protected readonly struct FTAnchor
        {
            public readonly float from;
            public readonly float to;
            public readonly float size => from - to;

            public FTAnchor(float from, float size)
            {
                this.from = from;
                to = from - size;
            }
        }
        protected readonly struct FTIndex
        {
            public readonly int fromIndex;
            public readonly int toIndex;
            public FTIndex(int fromIndex, int toIndex)
            {
                this.fromIndex = fromIndex;
                this.toIndex = toIndex;
            }
        }

        public ScrollRect scrollRect;
        public InfiniteCell cellPrefab;

        [SerializeField] protected float _headOffset;
        [SerializeField] protected float _tailOffset;

        public float HeadOffset
        {
            get => _headOffset;
            set
            {
                if (_headOffset != value)
                {
                    _headOffset = value;
                    setDirtyPosition(0);
                }
            }
        }
        public float TailOffset
        {
            get => _tailOffset;
            set
            {
                if (_tailOffset != value)
                {
                    _tailOffset = value;
                    setDirtyLayout();
                }
            }
        }

        public IReadOnlyList<InfiniteCellData> DataList => _dataList;

        protected readonly Dictionary<int, InfiniteCell> _showingCellTable = new Dictionary<int, InfiniteCell>();
        protected readonly List<InfiniteCell> _cellPool = new List<InfiniteCell>();
        [NonSerialized] protected readonly List<InfiniteCellData> _dataList = new List<InfiniteCellData>();

        public event Action<InfiniteCell> onCellShown;
        public event Action<InfiniteCell> onCellReleased;

        private NTweener _snapAnimation;
        private bool _isDirtyLayout;
        private int _dirtyPositionIndex = -1;
        private Action<float2> _setContentAnchoredPositionAction;
        private Action _stopSnapAction;
        public bool IsSnapping => _snapAnimation != null;
        public bool IsDragging { get; private set; }

        private IComponentInstantiator<InfiniteCell> _instantiator;

        protected virtual void Reset()
        {
            scrollRect = GetComponent<ScrollRect>();
        }
        
        protected virtual async void OnValidate()
        {
            if (NStartRunner.IsPlaying)
            {
                await new NWaitFrame(1);
                updateContentAnchorAndPivot();
                setDirtyPosition(0);
            }
            if (scrollRect && enabled && scrollRect?.enabled == false)
            {
                Debug.LogWarning("Enabled scrollRect");
                scrollRect.enabled = true;
            }
        }

        protected virtual void Awake()
        {
            if (!scrollRect) scrollRect = GetComponent<ScrollRect>();
            scrollRect.onValueChanged.AddListener((_) => onLayoutUpdated());

        }
        
        protected virtual void LateUpdate()
        {
            if (_dataList.Count <= 0) return;
            if (_dirtyPositionIndex >= 0)
            {
                exeCalculateCellPositions(_dirtyPositionIndex);
                _dirtyPositionIndex = -1;
            }
            if (_isDirtyLayout)
            {
                forceUpdateLayout();
            }
        }
        
        protected virtual void OnDisable()
        {
            stopSnapping();
        }

        private void __setContentAnchoredPosition(float2 pos)
        {
            scrollRect.content.anchoredPosition = pos;
        }
        
        private InfiniteCell __getCellFromPool()
        {
            InfiniteCell cell;
            if (_instantiator != null)
            {
                cell = _instantiator.getComponent();
                cell.transform.SetParent(scrollRect.content, false);
                return cell;
            }
            else
            {
                if (_cellPool.Count > 0)
                {
                    cell = _cellPool.takeAndRemoveLast();
                }
                else
                {
                    cell = Instantiate(cellPrefab, scrollRect.content);
                }

                cell.gameObject.SetActive(true);
                return cell;
            }
        }
        
        private void __releaseCell(InfiniteCell cell)
        {
            cell.releaseCellData();
            if (_instantiator != null)
            {
                _instantiator.release(cell.gameObject);
            }
            else
            {
                cell.gameObject.SetActive(false);
                _cellPool.Add(cell);
            }
            onCellReleased?.Invoke(cell);
        }
        protected void hideCell(int cellIndex)
        {
            if (_showingCellTable.tryTakeAndRemove(cellIndex, out var cell))
            {
                __releaseCell(cell);
            }
        }
        protected void setDirtyLayout()
        {
            _isDirtyLayout = true;
        }
        protected void setDirtyPosition(int index = 0)
        {
            _dirtyPositionIndex = index;
        }

        public void forceUpdateLayout()
        {
            onLayoutUpdated();
            _isDirtyLayout = false;
        }

        protected abstract void onLayoutUpdated();

        public void add<T>(in T data)
        {
            if (data is not InfiniteCellData cellData)
            {
                cellData = new InfiniteCellData(cellPrefab.rectTransform().rect.size, data);
            }
            add(cellData);
        }

        public void add(in InfiniteCellData cellData)
        {
            var index = _dataList.Count;
            _dataList.Add(cellData);
            __onAddItem(index);
        }

        public void insert<T>(int index, in T data)
        {
            if (data is not InfiniteCellData cellData)
            {
                cellData = new InfiniteCellData(cellPrefab.rectTransform().rect.size, data);
            }
            insert(index, cellData);
        }

        public void insert(int index, in InfiniteCellData cellData)
        {
            for (int i = index; i < _dataList.Count; i++)
            {
                ref var refData = ref _dataList.AsSpan()[i];
                refData.index++;
            }
            _dataList.Insert(index, cellData);
            __onAddItem(index);
        }

        public void addRange<TEnumerable>(TEnumerable dataEnumerable) where TEnumerable : IEnumerable<object>
        {
            foreach (var d in dataEnumerable)
            {
                add(d);
            }
        }

        public void addRange(ReadOnlySpan<InfiniteCellData> cellDataCollection)
        {
            foreach (var d in cellDataCollection)
            {
                add(d);
            }
        }

        private void __onAddItem(int index)
        {
            ref var refData = ref _dataList.AsSpan()[index];
            if (refData.cellSize.Equals(InfiniteCellData.PositiveInfinitySize))
            {
                refData.cellSize = cellPrefab.rectTransform().rect.size;
            }
            refData.index = index;
            refData.infiniteScrollRect = this;
            onInheritedAddedNewItem(ref refData);
            setDirtyPosition(index);
        }

        protected virtual void onInheritedAddedNewItem(ref InfiniteCellData data)
        {

        }
        public void remove(int index)
        {
            if (_dataList.Count == 0 || index >= _dataList.Count) return;
            var removedData = _dataList[index];
            _dataList.RemoveAt(index);
            hideCell(index);
            onRemovedItem(index, in removedData);
            setDirtyPosition(index);
        }
        protected virtual void onRemovedItem(int index, in InfiniteCellData data)
        {

        }

        public abstract void snap(int index, float duration = 0);
        
        public void snapToFirst(float duration = 0)
        {
            if (_dataList.Count == 0) return;
            snap(0, duration);
        }
        
        public void snapToLast(float duration = 0)
        {
            if (_dataList.Count == 0) return;
            snap(_dataList.Count - 1, duration);
        }

        protected void innerSnap(Vector2 contentAnchorPosition, float duration)
        {
            stopSnapping();
            if (!gameObject.activeInHierarchy || duration <= 0)
            {
                scrollRect.content.anchoredPosition = contentAnchorPosition;
                setDirtyLayout();
            }
            else
            {
                __exeSnapAnimation(contentAnchorPosition, duration);
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
        
        private void __exeSnapAnimation(Vector2 contentAnchorPosition, float duration)
        {
            _setContentAnchoredPositionAction ??= __setContentAnchoredPosition;
            _stopSnapAction ??= stopSnapping;
            _snapAnimation = NTween.fromTo(scrollRect.content.anchoredPosition, contentAnchorPosition, duration, _setContentAnchoredPositionAction)
                .onFinalized(_stopSnapAction);
        }
        
        protected InfiniteCell showCell(int index)
        {
            if (_showingCellTable.ContainsKey(index))
            {
                return _showingCellTable[index];
            }
            else
            {
                var cell = __getCellFromPool();
                onCellShown?.Invoke(cell);
                var cellData = _dataList[index];
                cell.internalUpdateCellData(index, in cellData);

                var cellRectTf = cell.rectTransform();
                cellRectTf.localScale = cellData.cellScale;
                cellRectTf.setSizeWithCurrentAnchors(cellData.cellSize);
                _showingCellTable.Add(index, cell);
                return cell;
            }
        }

        public virtual void clear()
        {
            stopSnapping();
            scrollRect.content.anchoredPosition = Vector2.zero;
            _dataList.Clear();
            foreach (var cell in _showingCellTable.Values)
            {
                __releaseCell(cell);
            }
            _showingCellTable.Clear();
            setDirtyLayout();
        }

        public void updateCellSize(int index, Vector2 newSize)
        {
            ref var cellData = ref _dataList.AsSpan()[index];
            var oldSize = cellData.cellSize;
            cellData.cellSize = newSize;
            hideCell(index);
            onCellSizeUpdated(index, oldSize, newSize);
            setDirtyLayout();
        }
        
        public void updateCellScale(int index, Vector3 newScale)
        {
            if (index >= _dataList.Count || index < 0) return;
            ref var cellData = ref _dataList.AsSpan()[index];
            cellData.cellScale = newScale;
            if (_showingCellTable.TryGetValue(index, out var cell))
            {
                cell.transform.localScale = newScale;
            }
        }
        
        public void forceCalculateCellPositions(int startIndex)
        {
            if (_dataList.Count <= 0) return;
            exeCalculateCellPositions(startIndex);
        }
        
        protected virtual void onCellSizeUpdated(int index, Vector2 oldSize, Vector2 newSize) { }
        
        protected abstract void exeCalculateCellPositions(int startIndex);
        
        protected abstract void updateContentAnchorAndPivot();

        public void overrideCellInstantiator(ComponentInstantiator<InfiniteCell> instantiator)
        {
            _instantiator = instantiator;
        }
        
        public void resetCellInstantiationFunc()
        {
            _instantiator = null;
        }
        
        public InfiniteCell getShowingCell(int index)
        {
            if (_showingCellTable.TryGetValue(index, out var cell)) return cell;
            return null;
        }

        public Dictionary<int, InfiniteCell>.ValueCollection getShowingCells()
        {
            return _showingCellTable.Values;
        }

        public abstract int GetCellIndexAtViewportPosition(float normalizedPosition);
        
        public abstract void snapCellIndexToNormalizePosition(int index, float normalizedPosition, float duration = 0);

        public void OnEndDrag(PointerEventData eventData)
        {
            IsDragging = false;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            IsDragging = true;
        }
    }
}
