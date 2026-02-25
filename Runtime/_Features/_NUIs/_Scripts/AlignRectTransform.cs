using UnityEngine;
using UnityEngine.UI;

namespace Nextension
{
    [ExecuteAlways, RequireComponent(typeof(RectTransform))]
    public class AlignRectTransform : MonoBehaviour, ILayoutIgnorer
    {
        private enum Alignment : byte
        {
            None = 0,
            Left = 1,
            Right = 2,
            Top = 3,
            Bottom = 4
        };

        [SerializeField] private Alignment _alignTo;
        [SerializeField] private float _offsetAtFirst;
        [SerializeField] private float _spacing;

        private DrivenRectTransformTracker _tracker;
        private RectTransformChangeChecker _otherChecker;

        public bool IsDirty { get; set; } = false;

        bool ILayoutIgnorer.ignoreLayout => true;

        private void OnDisable()
        {
            _tracker.Clear();
            DrivenTransformPropertiesHolder.clear(this);
        }

        private void LateUpdate()
        {
            __updateDrivenRectTransformTracker();
            __updatePosition();
        }

        private void OnRectTransformDimensionsChange()
        {
            IsDirty = true;
        }

        private void __updateDrivenRectTransformTracker()
        {
#if UNITY_EDITOR
            if (UnityEditor.Selection.activeGameObject != gameObject) return;
            IsDirty = true;
            _tracker.Clear();
            DrivenTransformPropertiesHolder.clear(this);
            var trackValue = DrivenTransformProperties.None;
            if (_alignTo == Alignment.Top || _alignTo == Alignment.Bottom)
            {
                trackValue |= DrivenTransformProperties.AnchoredPositionY;
            }
            else
            {
                trackValue |= DrivenTransformProperties.AnchoredPositionX;
            }
            trackValue = DrivenTransformPropertiesHolder.add(this, trackValue);
            _tracker.Add(this, transform.asRectTransform(), trackValue);
#endif
        }

        private void __updatePosition()
        {
            if (transform.parent == null)
            {
                return;
            }
            switch (_alignTo)
            {
                case Alignment.Left:
                    updatePosition_Left();
                    break;
                case Alignment.Right:
                    updatePosition_Right();
                    break;
                case Alignment.Top:
                    updatePosition_Top();
                    break;
                case Alignment.Bottom:
                    updatePosition_Bottom();
                    break;
            }
        }

        private RectTransform getOtherRectTransform()
        {
            var rectTransform = transform.asRectTransform();
            var sibIndex = rectTransform.GetSiblingIndex();
            if (_alignTo == Alignment.Right || _alignTo == Alignment.Bottom)
            {
                var totalCount = rectTransform.parent.childCount;
                if (sibIndex < totalCount)
                {
                    for (int i = sibIndex + 1; i < totalCount; i++)
                    {
                        var rectTf = transform.parent.GetChild(i).asRectTransform();
                        if (rectTf.gameObject.activeInHierarchy)
                        {
                            return rectTf;
                        }
                    }
                }
            }
            else
            {
                if (sibIndex > 0)
                {
                    for (int i = sibIndex - 1; i >= 0; i--)
                    {
                        var rectTf = transform.parent.GetChild(i).asRectTransform();
                        if (rectTf.gameObject.activeInHierarchy)
                        {
                            return rectTf;
                        }
                    }
                }
            }
            return null;
        }

        private bool __isNeedToUpdate(RectTransform rectTransform, RectTransform otherRectTransform)
        {
            if (IsDirty)
            {
                return true;
            }
            if (otherRectTransform == null)
            {
                otherRectTransform = transform.parent.asRectTransform();
            }
            if (_otherChecker.isChanged(otherRectTransform))
            {
                return true;
            }
            return false;
        }
        private void __onPostUpdate(RectTransform rectTransform, RectTransform otherRectTransform)
        {
            if (otherRectTransform == null)
            {
                otherRectTransform = transform.parent.asRectTransform();
            }
            IsDirty = false;
            _otherChecker.applyData(otherRectTransform);
        }

