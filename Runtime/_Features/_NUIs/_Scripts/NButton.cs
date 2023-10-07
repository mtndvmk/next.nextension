using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Nextension.UI
{
    [DisallowMultipleComponent]
    public class NButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private float _betweenClickIntervalTime = 0.2f;
        [SerializeField] private float _delayInvokeTime = 0.1f;
        [SerializeField] private bool _isInteractable = true;

        public UnityEvent onButtonDownEvent = new();
        public UnityEvent onButtonUpEvent = new();
        public UnityEvent onButtonClickEvent = new();
        public UnityEvent onButtonEnterEvent = new();
        public UnityEvent onButtonExitEvent = new();

        private HashSet<INButtonListener> _listeners;
        protected float _lastClickTime;
        protected bool _isSetup;

        public bool IsInteractable
        {
            get => _isInteractable;
            set
            {
                if (_isInteractable != value)
                {
                    _isInteractable = value;
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
                var listeners = GetComponents<INButtonListener>();
                if (listeners.Length > 0)
                {
                    if (_listeners == null)
                    {
                        _listeners = new(listeners);
                    }
                    else
                    {
                        _listeners.UnionWith(listeners);
                    }
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

        public void addListener(INButtonListener listener)
        {
            (_listeners ??= new(1)).Add(listener);
        }
        public void removeListener(INButtonListener listener)
        {
            _listeners?.Remove(listener);
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_isInteractable || !_isSetup)
            {
                return;
            }

            if (_listeners != null)
            {
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
            }
            invokeEvent(onButtonDownEvent);
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_isInteractable || !_isSetup)
            {
                return;
            }

            if (_listeners != null)
            {
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
            }
            invokeEvent(onButtonUpEvent);
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_isInteractable || !_isSetup)
            {
                return;
            }

            if (_listeners != null) 
            {
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
            }
            invokeEvent(onButtonEnterEvent);
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_isInteractable || !_isSetup)
            {
                return;
            }

            if (_listeners != null)
            {
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
            }
            invokeEvent(onButtonExitEvent);
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_isInteractable || !_isSetup)
            {
                return;
            }
            if (Time.time - _lastClickTime < _betweenClickIntervalTime)
            {
                return;
            }
            _lastClickTime = Time.time;

            if (_listeners != null)
            {
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
            }
            invokeEvent(onButtonClickEvent);
        }
    }
}
