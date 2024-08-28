using Nextension.Tween;
using System;
using Unity.Mathematics;
using UnityEngine;

namespace Nextension.UI
{
    public class UITransition : MonoBehaviour
    {
        public enum EffectOption : byte
        {
            None,
            Scale,
            MoveDown,
        }

        [SerializeField] private RectTransform _target;
        [SerializeField] private RectTransform _scaler;
        [SerializeField] private float _effectDuration = 0.15f;
        [SerializeField] private float _fromScaleValue = 0.4f;
        [SerializeField] private float _punchScaleValue = 0.04f;
        [SerializeField] private bool _addBlockingUIForScaler = true;
        [SerializeField] private bool _hideOnSetup = true;
        [SerializeField] private bool _hideWhenClickOnOutSide = true;
        [SerializeField] private EffectOption _effectOption = EffectOption.MoveDown;

        private NButton _closeBgButton;
        private CanvasGroup _canvasGroup;
        protected bool _isOpen = true;
        protected bool _isSetup;

        public RectTransform Scaler => _scaler;
        protected Vector2 _anchoredPosition;

        public event Action onShowEvent;
        public event Action onHideEvent;

        protected virtual void Reset()
        {
            _target = GetComponentInChildren<RectTransform>(true);
        }

        protected virtual void Awake()
        {
            innerSetup();
        }

        private void innerSetup()
        {
            if (_isSetup) return;

            _canvasGroup = _target.getOrAddComponent<CanvasGroup>();
            _closeBgButton = _target.getOrAddComponent<NButton>();

            if (_addBlockingUIForScaler)
            {
                _scaler.getOrAddComponent<NInteractiveBlockingUI>();
            }

            _anchoredPosition = _scaler.anchoredPosition;
            _closeBgButton.onButtonClickEvent.AddListener(() =>
            {
                hide();
            });

            _closeBgButton.Interactable = _hideWhenClickOnOutSide;

            onBeforeSetup();
            _isSetup = true;

            if (_hideOnSetup)
            {
                innerHide(true, true);
            }
        }

        public void hide()
        {
            innerHide(false);
        }
        public void hide(bool isImmediate)
        {
            innerHide(isImmediate);
        }
        public void setAnchorPosition(Vector2 anchorPosition)
        {
            _anchoredPosition = anchorPosition;
        }

        protected void setClosableFromBgButton(bool isClosable)
        {
            _hideWhenClickOnOutSide = isClosable;
            _closeBgButton.Interactable = isClosable;
        }
        protected void innerShow(bool isImmediate = false)
        {
            innerSetup();
            if (!_isOpen)
            {
                onBeforeShow();
                onShowEvent?.Invoke();
                _isOpen = true;
                gameObject.setActive(true);
                NTween.cancelAllTweeners(gameObject);
                if (isImmediate)
                {
                    _scaler.localScale = Vector3.one;
                    _canvasGroup.alpha = 1;
                }
                else
                {
                    if (_effectOption == EffectOption.Scale)
                    {
                        if (_punchScaleValue > 0)
                        {
                            NTween.scaleTo(_scaler, new float3(1 + _punchScaleValue), _effectDuration * 0.8f).onCompleted(() =>
                            {
                                NTween.scaleTo(_scaler, new float3(1), _effectDuration * 0.2f).setCancelControlKey(gameObject);
                            }).setCancelControlKey(gameObject);
                        }
                        else
                        {
                            NTween.scaleTo(_scaler, new float3(1), _effectDuration).setCancelControlKey(gameObject);
                        }
                    }
                    else
                    {
                        _scaler.localScale = Vector3.one;
                    }

                    if (_effectOption == EffectOption.MoveDown)
                    {
                        NTween.fromTo<float2>(_scaler.anchoredPosition, _anchoredPosition, _effectDuration, v => _scaler.anchoredPosition = v);
                    }
                    else
                    {
                        _scaler.anchoredPosition = _anchoredPosition;
                    }

                    NTween.fromTo(_canvasGroup.alpha, 1, _effectDuration, v => _canvasGroup.alpha = v).setCancelControlKey(gameObject);
                }
                _canvasGroup.blocksRaycasts = true;
            }
        }
        protected void innerHide(bool isImmediate = false, bool isforce = false)
        {
            innerSetup();
            if (!_isOpen && !isforce)
            {
                return;
            }
            onBeforeHide();
            onHideEvent?.Invoke();
            _isOpen = false;

            NTween.cancelAllTweeners(gameObject);
            if (isImmediate)
            {
                _canvasGroup.alpha = 0;
                gameObject.setActive(false);

                if (_effectOption == EffectOption.MoveDown)
                {
                    _scaler.anchoredPosition = _anchoredPosition.plusY(_scaler.rect.size.x / -10);
                }
                if (_effectOption == EffectOption.Scale)
                {
                    _scaler.localScale = new Vector3(_fromScaleValue, _fromScaleValue, _fromScaleValue);
                }
            }
            else
            {
                if (_effectOption == EffectOption.Scale)
                {
                    NTween.scaleTo(_scaler, new float3(_fromScaleValue), _effectDuration).setCancelControlKey(gameObject);
                }
                if (_effectOption == EffectOption.MoveDown)
                {
                    var targetAnchorPos = _anchoredPosition.plusY(_scaler.rect.size.x / -10);
                    NTween.fromTo<float2>(_scaler.anchoredPosition, targetAnchorPos, _effectDuration, v => _scaler.anchoredPosition = v);
                }

                NTween.fromTo(_canvasGroup.alpha, 0, _effectDuration, v => _canvasGroup.alpha = v).onCompleted(() =>
                {
                    gameObject.setActive(false);
                }).setCancelControlKey(gameObject);
            }
            _canvasGroup.blocksRaycasts = false;
        }

        protected virtual void onBeforeSetup()
        {

        }
        protected virtual void onBeforeShow()
        {

        }
        protected virtual void onBeforeHide()
        {

        }
    }
}
