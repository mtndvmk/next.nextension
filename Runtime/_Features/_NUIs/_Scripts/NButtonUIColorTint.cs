using Nextension.Tween;
using UnityEngine;
using UnityEngine.UI;

namespace Nextension.UI
{
    [DisallowMultipleComponent]
    public class NButtonUIColorTint : MonoBehaviour, INButtonListener
    {
        [SerializeField] NButton _nButton;
        [SerializeField] private CanvasRenderer _target;
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _enterColor = new Color32(0xF5, 0xF5, 0xF5, 0xFF);
        [SerializeField] private Color _downColor = new Color32(0xC8, 0xC8, 0xC8, 0xFF);
        [SerializeField] private Color _disableColor = new Color32(0xC8, 0xC8, 0xC8, 0x80);
        [SerializeField] private float _duration = 0.1f;

        private NTweener _colorTweener;

        private void OnValidate()
        {
            _target ??= GetComponent<CanvasRenderer>();
            _nButton ??= GetComponent<NButton>();

            OnEnable();
        }

        private void OnEnable()
        {
            if (_nButton && _target)
            {
                if (_nButton.IsInteractable)
                {
                    _target.SetColor(_normalColor);
                }
                else
                {
                    _target.SetColor(_disableColor);
                }
            }
        }

        void INButtonListener.onButtonUp()
        {
            if (_target == null) return;
            changeColor(_enterColor);
        }
        void INButtonListener.onButtonEnter()
        {
            if (_target == null) return;
            changeColor(_enterColor);
        }
        void INButtonListener.onButtonExit()
        {
            if (_target == null) return;
            changeColor(_normalColor);
        }
        void INButtonListener.onButtonDown()
        {
            if (_target == null) return;
            changeColor(_downColor);
        }
        void INButtonListener.onInteractableChanged(bool isInteractable)
        {
            if (_target == null) return;
            if (isInteractable)
            {
                changeColor(_normalColor);
            }
            else
            {
                changeColor(_disableColor);
            }
        }

        private void changeColor(Color color)
        {
            if (_colorTweener != null)
            {
                _colorTweener.cancel();
                _colorTweener = null;
            }
            if (_duration <= 0)
            {
                _target.SetColor(color);
            }
            else
            {
                _colorTweener = NTween.fromTo(_target.GetColor(), color, (resultColor) =>
                {
                    _target.SetColor(resultColor);
                }, _duration);
            }
        }
    }
}
