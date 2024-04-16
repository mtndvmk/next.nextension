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
        [SerializeField] private float _delayInvokeTime;
        [SerializeField] private bool _isInteractable = true;
        [SerializeField] private bool _findListenerInChildren;

        public UnityEvent onButtonDownEvent = new();
        public UnityEvent onButtonUpEvent = new();
        public UnityEvent onButtonClickEvent = new();
        public UnityEvent onButtonEnterEvent = new();
        public UnityEvent onButtonExitEvent = new();
        public UnityEvent onInteractableChangedEvent = new();

        [NonSerialized] private List<INButtonListener> _listeners;
        protected float _lastClickTime;
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
                if (_findListenerInChildren)
                {
                    listeners = GetComponentsInChildren<INButtonListener>(true);
                }
                else
                {
                    listeners = GetComponents<INButtonListener>();
                }
                if (listeners.Length > 0)
                {
                    if (_listeners == null)
                    {
                        _listeners = new(listeners);
                    }
                    else
                    {
                        _listeners.AddRange(listeners);
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
        private void invokeInteractableChangedEvent()
        {
            onInteractableChangedEvent?.Invoke();
            if (!_isSetup)
            {
                INButtonListener[] listeners;
                if (_findListenerInChildren)
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
            (_listeners ??= new(1)).addIfNotPresent(listener);
        }
        public void removeNButtonListener(INButtonListener listener)
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
        public void setInteractableWithoutNotify(bool isInteractable)
        {
            _isInteractable = isInteractable;
        }
    }
}
