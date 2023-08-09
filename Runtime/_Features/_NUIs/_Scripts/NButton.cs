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
        [SerializeField] private float _delayInvokeTime = 0.2f;
        [SerializeField] private bool _isInteractable = true;

        public UnityEvent onButtonDownEvent = new UnityEvent();
        public UnityEvent onButtonUpEvent = new UnityEvent();
        public UnityEvent onButtonClickEvent = new UnityEvent();
        public UnityEvent onButtonEnterEvent = new UnityEvent();
        public UnityEvent onButtonExitEvent = new UnityEvent();

        private List<INButtonListener> _listeners;
        protected float _lastClickTime;
        protected bool _isAwaked;

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
            if (!_isAwaked)
            {
                if (_listeners == null)
                {
                    _listeners = new List<INButtonListener>();
                }
                _listeners.AddRange(GetComponents<INButtonListener>());
                _isAwaked = true;
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

        public void addListener(INButtonListener listeners)
        {
            if (_listeners == null)
            {
                _listeners = new List<INButtonListener>();
            }
            _listeners.add(listeners);
        }      
        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_isInteractable || !_isAwaked)
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
            if (!_isInteractable || !_isAwaked)
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
            if (!_isInteractable || !_isAwaked)
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
            if (!_isInteractable || !_isAwaked)
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
            if (!_isInteractable || !_isAwaked)
            {
                return;
            }
            if (Time.time - _lastClickTime < _betweenClickIntervalTime)
            {
                return;
            }
            _lastClickTime = Time.time;

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
    }
}