        private void updatePosition_Top()
        {
            var rectTransform = transform.asRectTransform();
            var otherRectTransform = getOtherRectTransform();
            if (__isNeedToUpdate(rectTransform, otherRectTransform))
            {
                float otherBottom;
                if (otherRectTransform)
                {
                    otherBottom = NUtils.getBottomRight(otherRectTransform, true).y;
                }
                else
                {
                    otherBottom = NUtils.getTopRight(rectTransform.parent.asRectTransform(), true).y;
                }
                rectTransform.setPositionY(otherBottom, false);
                var anchorPos = rectTransform.anchoredPosition.y;
                if (!otherRectTransform)
                {
                    anchorPos -= _offsetAtFirst;
                }
                else
                {
                    anchorPos -= _spacing;
                }
                var deltaToPivot = rectTransform.rect.size.y * (rectTransform.pivot.y - 1);
                rectTransform.setAnchorPositionY(deltaToPivot + anchorPos);
                __onPostUpdate(rectTransform, otherRectTransform);
            }
        }
        private void updatePosition_Bottom()
        {
            var rectTransform = transform.asRectTransform();
            var otherRectTransform = getOtherRectTransform();
            if (__isNeedToUpdate(rectTransform, otherRectTransform))
            {
                float otherTop;
                if (otherRectTransform)
                {
                    otherTop = NUtils.getTopRight(otherRectTransform, true).y;
                }
                else
                {
                    otherTop = NUtils.getBottomRight(rectTransform.parent.asRectTransform(), true).y;
                }
                rectTransform.setPositionY(otherTop, false);
                var anchorPos = rectTransform.anchoredPosition.y;
                if (!otherRectTransform)
                {
                    anchorPos += _offsetAtFirst;
                }
                else
                {
                    anchorPos += _spacing;
                }
                var deltaToPivot = rectTransform.rect.size.y * rectTransform.pivot.y;
                rectTransform.setAnchorPositionY(deltaToPivot + anchorPos);
                __onPostUpdate(rectTransform, otherRectTransform);
            }
        }
        private void updatePosition_Left()
        {
            var rectTransform = transform.asRectTransform();
            var otherRectTransform = getOtherRectTransform();
            if (__isNeedToUpdate(rectTransform, otherRectTransform))
            {
                float otherRight;
                if (otherRectTransform)
                {
                    otherRight = NUtils.getBottomRight(otherRectTransform, true).x;
                }
                else
                {
                    otherRight = NUtils.getBottomLeft(rectTransform.parent.asRectTransform(), true).x;
                }
                rectTransform.setPositionX(otherRight, false);
                var anchorPos = rectTransform.anchoredPosition.x;
                if (!otherRectTransform)
                {
                    anchorPos += _offsetAtFirst;
                }
                else
                {
                    anchorPos += _spacing;
                }
                var deltaToPivot = rectTransform.rect.size.x * rectTransform.pivot.x;
                rectTransform.setAnchorPositionX(deltaToPivot + anchorPos);
                __onPostUpdate(rectTransform, otherRectTransform);
            }
        }
        private void updatePosition_Right()
        {
            var rectTransform = transform.asRectTransform();
            var otherRectTransform = getOtherRectTransform();
            if (__isNeedToUpdate(rectTransform, otherRectTransform))
            {
                float otherLeft;
                if (otherRectTransform)
                {
                    otherLeft = NUtils.getBottomLeft(otherRectTransform, true).x;
                }
                else
                {
                    otherLeft = NUtils.getBottomRight(rectTransform.parent.asRectTransform(), true).x;
                }
                rectTransform.setPositionX(otherLeft, false);
                var anchorPos = rectTransform.anchoredPosition.x;
                if (!otherRectTransform)
                {
                    anchorPos -= _offsetAtFirst;
                }
                else
                {
                    anchorPos -= _spacing;
                }
                var deltaToPivot = rectTransform.rect.size.x * (rectTransform.pivot.x - 1);
                rectTransform.setAnchorPositionX(deltaToPivot + anchorPos);
                __onPostUpdate(rectTransform, otherRectTransform);
            }
        }
    }
}
