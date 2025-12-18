using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Nextension
{
    [DisallowMultipleComponent, RequireComponent(typeof(NButton))]
    public class NToggleButton : AbsNButtonEffect
    {
        [SerializeField] private bool _value;
        [SerializeField] private Image _targetImage;
        [NIndent, SerializeField, NShowIf(nameof(_targetImage))] private Sprite _trueSprite;
        [NIndent, SerializeField, NShowIf(nameof(_targetImage))] private Sprite _falseSprite;

        [Space, NGroup("Event")] public UnityEvent<bool> onValueChanged;
        [NGroup("Event")] public UnityEvent onValueChanged_True;
        [NGroup("Event")] public UnityEvent onValueChanged_False;

        private void OnValidate()
        {
            __onValueChanged();
        }
        private void Start()
        {
            __onValueChanged();
        }

        public bool Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    __onValueChanged();
                }
            }
        }

        public void setValueWithoutNotify(bool value)
        {
            _value = value;
            __updateSprite();
        }

        private void __onValueChanged()
        {
            onValueChanged?.Invoke(_value);
            if (_value)
            {
                onValueChanged_True?.Invoke();
            }
            else
            {
                onValueChanged_False?.Invoke();
            }
            __updateSprite();
        }

        private void __updateSprite()
        {
            if (_targetImage)
            {
                _targetImage.sprite = _value ? _trueSprite : _falseSprite;
            }
        }

        public override void onButtonClick()
        {
            Value = !_value;
        }
    }
}
