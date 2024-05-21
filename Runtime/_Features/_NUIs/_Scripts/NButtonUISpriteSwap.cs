using UnityEngine;
using UnityEngine.UI;

namespace Nextension.UI
{
    [DisallowMultipleComponent]
    public class NButtonUISpriteSwap : MonoBehaviour, INButtonListener
    {
        [SerializeField] NButton _nButton;
        [SerializeField] private Image _target;
        [SerializeField] private Sprite _normalSprite;
        [SerializeField] private Sprite _enterSprite;
        [SerializeField] private Sprite _downSprite;
        [SerializeField] private Sprite _disableSprite;

        private void OnValidate()
        {
            if (_target.isNull())
            {
                _target = GetComponent<Image>();
            }
            if (_nButton.isNull())
            {
                _nButton = GetComponent<NButton>();
            }
            OnEnable();
        }

        private void OnEnable()
        {
            if (_nButton && _target)
            {
                if (_nButton.isInteractable())
                {
                    _target.overrideSprite = _normalSprite;
                }
                else
                {
                    _target.overrideSprite = _disableSprite;
                }
            }
        }

        void INButtonListener.onButtonUp()
        {
            if (!enabled) return;
            if (_target == null) return;
            changeSprite(_enterSprite);
        }
        void INButtonListener.onButtonEnter()
        {
            if (!enabled) return;
            if (_target == null) return;
            changeSprite(_enterSprite);
        }
        void INButtonListener.onButtonExit()
        {
            if (!enabled) return;
            if (_target == null) return;
            changeSprite(_normalSprite);
        }
        void INButtonListener.onButtonDown()
        {
            if (!enabled) return;
            if (_target == null) return;
            changeSprite(_downSprite);
        }
        void INButtonListener.onInteractableChanged(bool isInteractable)
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
        private void OnDestroy()
        {
            if (_nButton)
            {
                _nButton.removeNButtonListener(this);
            }
        }
    }
}
