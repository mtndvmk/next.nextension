using UnityEngine;
using UnityEngine.Events;

namespace Nextension
{
    [DisallowMultipleComponent]
    public class NToggleButton : AbsNButtonEffect
    {
        [SerializeField] private bool _value;

        public UnityEvent<bool> onValueChanged;
        public UnityEvent onValueChanged_True;
        public UnityEvent onValueChanged_False;

        private void OnValidate()
        {
            inner_onValueChanged();
        }
        private void Start()
        {
            inner_onValueChanged();
        }

        public bool Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    inner_onValueChanged();
                }
            }
        }

        public void setValueWithoutNotify(bool value)
        {
            _value = value;
        }

        private void inner_onValueChanged()
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
        }

        public override void onButtonClick()
        {
            Value = !_value;
        }
    }
}
