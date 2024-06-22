using Nextension.Tween;
using UnityEngine;

namespace Nextension.UI
{
    [DisallowMultipleComponent]
    public class NButtonUIColorTint : MonoBehaviour, INButtonListener
    {
        [SerializeField] private CanvasRenderer _target;
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _enterColor = new Color32(0xF5, 0xF5, 0xF5, 0xFF);
        [SerializeField] private Color _downColor = new Color32(0xC8, 0xC8, 0xC8, 0xFF);
        [SerializeField] private Color _disableColor = new Color32(0xC8, 0xC8, 0xC8, 0x80);
        [SerializeField] private float _duration = 0.1f;

        private NTweener _colorTweener;
        private NButton _nButton;

        private void Reset()
        {
            _target = GetComponent<CanvasRenderer>();
            _nButton = GetComponentInParent<NButton>();
            OnEnable();
        }
        private void OnValidate()
        {
            OnEnable();
        }
        private void Awake()
        {
            if (_nButton.isNull())
            {
                _nButton = GetComponentInParent<NButton>();
            }
        }

        private void OnEnable()
        {
            if (_nButton && _target)
            {
                if (_nButton.isInteractable())
                {
                    _target.SetColor(_normalColor);
                }
                else
                {
                    _target.SetColor(_disableColor);
                }
            }
        }
        private void OnDisable()
        {
            if (_target)
            {
                _target.SetColor(_normalColor);
            }
        }
        private void OnDestroy()
        {
            if (_nButton)
            {
                _nButton.removeNButtonListener(this);
            }
        }

        void INButtonListener.onButtonUp()
        {
            if (!enabled) return;
            if (_target == null) return;
            changeColor(_enterColor);
        }
        void INButtonListener.onButtonEnter()
        {
            if (!enabled) return;
            if (_target == null) return;
            changeColor(_enterColor);
        }
        void INButtonListener.onButtonExit()
        {
            if (!enabled) return;
            if (_target == null) return;
            changeColor(_normalColor);
        }
        void INButtonListener.onButtonDown()
        {
            if (!enabled) return;
            if (_target == null) return;
            changeColor(_downColor);
        }
        void INButtonListener.onInteractableChanged(bool isInteractable)
        {
            if (!enabled) return;
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
#if UNITY_EDITOR
            if (!NStartRunner.IsPlaying)
            {
                _target.SetColor(color);
                return;
            }
#endif
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
                _colorTweener.setCancelControlKey(_target);
            }
        }
    }
}
