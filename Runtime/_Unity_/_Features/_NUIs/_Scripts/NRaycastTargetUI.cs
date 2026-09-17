using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Nextension
{
    [DisallowMultipleComponent, RequireComponent(typeof(CanvasRenderer))]
    public class NRaycastTargetUI : Graphic, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IScrollHandler
    {
        public override void SetAllDirty() { }
        public override void Rebuild(CanvasUpdate update) { }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }

        public void OnPointerClick(PointerEventData eventData)
        {

        }
        public void OnPointerDown(PointerEventData eventData)
        {

        }
        public void OnPointerEnter(PointerEventData eventData)
        {

        }
        public void OnPointerExit(PointerEventData eventData)
        {

        }
        public void OnPointerUp(PointerEventData eventData)
        {

        }
        public void OnInitializePotentialDrag(PointerEventData eventData)
        {

        }
        public void OnBeginDrag(PointerEventData eventData)
        {

        }
        public void OnEndDrag(PointerEventData eventData)
        {

        }
        public void OnDrag(PointerEventData eventData)
        {

        }
        public void OnScroll(PointerEventData eventData)
        {

        }
    }
}
