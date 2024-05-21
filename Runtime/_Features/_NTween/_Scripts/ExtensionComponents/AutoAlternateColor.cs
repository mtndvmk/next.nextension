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
            CanvasRenderer,
            SpriteRenderer,
            Material,
        }
        [SerializeField] private AutoAlternateColorType _autoAlternateColorType;
        [SerializeField] private Graphic _graphic;
        [SerializeField] private CanvasRenderer _canvasRenderer;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Material _material;

        private void Reset()
        {
            switch (_autoAlternateColorType)
            {
                case AutoAlternateColorType.CanvasRenderer:
                    {
                        if (_canvasRenderer.isNull())
                        {
                            _canvasRenderer = GetComponent<CanvasRenderer>();
                            _fromValue = _toValue = Color.white;
                        }
                        break;
                    }
                case AutoAlternateColorType.Graphic:
                    {
                        if (_graphic.isNull())
                        {
                            _graphic = GetComponent<Graphic>();
                            if (_graphic.isNull())
                            {
                                _fromValue = _toValue = Color.white;
                            }
                            else
                            {
                                _fromValue = _toValue = _graphic.color;
                            }
                        }
                        break;
                    }
                case AutoAlternateColorType.SpriteRenderer:
                    {
                        if (_spriteRenderer.isNull())
                        {
                            _spriteRenderer = GetComponent<SpriteRenderer>();
                            if (_spriteRenderer.isNull())
                            {
                                _fromValue = _toValue = Color.white;
                            }
                            else
                            {
                                _fromValue = _toValue = _spriteRenderer.color;
                            }
                        }
                        break;
                    }
                default:
                    {
                        _fromValue = _toValue = Color.white;
                        break;
                    }
            }
        }

        protected override void setValue(Color value)
        {
            switch (_autoAlternateColorType)
            {
                case AutoAlternateColorType.CanvasRenderer:
                    _canvasRenderer.SetColor(value);
                    break;
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
                AutoAlternateColorType.CanvasRenderer => _canvasRenderer.GetColor(),
                AutoAlternateColorType.Graphic => _graphic.color,
                AutoAlternateColorType.SpriteRenderer => _spriteRenderer.color,
                AutoAlternateColorType.Material => _material.color,
                _ => throw new NotSupportedException()
            };
        }
        protected override NRunnableTweener onFromTo()
        {
            return NTween.fromTo(getCurrentValue().toFloat4(), _toValue.toFloat4(), setValue, _timePerHalfCycle);
        }
        protected override NRunnableTweener onToFrom()
        {
            return NTween.fromTo(getCurrentValue().toFloat4(), _fromValue.toFloat4(), setValue, _timePerHalfCycle);
        }
    }
}