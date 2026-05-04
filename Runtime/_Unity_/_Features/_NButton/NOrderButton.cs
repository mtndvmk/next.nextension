using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Nextension
{
    [DisallowMultipleComponent, RequireComponent(typeof(NButton))]
    public class NOrderButton : AbsNButtonEffect
    {
        [SerializeField, NSlider(0, nameof(TotalSprites))] private uint _order;
        [SerializeField] private Image _targetImage;
        [SerializeField] private List<Sprite> _orderSprites;
        [Space, NGroup("Event")] public UnityEvent<uint> onOrderChanged;

        public int TotalSprites => _orderSprites.Count;

        private void OnValidate()
        {
            __onValueChanged();
        }
        private void Start()
        {
            __onValueChanged();
        }

        public uint Order
        {
            get => _order;
            set
            {
                if (_order != value)
                {
                    if (value >= _orderSprites.Count)
                    {
                        NDebug.LogWarning("Order is higher than sprites count.");
                        return;
                    }
                    _order = value;
                    __onValueChanged();
                }
            }
        }

        public void setOrderWithoutNotify(uint order)
        {
            _order = order;
            __updateSprite();
        }

        public void addSprite(Sprite sprite)
        {
            _orderSprites.Add(sprite);
        }

        public void setSprite(int index, Sprite sprite)
        {
            _orderSprites[index] = sprite;
        }

        public Sprite getSprite(int index)
        {
            return _orderSprites[index];
        }

        public void removeSpriteAt(int index)
        {
            _orderSprites.RemoveAt(index);
        }

        public void clearSprites()
        {
            _orderSprites.Clear();
        }

        private void __onValueChanged()
        {
            onOrderChanged?.Invoke(_order);
            __updateSprite();
        }

        private void __updateSprite()
        {
            if (TotalSprites <= _order) return;
            if (_targetImage)
            {
                _targetImage.sprite = _orderSprites[(int)_order];
            }
        }

        public override void onButtonClick()
        {
            var nextOrder = _order + 1;
            if (nextOrder >= _orderSprites.Count) nextOrder = 0;
            Order = nextOrder;
        }
    }
}

