using UnityEngine;

namespace Nextension.UI
{
    [DisallowMultipleComponent]
    public class CanvasReference : MonoBehaviour
    {
        [SerializeField] private Canvas m_CanvasRef;
        public Canvas Canvas => m_CanvasRef;

        private void OnValidate()
        {
            findCanvasRef();
        }
        private void Awake()
        {
            findCanvasRef();
        }
        [ContextMenu("Find canvas")]
        public void findCanvasRef()
        {
            if (!m_CanvasRef)
            {
                m_CanvasRef = GetComponentInParent<Canvas>(true);
            }
        }
    }
}