using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Nextension.UI
{
    [DisallowMultipleComponent]
    public class NButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private float _betweenClickIntervalTime = 0.2f;
        [SerializeField] private float _delayInvokeTime;
        [SerializeField] private bool _isInteractable = true;
        [SerializeField] private bool _includeListenersInChildren;

        public UnityEvent onButtonDownEvent = new();
        public UnityEvent onButtonUpEvent = new();
        public UnityEvent onButtonClickEvent = new();
        public UnityEvent onButtonEnterEvent = new();
        public UnityEvent onButtonExitEvent = new();
        public UnityEvent onEnableInteractableEvent = new();
        public UnityEvent onDisableInteractableEvent = new();

        private NArray<INButtonListener> _listeners = new();
        protected float _nextClickableTime;
        protected bool _isSetup;

#if UNITY_EDITOR
        private bool? _editorInteractable;
        private void OnValidate()
        {
            if (_editorInteractable != _isInteractable)
            {
                _editorInteractable = _isInteractable;
                invokeInteractableChangedEvent();
            }
        }
#endif

        public bool IsInteractable
        {
            get => _isInteractable;
            set
            {
                if (_isInteractable != value)
                {
                    _isInteractable = value;
                    invokeInteractableChangedEvent();
                }
            }
        }

        private void Awake()
        {
            setup();
        }

        private void setup()
        {
            if (!_isSetup)
            {
                INButtonListener[] listeners;
                if (_includeListenersInChildren)
                {
                    listeners = GetComponentsInChildren<INButtonListener>(true);
                }
                else
                {
                    listeners = GetComponents<INButtonListener>();
                }
                if (listeners.Length > 0)
                {
                    _listeners.AddRange(listeners);
                }
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
            if (_isInteractable)
            {
                onEnableInteractableEvent?.Invoke();
            }
            else
            {
                onDisableInteractableEvent?.Invoke();
            }
            if (!_isSetup)
            {
                INButtonListener[] listeners;
                if (_includeListenersInChildren)
                {
                    listeners = GetComponentsInChildren<INButtonListener>(true);
                }
                else
                {
                    listeners = GetComponents<INButtonListener>();
                }
                foreach (var listener in listeners)
                {
                    try
                    {
                        listener?.onInteractableChanged(_isInteractable);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
            else
            {
                if (_listeners != null)
                {
                    foreach (var listener in _listeners)
                    {
                        try
                        {
                            listener?.onInteractableChanged(_isInteractable);
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e);
                        }
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
            if (!_isInteractable || !_isSetup)
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
            if (!_isInteractable || !_isSetup)
            {
                return;
            }

            foreach (var listener in _listeners)
            {
                try
                {
                    listener?.onButtonUp();
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
            if (!_isInteractable || !_isSetup)
            {
                return;
            }

            foreach (var listener in _listeners)
            {
                try
                {
                    listener?.onButtonEnter();
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
            if (!_isInteractable || !_isSetup)
            {
                return;
            }

            foreach (var listener in _listeners)
            {
                try
                {
                    listener?.onButtonExit();
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
            if (!_isInteractable || !_isSetup)
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
                    listener?.onButtonClick();
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
            _isInteractable = isInteractable;
        }
    }
}
