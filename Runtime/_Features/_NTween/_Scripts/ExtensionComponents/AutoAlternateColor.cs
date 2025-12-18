using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace Nextension.Tween
{
    [DisallowMultipleComponent]
    public sealed class AutoAlternateColor : AbsAutoAlternate<Color>
    {
        public enum AutoAlternateColorType : byte
        {
            None = 0,
            Graphic = 1,
            CanvasRenderer = 2,
            Material = 3,
            SpriteRenderer = 4,
            MeshRenderer = 5
        }
        [SerializeField] private bool _hsvMode = true; 
        [SerializeField, NDisplayName("Target Type")] private AutoAlternateColorType _autoAlternateColorType;
        [SerializeField, NShowIf(nameof(_autoAlternateColorType), AutoAlternateColorType.Graphic)] private Graphic _graphic;
        [SerializeField, NShowIf(nameof(_autoAlternateColorType), AutoAlternateColorType.CanvasRenderer)] private CanvasRenderer _canvasRenderer;
        [SerializeField, NShowIf(nameof(_autoAlternateColorType), AutoAlternateColorType.Material)] private Material _material;
        [SerializeField, NShowIf(nameof(_autoAlternateColorType), AutoAlternateColorType.SpriteRenderer)] private SpriteRenderer _spriteRenderer;
        [SerializeField, NShowIf(nameof(_autoAlternateColorType), AutoAlternateColorType.MeshRenderer)] private Renderer _meshRenderer;
        [SerializeField, NShowIf(nameof(_autoAlternateColorType), AutoAlternateColorType.MeshRenderer)] private bool _useShareMaterial = false;

        private void Reset()
        {
            if (_autoAlternateColorType == AutoAlternateColorType.None && _graphic.isNull())
            {
                _graphic = GetComponent<Graphic>();
                if (!_graphic.isNull())
                {
                    _autoAlternateColorType = AutoAlternateColorType.Graphic;
                }
            }

            if (_autoAlternateColorType == AutoAlternateColorType.None && _canvasRenderer.isNull())
            {
                _canvasRenderer = GetComponent<CanvasRenderer>();
                if (!_canvasRenderer.isNull())
                {
                    _autoAlternateColorType = AutoAlternateColorType.CanvasRenderer;
                }
            }

            if (_autoAlternateColorType == AutoAlternateColorType.None && _spriteRenderer.isNull())
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
                if (!_spriteRenderer.isNull())
                {
                    _autoAlternateColorType = AutoAlternateColorType.SpriteRenderer;
                }
            }

            if (_autoAlternateColorType == AutoAlternateColorType.None && _meshRenderer.isNull())
            {
                _meshRenderer = GetComponent<Renderer>();
                if (!_meshRenderer.isNull())
                {
                    _autoAlternateColorType = AutoAlternateColorType.MeshRenderer;
                }
            }

            switch (_autoAlternateColorType)
            {
                case AutoAlternateColorType.CanvasRenderer:
                    {
                        _canvasRenderer = GetComponent<CanvasRenderer>();
                        _fromValue = _toValue = Color.white;
                        break;
                    }
                case AutoAlternateColorType.Graphic:
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
                        break;
                    }
                case AutoAlternateColorType.SpriteRenderer:
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
                        break;
                    }
                case AutoAlternateColorType.MeshRenderer:
                    {
                        _meshRenderer = GetComponent<Renderer>();
                        if (_meshRenderer.isNull())
                        {
                            _fromValue = _toValue = Color.white;
                        }
                        else
                        {
                            Material material;
                            if (_useShareMaterial)
                            {
                                material = _meshRenderer.sharedMaterial;
                            }
                            else
                            {
                                material = _meshRenderer.material;
                            }
                            if (material.isNull())
                            {
                                _fromValue = _toValue = Color.white;
                            }
                            else
                            {
                                _fromValue = _toValue = material.color;
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
                    if (_canvasRenderer.isNull()) return;
                    _canvasRenderer.SetColor(value);
                    break;
                case AutoAlternateColorType.Graphic:
                    if (_graphic.isNull()) return;
                    _graphic.color = value;
                    break;
                case AutoAlternateColorType.SpriteRenderer:
                    if (_spriteRenderer.isNull()) return;
                    _spriteRenderer.color = value;
                    break;
                case AutoAlternateColorType.Material:
                    if (_material.isNull()) return;
                    _material.color = value;
                    break;
                case AutoAlternateColorType.MeshRenderer:
                    if (_meshRenderer.isNull()) return;
                    if (_useShareMaterial)
                    {
                        _meshRenderer.sharedMaterial.color = value;
                    }
                    else
                    {
                        _meshRenderer.material.color = value;
                    }
                    break;
                case AutoAlternateColorType.None:
                    break;
                default:
                    throw new NotSupportedException();
            }
            onValueChanged?.Invoke(value);
        }
        private void setValue(float4 value)
        {
            setValue(convertF4ToColor(value));
        }

        private Color getCurrentValue()
        {
            switch (_autoAlternateColorType)
            {
                case AutoAlternateColorType.None:
                    return Color.white;
                case AutoAlternateColorType.Graphic:
                    {
                        if (_graphic.isNull()) return Color.white;
                        return _graphic.color;
                    }
                case AutoAlternateColorType.CanvasRenderer:
                    {
                        if (_canvasRenderer.isNull()) return Color.white;
                        return _canvasRenderer.GetColor();
                    }
                case AutoAlternateColorType.Material:
                    {
                        if (_material.isNull()) return Color.white;
                        return _material.color;
                    }
                case AutoAlternateColorType.SpriteRenderer:
                    {
                        if (_spriteRenderer.isNull()) return Color.white;
                        return _spriteRenderer.color;
                    }
                case AutoAlternateColorType.MeshRenderer:
                    {
                        if (_meshRenderer.isNull()) return Color.white;
                        if (!NStartRunner.IsPlaying || _useShareMaterial)
                        {
                            if (_meshRenderer.sharedMaterial) return _meshRenderer.sharedMaterial.color;
                            return Color.white;
                        }
                        else
                        {
                            if (_meshRenderer.material) return _meshRenderer.material.color;
                            return Color.white;
                        }
                    }
                default:
                    throw new NotSupportedException();
            }
        }
        
        protected override NRunnableTweener onFromTo()
        {
            return NTween.fromTo(convertColorToF4(getCurrentValue()), convertColorToF4(_toValue), FromToDuration, setValue);
        }
        protected override NRunnableTweener onToFrom()
        {
            return NTween.fromTo(convertColorToF4(getCurrentValue()), convertColorToF4(_fromValue), FromToDuration, setValue);
        }

        protected override Color getValueFromNormalizedTime(float normalizedTime)
        {
            var from = convertColorToF4(_fromValue);
            var to = convertColorToF4(_toValue);
            return convertF4ToColor(EaseUtils.ease(from, to, normalizedTime, _easeType));
        }

        private float4 convertColorToF4(Color color)
        {
            if (_hsvMode)
            {
                return color.toHsvFloat4();
            }
            else
            {
                return color.toFloat4();
            }
        }
        private Color convertF4ToColor(float4 f4)
        {
            if (_hsvMode)
            {
                return f4.toHsvColor();
            }
            else
            {
                return f4.toColor();
            }
        }
    }
}