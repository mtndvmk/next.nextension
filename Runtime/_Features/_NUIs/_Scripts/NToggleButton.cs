using UnityEngine;
using UnityEngine.Events;

namespace Nextension.UI
{
    [DisallowMultipleComponent, RequireComponent(typeof(NButton))]
    public class NToggleButton : MonoBehaviour, INButtonListener
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

        private NButton _button;
        public NButton Button
        {
            get
            {
                if (_button.isNull())
                {
                    _button = GetComponent<NButton>();
                }
                return _button;
            }
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

        void INButtonListener.onButtonClick()
        {
            Value = !_value;
        }
    }
}
