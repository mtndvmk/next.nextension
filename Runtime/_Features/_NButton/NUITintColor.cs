using UnityEngine;

namespace Nextension
{
    [ExecuteAlways]
    public class NUITintColor : MonoBehaviour
    {
        [SerializeField] private Color _color;
        private CanvasRenderer _canvasRenderer;

        private void OnValidate()
        {
            if (enabled)
            {
                updateColor(_color);
            }
            else
            {
                updateColor(Color.white);
            }
        }
        private void OnEnable()
        {
            updateColor(_color);
        }
        private void OnDisable()
        {
            updateColor(Color.white);
        }

        private void updateColor(Color color)
        {
            _canvasRenderer = _canvasRenderer != null ? _canvasRenderer : GetComponent<CanvasRenderer>();
            if (_canvasRenderer)
            {
                _canvasRenderer.SetColor(color);
            }
        }
    }
}
