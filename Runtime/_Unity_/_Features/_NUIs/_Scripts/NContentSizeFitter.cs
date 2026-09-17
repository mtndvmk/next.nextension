using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Nextension
{
    /// <summary>
    /// Same as UnityEngine.UI.ContentSizeFitter, with an extra size offset and an optional min/max size for each dimension.
    /// <para>Fitted size = clamp((min or preferred size of the content), minSize, maxSize) + sizeOffset, min/max don't include sizeOffset.</para>
    /// </summary>
    [ExecuteAlways, DisallowMultipleComponent, RequireComponent(typeof(RectTransform))]
    public class NContentSizeFitter : UIBehaviour, ILayoutSelfController
    {
        public enum FitMode
        {
            Unconstrained,
            MinSize,
            PreferredSize,
        }

        [SerializeField] private Vector2 _sizeOffset;

        [SerializeField] private FitMode _horizontalFit = FitMode.Unconstrained;
        [NIndent, SerializeField] private bool _useMinWidth;
        [NIndent, NShowIf(nameof(_useMinWidth)), SerializeField] private float _minWidth;
        [NIndent, SerializeField] private bool _useMaxWidth;
        [NIndent, NShowIf(nameof(_useMaxWidth)), SerializeField] private float _maxWidth;

        [SerializeField] private FitMode _verticalFit = FitMode.Unconstrained;
        [NIndent, SerializeField] private bool _useMinHeight;
        [NIndent, NShowIf(nameof(_useMinHeight)), SerializeField] private float _minHeight;
        [NIndent, SerializeField] private bool _useMaxHeight;
        [NIndent, NShowIf(nameof(_useMaxHeight)), SerializeField] private float _maxHeight;

        [System.NonSerialized] private RectTransform _rectTransform;
        private DrivenRectTransformTracker _tracker;

        public RectTransform rectTransform
        {
            get
            {
                if (_rectTransform == null) _rectTransform = transform.asRectTransform();
                return _rectTransform;
            }
        }

        public FitMode horizontalFit
        {
            get => _horizontalFit;
            set { if (_horizontalFit == value) return; _horizontalFit = value; setDirty(); }
        }
        public FitMode verticalFit
        {
            get => _verticalFit;
            set { if (_verticalFit == value) return; _verticalFit = value; setDirty(); }
        }
        /// <summary>
        /// Extra size added to the fitted size, x on the horizontal axis, y on the vertical axis
        /// </summary>
        public Vector2 sizeOffset
        {
            get => _sizeOffset;
            set { if (_sizeOffset == value) return; _sizeOffset = value; setDirty(); }
        }
        /// <summary>
        /// Null means unlimited
        /// </summary>
        public float? minWidth
        {
            get => _useMinWidth ? _minWidth : null;
            set { __setLimit(ref _useMinWidth, ref _minWidth, value); }
        }
        /// <summary>
        /// Null means unlimited
        /// </summary>
        public float? maxWidth
        {
            get => _useMaxWidth ? _maxWidth : null;
            set { __setLimit(ref _useMaxWidth, ref _maxWidth, value); }
        }
        /// <summary>
        /// Null means unlimited
        /// </summary>
        public float? minHeight
        {
            get => _useMinHeight ? _minHeight : null;
            set { __setLimit(ref _useMinHeight, ref _minHeight, value); }
        }
        /// <summary>
        /// Null means unlimited
        /// </summary>
        public float? maxHeight
        {
            get => _useMaxHeight ? _maxHeight : null;
            set { __setLimit(ref _useMaxHeight, ref _maxHeight, value); }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            setDirty();
        }
        protected override void OnDisable()
        {
            _tracker.Clear();
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            base.OnDisable();
        }
        protected override void OnRectTransformDimensionsChange()
        {
            setDirty();
        }
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            setDirty();
        }
#endif

        public void setDirty()
        {
            if (!IsActive()) return;
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }
        public void setSizeOffset(float horizontal, float vertical)
        {
            sizeOffset = new Vector2(horizontal, vertical);
        }
        public void setSizeOffset(RectTransform.Axis axis, float offset)
        {
            var newSizeOffset = _sizeOffset;
            if (axis == RectTransform.Axis.Horizontal) newSizeOffset.x = offset;
            else newSizeOffset.y = offset;
            sizeOffset = newSizeOffset;
        }
        public void setMinSize(RectTransform.Axis axis, float? minSize)
        {
            if (axis == RectTransform.Axis.Horizontal) minWidth = minSize;
            else minHeight = minSize;
        }
        public void setMaxSize(RectTransform.Axis axis, float? maxSize)
        {
            if (axis == RectTransform.Axis.Horizontal) maxWidth = maxSize;
            else maxHeight = maxSize;
        }

        public virtual void SetLayoutHorizontal()
        {
            _tracker.Clear();
            __handleSelfFittingAlongAxis(0);
        }
        public virtual void SetLayoutVertical()
        {
            __handleSelfFittingAlongAxis(1);
        }

        private void __setLimit(ref bool useLimit, ref float limit, float? newLimit)
        {
            var newUseLimit = newLimit.HasValue;
            var newValue = newLimit ?? limit;
            if (useLimit == newUseLimit && limit == newValue) return;
            useLimit = newUseLimit;
            limit = newValue;
            setDirty();
        }
        private void __handleSelfFittingAlongAxis(int axis)
        {
            var rectTf = rectTransform;
            var fitMode = axis == 0 ? _horizontalFit : _verticalFit;
            if (fitMode == FitMode.Unconstrained)
            {
                // Keep a reference to the tracked transform, but don't control its properties
                _tracker.Add(this, rectTf, DrivenTransformProperties.None);
                return;
            }

            _tracker.Add(this, rectTf, axis == 0 ? DrivenTransformProperties.SizeDeltaX : DrivenTransformProperties.SizeDeltaY);

            var size = fitMode == FitMode.MinSize ? LayoutUtility.GetMinSize(rectTf, axis) : LayoutUtility.GetPreferredSize(rectTf, axis);
            rectTf.SetSizeWithCurrentAnchors((RectTransform.Axis)axis, __computeSize(axis, size));
        }
        private float __computeSize(int axis, float contentSize)
        {
            float offset, minSize, maxSize;
            bool useMinSize, useMaxSize;
            if (axis == 0)
            {
                offset = _sizeOffset.x;
                (useMinSize, minSize) = (_useMinWidth, _minWidth);
                (useMaxSize, maxSize) = (_useMaxWidth, _maxWidth);
            }
            else
            {
                offset = _sizeOffset.y;
                (useMinSize, minSize) = (_useMinHeight, _minHeight);
                (useMaxSize, maxSize) = (_useMaxHeight, _maxHeight);
            }
            if (useMinSize && contentSize < minSize) contentSize = minSize;
            if (useMaxSize && contentSize > maxSize) contentSize = maxSize;
            var size = contentSize + offset;
            return size < 0 ? 0 : size;
        }
    }
}
