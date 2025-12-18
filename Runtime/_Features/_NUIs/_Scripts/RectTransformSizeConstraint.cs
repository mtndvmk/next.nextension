using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Nextension
{
    [ExecuteAlways, RequireComponent(typeof(RectTransform))]
    public class RectTransformSizeConstraint : MonoBehaviour, ILayoutIgnorer, ILayoutSelfController
    {
        [SerializeField] private RectTransform _source;
        [SerializeField] private bool _isConstraintWidth;
        [SerializeField] private bool _isConstraintHeight;
        [SerializeField] private Vector2 _sizeAtRest;
        [SerializeField] private Vector2 _sizeOffset;
        [SerializeField] private Vector2 _minSize;
        [SerializeField] private bool _isRotated;
        [SerializeField] private bool _forceSetInactiveSource;
        [SerializeField] private bool _isIgnoreLayout;

        private Vector2 _srcRectSize;
        private DrivenRectTransformTracker _tracker;

        public RectTransform source
        {
            get => _source;
            set
            {
                _source = value;
            }
        }
        public bool IsConstraintWidth
        {
            get => _isConstraintWidth;
            set
            {
                _isConstraintWidth = value;
                updateDimensions();
            }
        }
        public bool IsConstraintHeight
        {
            get => _isConstraintHeight;
            set
            {
                _isConstraintHeight = value;
                updateDimensions();
            }
        }
        public Vector2 SizeOffset
        {
            get => _sizeOffset;
            set
            {
                _sizeOffset = value;
                updateDimensions();
            }
        }

        bool ILayoutIgnorer.ignoreLayout => _isIgnoreLayout;

        private void OnValidate()
        {
            _srcRectSize = Vector2.negativeInfinity;
        }
        private void OnDisable()
        {
            _srcRectSize = Vector2.negativeInfinity;
            _tracker.Clear();
            DrivenTransformPropertiesHolder.clear(this);
        }
        private void LateUpdate()
        {
            __updateDrivenRectTransformTracker();
            updateDimensions();
        }
        private void OnRectTransformDimensionsChange()
        {
            _srcRectSize = Vector2.negativeInfinity;
        }
        private void __updateDrivenRectTransformTracker()
        {
#if UNITY_EDITOR
            if (UnityEditor.Selection.activeGameObject != gameObject) return;
            _tracker.Clear();
            DrivenTransformPropertiesHolder.clear(this);
            var trackValue = DrivenTransformProperties.None;
            if (_isConstraintWidth) trackValue |= DrivenTransformProperties.SizeDeltaX;
            if (_isConstraintHeight) trackValue |= DrivenTransformProperties.SizeDeltaY;
            trackValue = DrivenTransformPropertiesHolder.add(this, trackValue);
            _tracker.Add(this, transform.asRectTransform(), trackValue);
#endif
        }
        public void forceUpdateDimensions()
        {
            updateDimensions(true);
        }
        private void updateDimensions(bool rebuildSrcLayout = false)
        {
            if (_source == null) return;
            var rectTf = transform.asRectTransform();
            bool atRest = false;
            Vector2 srcRectSize;
            if (!_forceSetInactiveSource && !_source.gameObject.activeInHierarchy)
            {
                atRest = true;
                srcRectSize = Vector2.zero;
            }
            else
            {
                if (rebuildSrcLayout)
                {
                    _source.markLayoutForRebuild(true);
                }
                srcRectSize = _source.rect.size;
            }
            if (_srcRectSize == srcRectSize) return;
            _srcRectSize = srcRectSize;

            if (_source.IsChildOf(rectTf))
            {
                if (_isConstraintWidth)
                {
                    if (_source.anchorMin.x != _source.anchorMax.x)
                    {
                        _source.anchorMin = new Vector2(0.5f, _source.anchorMin.y);
                        _source.anchorMax = new Vector2(0.5f, _source.anchorMax.y);
                        _source.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, srcRectSize.x);
                    }
                }

                if (_isConstraintHeight)
                {
                    if (_source.anchorMin.y != _source.anchorMax.y)
                    {
                        _source.anchorMin = new Vector2(_source.anchorMin.x, 0.5f);
                        _source.anchorMax = new Vector2(_source.anchorMax.x, 0.5f);
                        _source.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, srcRectSize.y);
                    }
                }
            }

            var sizeOffset = _sizeOffset;
            var minSize = _minSize;
            var sizeAtRest = _sizeAtRest;

            if (_isRotated)
            {
                (srcRectSize.x, srcRectSize.y) = (srcRectSize.y, srcRectSize.x);
                (sizeOffset.x, sizeOffset.y) = (sizeOffset.y, sizeOffset.x);
                (minSize.x, minSize.y) = (minSize.y, minSize.x);
                (sizeAtRest.x, sizeAtRest.y) = (sizeAtRest.y, sizeAtRest.x);
            }

            if (_isConstraintWidth)
            {
                float newSize;
                if (atRest)
                {
                    newSize = sizeAtRest.x;
                }
                else
                {
                    newSize = srcRectSize.x + sizeOffset.x;
                }
                if (newSize < minSize.x)
                {
                    newSize = minSize.x;
                }
                if (newSize != rectTf.rect.width)
                {
                    rectTf.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newSize);
                }
            }
            if (_isConstraintHeight)
            {
                float newSize;
                if (atRest)
                {
                    newSize = sizeAtRest.y;
                }
                else
                {
                    newSize = srcRectSize.y + sizeOffset.y;
                }
                if (newSize < minSize.y)
                {
                    newSize = minSize.y;
                }
                if (newSize != rectTf.rect.height)
                {
                    rectTf.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newSize);
                }
            }
        }

        public void SetLayoutHorizontal()
        {
            
        }

        public void SetLayoutVertical()
        {
            forceUpdateDimensions();
        }
    }

    internal static class DrivenTransformPropertiesHolder
    {
#if UNITY_EDITOR
        private static Dictionary<int, DrivenTransformProperties> _driverHolders = new();
        private static RectTransform _currentTarget;
        private static DrivenTransformProperties _currentProperties;
#endif
        public static DrivenTransformProperties clear(MonoBehaviour driver)
        {
#if UNITY_EDITOR
            RectTransform target = driver.rectTransform();
            if (target == null) { return DrivenTransformProperties.None; }
            var driverInsId = driver.GetInstanceID();

            if (_driverHolders.TryGetValue(driverInsId, out var p))
            {
                _driverHolders.Remove(driverInsId);
                p = _currentProperties &= ~p;
                return p;
            }
#endif
            return DrivenTransformProperties.None;
        }
        public static DrivenTransformProperties add(MonoBehaviour driver, DrivenTransformProperties properties)
        {
#if UNITY_EDITOR
            RectTransform target = driver.rectTransform();
            if (target == null) { return properties; }
            if (_currentTarget != target) { _currentProperties = DrivenTransformProperties.None; }
            _currentTarget = target;
            var driverInsId = driver.GetInstanceID();

            if (_driverHolders.TryGetValue(driverInsId, out var p))
            {
                _driverHolders[driverInsId] = properties;
                _currentProperties &= ~p;
            }
            else
            {
                _driverHolders.Add(driverInsId, properties);
            }
            p = _currentProperties |= properties;
            return p;
#else
            return properties;
#endif
        }
    }
}
