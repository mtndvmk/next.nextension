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
        [SerializeField] private RectValue<RectTransform>[] _subEdges;
        [SerializeField] private RectValue<bool> _includeInactive;
        [SerializeField] private bool _disableHorizontal;
        [SerializeField] private bool _disableVertical;

        public bool disableHorizontal { get => _disableHorizontal; set { if (_disableHorizontal == value) return; _disableHorizontal = value; IsDirty = true; } }
        public bool disableVertical { get => _disableVertical; set { if (_disableVertical == value) return; _disableVertical = value; IsDirty = true; } }


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

            var trackValue = DrivenTransformProperties.Pivot;
            if (!_disableHorizontal)
            {
                trackValue |= DrivenTransformProperties.AnchorMinX
                    | DrivenTransformProperties.AnchorMaxX
                    | DrivenTransformProperties.AnchoredPositionX
                    | DrivenTransformProperties.SizeDeltaX;
            }
            if (!_disableVertical)
            {
                trackValue |= DrivenTransformProperties.AnchorMinY
                    | DrivenTransformProperties.AnchorMaxY
                    | DrivenTransformProperties.AnchoredPositionY
                    | DrivenTransformProperties.SizeDeltaY;
            }

            trackValue = DrivenTransformPropertiesHolder.add(this, trackValue);
            _tracker.Add(this, transform.asRectTransform(), trackValue);
#endif
        }


        private bool __hasEdgeChanged()
        {
            bool isChanged = IsDirty;

            if (!_disableHorizontal)
            {
                if (!isChanged) isChanged = _edgeCheckers.left.isChanged(__getEdge(Edge.Left));
                if (!isChanged) isChanged = _edgeCheckers.right.isChanged(__getEdge(Edge.Right));
            }

            if (!_disableVertical)
            {
                if (!isChanged) isChanged = _edgeCheckers.top.isChanged(__getEdge(Edge.Top));
                if (!isChanged) isChanged = _edgeCheckers.bottom.isChanged(__getEdge(Edge.Bottom));
            }

            return isChanged;
        }


        private RectTransform __getEdge(Edge edge)
        {
            if (__isValid(_edges, edge, out var rect)) return rect;
            if (_subEdges != null)
            {
                foreach (var subEdge in _subEdges)
                {
                    if (__isValid(subEdge, edge, out rect)) return rect;
                }
            }
            return null;
        }

        private bool __isValid(RectValue<RectTransform> value, Edge edge, out RectTransform rect)
        {
            rect = edge switch
            {
                Edge.Left => value.left,
                Edge.Right => value.right,
                Edge.Top => value.top,
                Edge.Bottom => value.bottom,
                _ => null
            };
            if (rect == null) return false;
            bool includeInactive = edge switch
            {
                Edge.Left => _includeInactive.left,
                Edge.Right => _includeInactive.right,
                Edge.Top => _includeInactive.top,
                Edge.Bottom => _includeInactive.bottom,
                _ => false
            };
            return rect.gameObject.activeInHierarchy || includeInactive;
        }

        private void LateUpdate()
        {
            if (!(transform is RectTransform self))
            {
                return;
            }
            if (!(transform.parent is RectTransform parent))
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
            Vector2 anchorMin = self.anchorMin;
            Vector2 anchorMax = self.anchorMax;
            Vector2 offsetMin = self.offsetMin;
            Vector2 offsetMax = self.offsetMax;

            var parentSize = parent.rect.size;

            if (!_disableVertical)
            {
                anchorMin.y = 0;
                anchorMax.y = 1;
                offsetMin.y = _padding.bottom;
                offsetMax.y = -_padding.top;

                var top = __getEdge(Edge.Top);
                if (top)
                {
                    anchorMax.y = top.getRectInRootSpace(parent).yMin / parentSize.y;
                }

                var bottom = __getEdge(Edge.Bottom);
                if (bottom)
                {
                    anchorMin.y = bottom.getRectInRootSpace(parent).yMax / parentSize.y;
                }
            }

            if (!_disableHorizontal)
            {
                anchorMin.x = 0;
                anchorMax.x = 1;
                offsetMin.x = _padding.left;
                offsetMax.x = -_padding.right;

                var left = __getEdge(Edge.Left);
                if (left)
                {
                    anchorMin.x = left.getRectInRootSpace(parent).xMax / parentSize.x;
                }

                var right = __getEdge(Edge.Right);
                if (right)
                {
                    anchorMax.x = right.getRectInRootSpace(parent).xMin / parentSize.x;
                }
            }

            self.anchorMin = anchorMin;
            self.anchorMax = anchorMax;
            self.offsetMin = offsetMin;
            self.offsetMax = offsetMax;
        }


        private void __onPostUpdate()
        {
            IsDirty = false;

            if (!_disableHorizontal)
            {
                _edgeCheckers.left.applyData(__getEdge(Edge.Left));
                _edgeCheckers.right.applyData(__getEdge(Edge.Right));
            }

            if (!_disableVertical)
            {
                _edgeCheckers.top.applyData(__getEdge(Edge.Top));
                _edgeCheckers.bottom.applyData(__getEdge(Edge.Bottom));
            }
        }
    }
}
