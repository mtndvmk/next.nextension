using System;
using UnityEngine;

namespace Nextension
{
    [ExecuteAlways, RequireComponent(typeof(RectTransform))]
    public class RectTransformConstraint : MonoBehaviour
    {
        [ExecuteAlways, RequireComponent(typeof(RectTransform))]
        internal class SourceConstraint : MonoBehaviour
        {
            private void OnValidate()
            {
                hideFlags = HideFlags.HideInInspector | HideFlags.DontSave;
            }
            public event Action onRectTransformDimensionsChange;
            private void OnRectTransformDimensionsChange()
            {
                onRectTransformDimensionsChange?.Invoke();
            }
        }

        [SerializeField] private RectTransform _source;
        [SerializeField] private bool _freezeWidth;
        [SerializeField] private bool _freezeHeight;
        [SerializeField] private Vector2 _sizeOffset;
        
        public RectTransform source
        {
            get => _source;
            set
            {
                removeEvent();
                _sourceController = null;
                _source = value;
                addEvent();
            }
        }
        public bool freezeWidth
        {
            get => _freezeWidth;
            set
            {
                _freezeWidth = value;
                updateDimensions();
            }
        }
        public bool freezeHeight
        {
            get => _freezeHeight;
            set
            {
                _freezeHeight = value;
                updateDimensions();
            }
        }
        public Vector2 sizeOffset
        {
            get => _sizeOffset;
            set
            {
                _sizeOffset = value;
                updateDimensions();
            }
        }

        private bool _isAddedEvent;
        private SourceConstraint _sourceController;

#if UNITY_EDITOR
        private bool _editor_freezeWidth;
        private bool _editor_freezeHeight;
        private Vector2 _editor_sizeOffset;
        private void OnValidate()
        {
            if (_sourceController != null)
            {
                if (_source == null)
                {
                    removeEvent();
                }
                else
                {
                    if (_sourceController.gameObject != _source.gameObject)
                    {
                        source = _source;
                    }
                }
            }
            else
            {
                if (_source != null)
                {
                    addEvent();
                }
            }

            if (_editor_freezeWidth != _freezeWidth || _editor_freezeHeight != _freezeHeight || _editor_sizeOffset != _sizeOffset)
            {
                _editor_freezeWidth = _freezeWidth;
                _editor_freezeHeight = _freezeHeight;
                _editor_sizeOffset = _sizeOffset;
                updateDimensions();
            }
        }
#endif
        private void OnEnable()
        {
            addEvent();
        }
        private void OnDisable()
        {
            removeEvent();
        }

        private void addEvent()
        {
            if (_isAddedEvent && _sourceController != null) return;

            if (!_sourceController && source)
            {
                _sourceController = source.getOrAddComponent<SourceConstraint>();
            }
            if (_sourceController)
            {
                _isAddedEvent = true;
                _sourceController.onRectTransformDimensionsChange += updateDimensions;
                updateDimensions();
            }
        }
        private void removeEvent()
        {
            if (!_isAddedEvent) return;
            if (_sourceController)
            {
                _sourceController.onRectTransformDimensionsChange -= updateDimensions;
            }
            _isAddedEvent = false;
        }

        private void updateDimensions()
        {
            var rectTf = transform.asRectTransform();
            var srcRect = _source.rect;
            if (!_freezeWidth)
            {
                rectTf.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, srcRect.size.x + _sizeOffset.x);
            }
            if (!_freezeHeight)
            {
                rectTf.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, srcRect.size.y + _sizeOffset.y);
            }
        }
    }
}
