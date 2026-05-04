using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Nextension
{
    [DisallowMultipleComponent]
    public class NHandleEventUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public UnityEvent onClick = new UnityEvent();
        public UnityEvent onEnter = new UnityEvent();
        public UnityEvent onExit = new UnityEvent();
        public UnityEvent onDown = new UnityEvent();
        public UnityEvent onUp = new UnityEvent();
        public void OnPointerClick(PointerEventData eventData)
        {
            onClick?.Invoke();
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            onDown?.Invoke();
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            onEnter?.Invoke();
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            onExit?.Invoke();
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            onUp?.Invoke();
        }
    }
}
