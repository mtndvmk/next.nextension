using UnityEngine;
using UnityEngine.UI;

namespace Nextension
{
    [DisallowMultipleComponent, ExecuteAlways]
    public class NButtonUISpriteSwap : AbsNButtonEffect
    {
        [SerializeField] private Image _target;
        [SerializeField] private Sprite _normalSprite;
        [SerializeField] private Sprite _enterSprite;
        [SerializeField] private Sprite _downSprite;
        [SerializeField] private Sprite _disableSprite;

        private enum SpriteState : byte
        {
            Normal,
            Enter,
            Down,
            Disable
        }

        private SpriteState _currentState;

        private void Reset()
        {
            _target = GetComponent<Image>();
            OnEnable();
        }
        private void OnValidate()
        {
            refreshSprite();
        }
        private void OnEnable()
        {
            if (Button != null)
            {
                onInteractableChanged(_button.isInteractable());
            }
        }
        private void OnDisable()
        {
            _target.overrideSprite = default;
        }
        public override void onButtonUp()
        {
            _currentState = SpriteState.Enter;
            changeSprite(_enterSprite);
        }
        public override void onButtonEnter()
        {
            _currentState = SpriteState.Enter;
            changeSprite(_enterSprite);
        }
        public override void onButtonExit()
        {
            _currentState = SpriteState.Normal;
            changeSprite(_normalSprite);
        }
        public override void onButtonDown()
        {
            _currentState = SpriteState.Down;
            changeSprite(_downSprite);
        }
        public override void onInteractableChanged(bool isInteractable)
        {
            if (isInteractable)
            {
                _currentState = SpriteState.Normal;
                changeSprite(_normalSprite);
            }
            else
            {
                _currentState = SpriteState.Disable;
                changeSprite(_disableSprite);
            }
        }
        private void refreshSprite()
        {
            switch (_currentState)
            {
                case SpriteState.Normal:
                    changeSprite(_normalSprite);
                    break;
                case SpriteState.Enter:
                    changeSprite(_enterSprite);
                    break;
                case SpriteState.Down:
                    changeSprite(_downSprite);
                    break;
                case SpriteState.Disable:
                    changeSprite(_disableSprite);
                    break;
            }
        }
        private void changeSprite(Sprite sprite)
        {
            if (!enabled) return;
            if (_target == null) return;
            _target.overrideSprite = sprite;
        }
    }
}
