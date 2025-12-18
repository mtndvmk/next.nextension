using System;
using UnityEngine;
using UnityEngine.UI;

namespace Nextension
{
    [ExecuteAlways, RequireComponent(typeof(RectTransform))]
    public class FitSizeToEdge : MonoBehaviour, ILayoutIgnorer
    {
        [Serializable]
        private struct RectValue<T>
        {
            public T left;
            public T right;
            public T top;
            public T bottom;
        }

        public enum Edge
        {
            Left,
            Right,
            Top,
            Bottom
        }

        [SerializeField] private RectValue<float> _padding;
        [SerializeField] private RectValue<RectTransform> _edges;
        [SerializeField] private RectValue<bool> _includeInactive;

        [NonSerialized] private RectValue<RectTransformChangeChecker> _edgeCheckers;

        private DrivenRectTransformTracker _tracker;

        public bool ignoreLayout => true;

        public bool IsDirty { get; set; } = false;

        public void setEdge(Edge edge, RectTransform rectTransform)
        {
            switch (edge)
            {
                case Edge.Left:
                    if (_edges.left != rectTransform) break;
                    _edges.left = rectTransform;
                    IsDirty = true;
                    break;
                case Edge.Right:
                    if (_edges.right != rectTransform) break;
                    _edges.right = rectTransform;
                    IsDirty = true;
                    break;
                case Edge.Top:
                    if (_edges.top != rectTransform) break;
                    _edges.top = rectTransform;
                    IsDirty = true;
                    break;
                case Edge.Bottom:
                    if (_edges.bottom != rectTransform) break;
                    _edges.bottom = rectTransform;
                    IsDirty = true;
                    break;
            }
        }
        public void setPadding(float left, float right, float top, float bottom)
        {
            _padding.left = left;
            _padding.right = right;
            _padding.top = top;
            _padding.bottom = bottom;
            IsDirty = true;
        }
        public void setPadding(Edge edge, float padding)
        {
            switch (edge)
            {
                case Edge.Left:
                    if (_padding.left == padding) break;
                    _padding.left = padding;
                    IsDirty = true;
                    break;
                case Edge.Right:
                    if (_padding.right == padding) break;
                    _padding.right = padding;
                    IsDirty = true;
                    break;
                case Edge.Top:
                    if (_padding.top == padding) break;
                    _padding.top = padding;
                    IsDirty = true;
                    break;
                case Edge.Bottom:
                    if (_padding.bottom == padding) break;
                    _padding.bottom = padding;
                    IsDirty = true;
                    break;
            }
        }
        public void setIncludeInactive(Edge edge, bool includeInactive)
        {
            switch (edge)
            {
                case Edge.Left:
                    if (_includeInactive.left == includeInactive) break;
                    _includeInactive.left = includeInactive;
                    IsDirty = true;
                    break;
                case Edge.Right:
                    if (_includeInactive.right == includeInactive) break;
                    _includeInactive.right = includeInactive;
                    IsDirty = true;
                    break;
                case Edge.Top:
                    if (_includeInactive.top == includeInactive) break;
                    _includeInactive.top = includeInactive;
                    IsDirty = true;
                    break;
                case Edge.Bottom:
                    if (_includeInactive.bottom == includeInactive) break;
                    _includeInactive.bottom = includeInactive;
                    IsDirty = true;
                    break;
            }
        }

        private void OnRectTransformDimensionsChange()
        {
            IsDirty = true;
        }

        private void OnDisable()
        {
            _tracker.Clear();
            DrivenTransformPropertiesHolder.clear(this);
        }

        private void __updateDrivenRectTransformTracker()
        {
#if UNITY_EDITOR
            if (UnityEditor.Selection.activeGameObject != gameObject) return;
            IsDirty = true;
            _tracker.Clear();
            DrivenTransformPropertiesHolder.clear(this);
            var trackValue = DrivenTransformProperties.AnchoredPosition
                | DrivenTransformProperties.AnchorMin
                | DrivenTransformProperties.AnchorMax
                | DrivenTransformProperties.Pivot
                | DrivenTransformProperties.SizeDelta;

            trackValue = DrivenTransformPropertiesHolder.add(this, trackValue);
            _tracker.Add(this, transform.asRectTransform(), trackValue);
#endif
        }

        private bool __hasEdgeChanged()
        {
            bool isChanged = IsDirty;

            if (!isChanged) isChanged = _edgeCheckers.left.isChanged(_edges.left);
            if (!isChanged) isChanged = _edgeCheckers.right.isChanged(_edges.right);
            if (!isChanged) isChanged = _edgeCheckers.top.isChanged(_edges.top);
            if (!isChanged) isChanged = _edgeCheckers.bottom.isChanged(_edges.bottom);

            return isChanged;
        }

        private void LateUpdate()
        {
            if (transform is not RectTransform self)
            {
                return;
            }
            if (transform.parent is not RectTransform parent)
            {
                return;
            }

            __updateDrivenRectTransformTracker();

            if (!__hasEdgeChanged())
            {
                return;
            }

            __updateAnchorsAndOffsets(self, parent);

            __onPostUpdate();
        }

        private void __updateAnchorsAndOffsets(RectTransform self, RectTransform parent)
        {
            Vector2 anchorMin = Vector2.zero;
            Vector2 anchorMax = Vector2.one;
            var parentSize = parent.rect.size;

            if (_edges.top)
            {
                anchorMax.y = _edges.top.getRectInParentSpace().yMin / parentSize.y;
            }
            if (_edges.bottom)
            {
                anchorMin.y = _edges.bottom.getRectInParentSpace().yMax / parentSize.y;
            }
            if (_edges.left)
            {
                anchorMin.x = _edges.left.getRectInParentSpace().xMax / parentSize.x;
            }
            if (_edges.right)
            {
                anchorMax.x = _edges.right.getRectInParentSpace().xMin / parentSize.x;
            }

            self.anchorMin = anchorMin;
            self.anchorMax = anchorMax;
            self.offsetMin = new Vector2(_padding.left, _padding.bottom);
            self.offsetMax = new Vector2(-_padding.right, -_padding.top);
        }

        private void __onPostUpdate()
        {
            IsDirty = false;
            _edgeCheckers.left.applyData(_edges.left);
            _edgeCheckers.right.applyData(_edges.right);
            _edgeCheckers.top.applyData(_edges.top);
            _edgeCheckers.bottom.applyData(_edges.bottom);
        }
    }
}
