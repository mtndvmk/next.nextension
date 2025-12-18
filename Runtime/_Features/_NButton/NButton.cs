using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Nextension
{
    [DisallowMultipleComponent]
    public class NButton : UIBehaviour, INButton, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private float _betweenClickIntervalTime = 0.1f;
        [SerializeField] private float _delayInvokeTime;
        [SerializeField] private bool _interactable = true;

        public UnityEvent onButtonDownEvent = new();
        public UnityEvent onButtonUpEvent = new();
        public UnityEvent onButtonClickEvent = new();
        public UnityEvent onButtonEnterEvent = new();
        public UnityEvent onButtonExitEvent = new();
        public UnityEvent onEnableInteractableEvent = new();
        public UnityEvent onDisableInteractableEvent = new();

        private readonly NList<INButtonListener> _listeners = new();
        private readonly NList<INButtonListener> _disableInteratableFromListeners = new();
        private readonly List<CanvasGroup> m_CanvasGroupCache = new();

        protected float _nextClickableTime;
        private bool _groupsAllowInteraction = true;
        protected bool _isSetup;

#if UNITY_EDITOR
        private bool? _editorInteractable;
        protected override void OnValidate()
        {
            if (!NStartRunner.IsPlaying && _editorInteractable != _interactable && gameObject.activeInHierarchy)
            {
                _editorInteractable = _interactable;
                invokeInteractableChangedEvent();
            }
        }
#endif
        protected override void OnCanvasGroupChanged()
        {
            var parentGroupAllowsInteraction = this.parentGroupAllowsInteraction();

            if (parentGroupAllowsInteraction != _groupsAllowInteraction)
            {
                var oldState = isInteractable();
                _groupsAllowInteraction = parentGroupAllowsInteraction;
                if (oldState != isInteractable())
                {
                    invokeInteractableChangedEvent();
                }
            }
        }
        public bool parentGroupAllowsInteraction()
        {
            Transform t = transform;
            while (t != null)
            {
                t.GetComponents(m_CanvasGroupCache);
                for (var i = 0; i < m_CanvasGroupCache.Count; i++)
                {
                    if (m_CanvasGroupCache[i].enabled && !m_CanvasGroupCache[i].interactable)
                        return false;

                    if (m_CanvasGroupCache[i].ignoreParentGroups)
                        return true;
                }
                t = t.parent;
            }
            return true;
        }

        public bool Interactable
        {
            get => _interactable;
            set
            {
                if (_interactable != value)
                {
                    _interactable = value;
                    invokeInteractableChangedEvent();
                }
            }
        }

        public bool isInteractable()
        {
            return _interactable && _groupsAllowInteraction && _disableInteratableFromListeners.Count == 0;
        }

        public void setInteratableFromListener(INButtonListener listener, bool interactable)
        {
            if (interactable)
            {
                _disableInteratableFromListeners.removeSwapBack(listener);
            }
            else
            {
                _disableInteratableFromListeners.addIfNotPresent(listener);
            }
        }

        protected override void Awake()
        {
            setup();
        }

        private void setup()
        {
            if (!_isSetup)
            {
                if (_betweenClickIntervalTime < 0)
                {
                    _betweenClickIntervalTime = 0;
                }
                _isSetup = true;
            }
        }
        private async void invokeEvent(UnityEvent unityEvent)
        {
            if (_delayInvokeTime > 0)
            {
                await new NWaitSecond(_delayInvokeTime);
            }
            try
            {
                unityEvent?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
        private void invokeInteractableChangedEvent()
        {
            var interactable = isInteractable();
            if (interactable)
            {
                onEnableInteractableEvent?.Invoke();
            }
            else
            {
                onDisableInteractableEvent?.Invoke();
            }
            if (!_isSetup)
            {
                using var listeners = gameObject.getComponentsInChildren_CachedList<INButtonListener>();
                foreach (var listener in listeners)
                {
                    try
                    {
                        listener.onInteractableChanged(interactable);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
            else
            {
                foreach (var listener in _listeners)
                {
                    try
                    {
                        listener.onInteractableChanged(interactable);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
        }

        public void addNButtonListener(INButtonListener listener)
        {
            _listeners.addIfNotPresent(listener);
        }
        public void removeNButtonListener(INButtonListener listener)
        {
            _listeners.Remove(listener);
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            if (!isInteractable() || !_isSetup)
            {
                return;
            }

            foreach (var listener in _listeners)
            {
                try
                {
                    listener?.onButtonDown();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            invokeEvent(onButtonDownEvent);
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            if (!isInteractable() || !_isSetup)
            {
                return;
            }

            foreach (var listener in _listeners)
            {
                try
                {
                    listener.onButtonUp();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            invokeEvent(onButtonUpEvent);
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!isInteractable() || !_isSetup)
            {
                return;
            }

            foreach (var listener in _listeners)
            {
                try
                {
                    listener.onButtonEnter();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            invokeEvent(onButtonEnterEvent);
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isInteractable() || !_isSetup)
            {
                return;
            }

            foreach (var listener in _listeners)
            {
                try
                {
                    listener.onButtonExit();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            invokeEvent(onButtonExitEvent);
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            if (!isInteractable() || !_isSetup)
            {
                return;
            }

            var currentTime = Time.time;
            if (currentTime < _nextClickableTime)
            {
                return;
            }
            _nextClickableTime = currentTime + _betweenClickIntervalTime;

            foreach (var listener in _listeners)
            {
                try
                {
                    listener.onButtonClick();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            invokeEvent(onButtonClickEvent);
        }
        public void setInteractableWithoutNotify(bool isInteractable)
        {
            _interactable = isInteractable;
        }
    }
}
