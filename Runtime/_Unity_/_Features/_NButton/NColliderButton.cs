using System;
using UnityEngine;
using UnityEngine.Events;

namespace Nextension
{
    public class NColliderButton : MonoBehaviour, INButton
    {
        [SerializeField] private float _betweenClickIntervalTime = 0.2f;
        [SerializeField] private float _delayInvokeTime;
        [SerializeField] private bool _interactable = true;
        [SerializeField] private bool _interactableWhenPointerOverUI;

        public UnityEvent onButtonDownEvent = new UnityEvent();
        public UnityEvent onButtonUpEvent = new UnityEvent();
        public UnityEvent onButtonClickEvent = new UnityEvent();
        public UnityEvent onButtonEnterEvent = new UnityEvent();
        public UnityEvent onButtonExitEvent = new UnityEvent();
        public UnityEvent onEnableInteractableEvent = new UnityEvent();
        public UnityEvent onDisableInteractableEvent = new UnityEvent();

        private readonly NList<INButtonListener> _listeners = new NList<INButtonListener>();
        private readonly NList<INButtonListener> _disableInteratableFromListeners = new NList<INButtonListener>();

        protected float _nextClickableTime;
        protected bool _isSetup;
        protected bool _isDown;
        protected float _downTime;


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
            if (!_interactableWhenPointerOverUI && EventSystemUtil.isOverUI()) return false;
            if (_disableInteratableFromListeners.Count > 0) return false;
            return true;
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
        protected void Awake()
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
        private void invokeEvent(UnityEvent unityEvent)
        {
            __invokeEvent(unityEvent).forget();
        }
        private async NTaskVoid __invokeEvent(UnityEvent unityEvent)
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
                NDebug.LogException(ex);
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
                using var listeners = gameObject.getComponentsInChildren_CachedList<INButtonListener>();
                foreach (var listener in listeners)
                {
                    try
                    {
                        listener.onInteractableChanged(_interactable);
                    }
                    catch (Exception e)
                    {
                        NDebug.LogException(e);
                    }
                }
            }
            else
            {
                foreach (var listener in _listeners)
                {
                    try
                    {
                        listener.onInteractableChanged(_interactable);
                    }
                    catch (Exception e)
                    {
                        NDebug.LogException(e);
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
                    listener.onButtonClick();
                }
                catch (Exception e)
                {
                    NDebug.LogException(e);
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
            _downTime = Time.time;

            foreach (var listener in _listeners)
            {
                try
                {
                    listener.onButtonDown();
                }
                catch (Exception e)
                {
                    NDebug.LogException(e);
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
                    listener.onButtonUp();
                }
                catch (Exception e)
                {
                    NDebug.LogException(e);
                }
            }
            invokeEvent(onButtonUpEvent);

            if (_isDown)
            {
                if (Time.time - _downTime < 0.2f)
                {
                    invokeClickEvent();
                }
                _isDown = false;
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
                    listener.onButtonEnter();
                }
                catch (Exception e)
                {
                    NDebug.LogException(e);
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
                    listener.onButtonExit();
                }
                catch (Exception e)
                {
                    NDebug.LogException(e);
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

