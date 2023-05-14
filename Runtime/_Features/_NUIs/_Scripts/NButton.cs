using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Nextension.UI
{
    [DisallowMultipleComponent]
    public class NButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] protected float _betweenClickIntervalTime = 0.2f;
        [SerializeField] private bool _isInvokeEventOnEndOfEffect = true;
        [SerializeField] private bool _isInteractable = true;
        [SerializeField, HideInInspector] private AbsNButtonEffect _buttonEffect;

        public UnityEvent onButtonDownEvent = new UnityEvent();
        public UnityEvent onButtonUpEvent = new UnityEvent();
        public UnityEvent onButtonClickEvent = new UnityEvent();
        public UnityEvent onButtonEnterEvent = new UnityEvent();
        public UnityEvent onButtonExitEvent = new UnityEvent();

        protected float _lastClickTime;

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

        private void OnValidate()
        {
            if (!_buttonEffect)
            {
                _buttonEffect = GetComponent<AbsNButtonEffect>();
            }
        }
        private void Awake()
        {
            if (!_buttonEffect)
            {
                _buttonEffect = GetComponent<AbsNButtonEffect>();
            }
        }
        private async void invokeEvent(UnityEvent unityEvent)
        {
            if (_buttonEffect && _isInvokeEventOnEndOfEffect)
            {
                await new NWaitSecond(_buttonEffect.AnimationTime);
            }
            unityEvent?.Invoke();
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_isInteractable)
            {
                return;
            }
            if (_buttonEffect)
            {
                _buttonEffect.onButtonDown();
            }
            invokeEvent(onButtonDownEvent);
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_isInteractable)
            {
                return;
            }
            if (_buttonEffect)
            {
                _buttonEffect.onButtonUp();
            }
            invokeEvent(onButtonUpEvent);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_isInteractable)
            {
                return;
            }
            if (_buttonEffect)
            {
                _buttonEffect.onButtonEnter();
            }
            invokeEvent(onButtonEnterEvent);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_isInteractable)
            {
                return;
            }
            if (_buttonEffect)
            {
                _buttonEffect.onButtonExit();
            }
            invokeEvent(onButtonExitEvent);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_isInteractable)
            {
                return;
            }
            if (Time.time - _lastClickTime < _betweenClickIntervalTime)
            {
                return;
            }
            _lastClickTime = Time.time;
            if (_buttonEffect)
            {
                _buttonEffect.onButtonClick();
            }
            invokeEvent(onButtonClickEvent);
        }
    }
}
