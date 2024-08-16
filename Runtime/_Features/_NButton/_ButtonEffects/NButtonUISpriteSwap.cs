using UnityEngine;
using UnityEngine.UI;

namespace Nextension
{
    [DisallowMultipleComponent]
    public class NButtonUISpriteSwap : AbsNButtonEffect
    {
        [SerializeField] private Image _target;
        [SerializeField] private Sprite _normalSprite;
        [SerializeField] private Sprite _enterSprite;
        [SerializeField] private Sprite _downSprite;
        [SerializeField] private Sprite _disableSprite;

        private void Reset()
        {
            _target = GetComponent<Image>();
            OnEnable();
        }
        private void OnEnable()
        {
            if (Button != null && _target)
            {
                if (_button.isInteractable())
                {
                    _target.overrideSprite = _normalSprite;
                }
                else
                {
                    _target.overrideSprite = _disableSprite;
                }
            }
        }
        public override void onButtonUp()
        {
            if (!enabled) return;
            if (_target == null) return;
            changeSprite(_enterSprite);
        }
        public override void onButtonEnter()
        {
            if (!enabled) return;
            if (_target == null) return;
            changeSprite(_enterSprite);
        }
        public override void onButtonExit()
        {
            if (!enabled) return;
            if (_target == null) return;
            changeSprite(_normalSprite);
        }
        public override void onButtonDown()
        {
            if (!enabled) return;
            if (_target == null) return;
            changeSprite(_downSprite);
        }
        public override void onInteractableChanged(bool isInteractable)
        {
            if (!enabled) return;
            if (_target == null) return;
            if (isInteractable)
            {
                changeSprite(_normalSprite);
            }
            else
            {
                changeSprite(_disableSprite);
            }
        }

        private void changeSprite(Sprite sprite)
        {
            _target.overrideSprite = sprite;
        }
    }
}
