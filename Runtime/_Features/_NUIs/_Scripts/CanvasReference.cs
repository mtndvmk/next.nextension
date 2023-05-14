using UnityEngine;

namespace Nextension.UI
{
    [DisallowMultipleComponent]
    public class CanvasReference : MonoBehaviour
    {
        [SerializeField] private Canvas m_CanvasRef;
        public Canvas getCanvas() => m_CanvasRef;

        private void OnValidate()
        {
            if (!m_CanvasRef)
            {
                findMyCanvas();
            }
        }
        private void Awake()
        {
            if (!m_CanvasRef)
            {
                findMyCanvas();
            }
        }
        [ContextMenu("Find my canvas")]
        public void findMyCanvas()
        {
            var p = transform;
            while (!m_CanvasRef)
            {
                if (p)
                {
                    m_CanvasRef = p.GetComponent<Canvas>();
                    if (m_CanvasRef)
                    {
                        break;
                    }
                    else
                    {
                        p = p.parent;
                    }
                }
                else
                {
                    break;
                }
            }
        }
    }
}