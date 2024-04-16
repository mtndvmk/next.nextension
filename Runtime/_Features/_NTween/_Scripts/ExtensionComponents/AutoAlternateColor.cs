using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace Nextension.Tween
{
    [DisallowMultipleComponent]
    public sealed class AutoAlternateColor : AbsAutoAlternate<Color>
    {
        public enum AutoAlternateColorType
        {
            Graphic,
            SpriteRenderer,
            Material
        }
        [SerializeField] private AutoAlternateColorType _autoAlternateColorType;
        [SerializeField] private Graphic _graphic;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Material _material;

        protected override void setValue(Color value)
        {
            switch (_autoAlternateColorType)
            {
                case AutoAlternateColorType.Graphic:
                    _graphic.color = value;
                    break;
                case AutoAlternateColorType.SpriteRenderer:
                    _spriteRenderer.color = value;
                    break;
                case AutoAlternateColorType.Material:
                    _material.color = value;
                    break;
                default:
                    throw new NotSupportedException();
            }
        }
        private void setValue(float4 value)
        {
            setValue(value.toColor());
        }
        protected override Color getCurrentValue()
        {
            return _autoAlternateColorType switch
            {
                AutoAlternateColorType.Graphic => _graphic.color,
                AutoAlternateColorType.SpriteRenderer => _spriteRenderer.color,
                AutoAlternateColorType.Material => _material.color,
                _ => throw new NotSupportedException()
            };
        }
        protected override NTweener onFromTo()
        {
            return NTween.fromTo(getCurrentValue().toFloat4(), _toValue.toFloat4(), setValue, _timePerHalfCycle);
        }
        protected override NTweener onToFrom()
        {
            return NTween.fromTo(getCurrentValue().toFloat4(), _fromValue.toFloat4(), setValue, _timePerHalfCycle);
        }
    }
}
