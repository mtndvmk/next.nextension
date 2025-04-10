using Nextension.Tween;
using UnityEngine;

namespace Nextension
{
    [DisallowMultipleComponent, ExecuteAlways]
    public class NButtonUIColorTint : AbsNButtonEffect
    {
        [SerializeField] private CanvasRenderer _target;
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _enterColor = new Color32(0xF5, 0xF5, 0xF5, 0xFF);
        [SerializeField] private Color _downColor = new Color32(0xC8, 0xC8, 0xC8, 0xFF);
        [SerializeField] private Color _disableColor = new Color32(0xC8, 0xC8, 0xC8, 0x80);
        [SerializeField] private float _duration = 0.1f;

        private NTweener _colorTweener;

        private void Reset()
        {
            _target = GetComponent<CanvasRenderer>();
            OnEnable();
        }
        private void OnValidate()
        {
            OnEnable();
        }

        private void OnEnable()
        {
            if (Button != null && _target)
            {
                if (_button.isInteractable())
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
                _target.SetColor(Color.white);
            }
        }

        public override void onButtonUp()
        {
            if (!enabled) return;
            if (_target == null) return;
            changeColor(_enterColor);
        }
        public override void onButtonEnter()
        {
            if (!enabled) return;
            if (_target == null) return;
            changeColor(_enterColor);
        }
        public override void onButtonExit()
        {
            if (!enabled) return;
            if (_target == null) return;
            changeColor(_normalColor);
        }
        public override void onButtonDown()
        {
            if (!enabled) return;
            if (_target == null) return;
            changeColor(_downColor);
        }
        public override void onInteractableChanged(bool isInteractable)
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
                _colorTweener = NTween.fromTo(_target.GetColor(), color, _duration, resultColor => _target.SetColor(resultColor));
                _colorTweener.setCancelControlKey(_target);
            }
        }
    }
}
