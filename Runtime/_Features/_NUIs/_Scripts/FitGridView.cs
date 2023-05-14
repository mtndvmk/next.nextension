using UnityEngine;
using UnityEngine.UI;

namespace Nextension.UI
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(GridLayoutGroup))]
    public class FitGridView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GridLayoutGroup m_GridLayoutGroup;
        [SerializeField] private RectTransform m_RectView;
        [Header("Config")]
        [SerializeField] private RectOffset m_Padding = new RectOffset();
        [SerializeField] private Vector2 m_FitSizeFactor = Vector2.one;
        [SerializeField] private Vector2 m_SpacingFactor = Vector2.zero;
        [SerializeField] private bool m_IsScaleSpacingWithFitSize = false;
        [SerializeField] private bool m_IsUpdateOnScreenSizeChanged = true;
        [SerializeField] private bool m_IsForceUpdateOnEditor;


        private void OnValidate()
        {
            if (m_IsForceUpdateOnEditor)
            {
                updateGrid();
            }
        }
        private void Start()
        {
            ScreenResizeDetector.Instance.onScreenResizeEvent += (size) =>
            {
                if (m_IsUpdateOnScreenSizeChanged)
                {
                    updateGrid();
                }
            };
        }
#if UNITY_EDITOR
        private void Update()
        {
            if (m_IsForceUpdateOnEditor && !Application.isPlaying)
            {
                updateGrid();
            }
        }
#endif
        [ContextMenu("Update Grid Value")]
        private void updateGrid()
        {
            if (!m_GridLayoutGroup)
            {
                m_GridLayoutGroup = GetComponent<GridLayoutGroup>();
            }
            if (!m_RectView)
            {
                m_RectView = GetComponent<RectTransform>();
            }
            m_GridLayoutGroup.padding = m_Padding;
            var size = m_RectView.rect.size - new Vector2(m_Padding.left + m_Padding.right, m_Padding.top + m_Padding.bottom);
            var newSize = m_GridLayoutGroup.cellSize;
            if (!m_IsScaleSpacingWithFitSize)
            {
                m_GridLayoutGroup.spacing = m_SpacingFactor;
            }
            else
            {
                if (m_FitSizeFactor.x > 0)
                {
                    newSize.x = size.x / (m_FitSizeFactor.x + m_SpacingFactor.x);
                }
                if (m_FitSizeFactor.y > 0)
                {
                    newSize.y = size.y / (m_FitSizeFactor.y + m_SpacingFactor.x);
                }
                m_GridLayoutGroup.spacing = new Vector2(m_SpacingFactor.x * newSize.x, m_SpacingFactor.y * newSize.y);
            }
            if (m_FitSizeFactor.x > 0)
            {
                newSize.x = (size.x - (m_FitSizeFactor.x - 1) * m_SpacingFactor.x) / m_FitSizeFactor.x;
            }
            if (m_FitSizeFactor.y > 0)
            {
                newSize.y = (size.y - (m_FitSizeFactor.y - 1) * m_SpacingFactor.y) / m_FitSizeFactor.y;
            }
            m_GridLayoutGroup.cellSize = newSize;
            Canvas.ForceUpdateCanvases();
        }
    }
}
