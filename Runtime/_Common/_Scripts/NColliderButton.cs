using Nextension.UI;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Nextension
{
    public class NColliderButton : MonoBehaviour
    {
        [SerializeField] private float _betweenClickIntervalTime = 0.2f;
        [SerializeField] private float _delayInvokeTime;
        [SerializeField] private bool _interactable = true;
        [SerializeField] private bool _includeListenersInChildren;
        [SerializeField] private bool _interactableWhenPointerOverUI;

        public UnityEvent onButtonDownEvent = new();
        public UnityEvent onButtonUpEvent = new();
        public UnityEvent onButtonClickEvent = new();
        public UnityEvent onButtonEnterEvent = new();
        public UnityEvent onButtonExitEvent = new();
        public UnityEvent onEnableInteractableEvent = new();
        public UnityEvent onDisableInteractableEvent = new();

        private readonly NArray<INButtonListener> _listeners = new();

        protected float _nextClickableTime;
        protected bool _isSetup;
        protected bool _isDown;


#if UNITY_EDITOR
        private bool? _editorInteractable;
        protected void OnValidate()
        {
            if (_editorInteractable != _interactable)
            {
                _editorInteractable = _interactable;
                invokeInteractableChangedEvent();
            }
        }
#endif

        public bool Interactable
        {
            get => _interactable;
            set
            {
                if (_interactable != value)
                {
                    _isDown = false;
                    _interactable = value;
                    invokeInteractableChangedEvent();
                }
            }
        }

        public bool isInteractable()
        {
            if (!_interactable) return false;
            if (!_interactableWhenPointerOverUI && PointerManager.isOverUI()) return false;
            return true;
        }

        protected void Awake()
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
            if (_interactable)
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
                        listener?.onInteractableChanged(_interactable);
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
                            listener?.onInteractableChanged(_interactable);
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e);
                        }
                    }
                }
            }
        }
        private void invokeClickEvent()
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
                    listener?.onButtonClick();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            invokeEvent(onButtonClickEvent);
        }

        public void addNButtonListener(INButtonListener listener)
        {
            _listeners.addIfNotPresent(listener);
        }
        public void removeNButtonListener(INButtonListener listener)
        {
            _listeners.Remove(listener);
        }

        public void OnMouseDown()
        {
            if (!isInteractable() || !_isSetup)
            {
                return;
            }
            _isDown = true;

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
        public void OnMouseUp()
        {
            if (!isInteractable() || !_isSetup)
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

            if (_isDown)
            {
                _isDown = false;
                invokeClickEvent();
            }
        }
        public void OnMouseEnter()
        {
            if (!isInteractable() || !_isSetup)
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
        public void OnMouseExit()
        {
            if (!isInteractable() || !_isSetup)
            {
                return;
            }

            _isDown = false;

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
        
        public void setInteractableWithoutNotify(bool isInteractable)
        {
            _interactable = isInteractable;
        }
    }
}
