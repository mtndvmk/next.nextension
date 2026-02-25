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

        private void OnValidate()
        {
            IsDirty = true;
        }
        private void OnRectTransformDimensionsChange()
        {
            IsDirty = true;
        }
        private void OnEnable()
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

            if (!isChanged) isChanged = _edgeCheckers.left.isChanged(__getEdge(Edge.Left));
            if (!isChanged) isChanged = _edgeCheckers.right.isChanged(__getEdge(Edge.Right));
            if (!isChanged) isChanged = _edgeCheckers.top.isChanged(__getEdge(Edge.Top));
            if (!isChanged) isChanged = _edgeCheckers.bottom.isChanged(__getEdge(Edge.Bottom));

            return isChanged;
        }

        private RectTransform __getEdge(Edge edge)
        {
            RectTransform rect;
            switch (edge)
            {
                case Edge.Left:
                    rect = _edges.left;
                    if (rect == null) return null;
                    if (rect.gameObject.activeInHierarchy || _includeInactive.left) return rect;
                    break;
                case Edge.Right:
                    rect = _edges.right;
                    if (rect == null) return null;
                    if (rect.gameObject.activeInHierarchy || _includeInactive.right) return rect;
                    break;
                case Edge.Top:
                    rect = _edges.top;
                    if (rect == null) return null;
                    if (rect.gameObject.activeInHierarchy || _includeInactive.top) return rect;
                    break;
                case Edge.Bottom:
                    rect = _edges.bottom;
                    if (rect == null) return null;
                    if (rect.gameObject.activeInHierarchy || _includeInactive.bottom) return rect;
                    break;
            }
            return null;
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

            var top = __getEdge(Edge.Top);
            if (top)
            {
                anchorMax.y = top.getRectInParentSpace().yMin / parentSize.y;
            }

            var bottom = __getEdge(Edge.Bottom);
            if (bottom)
            {
                anchorMin.y = bottom.getRectInParentSpace().yMax / parentSize.y;
            }

            var left = __getEdge(Edge.Left);
            if (left)
            {
                anchorMin.x = left.getRectInParentSpace().xMax / parentSize.x;
            }

            var right = __getEdge(Edge.Right);
            if (right)
            {
                anchorMax.x = right.getRectInParentSpace().xMin / parentSize.x;
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
