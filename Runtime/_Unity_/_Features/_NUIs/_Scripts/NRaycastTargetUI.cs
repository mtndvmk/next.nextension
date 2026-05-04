using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Nextension
{
    [DisallowMultipleComponent, RequireComponent(typeof(CanvasRenderer))]
    public class NRaycastTargetUI : Graphic, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
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
    }
}
