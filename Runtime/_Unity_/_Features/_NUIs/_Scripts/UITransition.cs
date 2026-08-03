using System;
using Nextension.Tween;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

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

        [SerializeField] protected RectTransform _scaler;
        [SerializeField] protected float _effectDuration = 0.15f;
        [SerializeField] protected float _fromScaleValue = 0.4f;
        [SerializeField] protected float _punchScaleValue = 0.04f;
        [SerializeField] protected bool _addBlockingUIForScaler = true;
        [SerializeField] protected bool _hideOnSetup = true;
        [SerializeField] protected bool _hideWhenClickOnOutSide = true;
        [SerializeField] protected EffectOption _effectOption = EffectOption.MoveDown;

        [Header("Optimize")]
        [SerializeField] protected bool _notUseSubCanvas;
        [SerializeField] protected bool _forceDeactiveOnHide;

        private NButton _closeBgButton;
        private CanvasGroup _canvasGroup;
        private Canvas _canvas;
        private Func<bool> _waitHidden;
        private Action<float2> _setScalerAnchorPositionAction;
        private Action _hideUIComponentAction;
        private Action _scaleScalerToOneAction;
        private Action<float> _setCanvasGroupAlphaAction;

        protected bool _isShown = true;
        protected bool _isHiding = false;
        protected bool _isSetup;
        protected Vector2 _anchoredPosition;

        public RectTransform Scaler => _scaler;
        public Func<bool> WaitHidden => _waitHidden ??= isHidden;
        public bool IsShown => _isShown;
        public float EffectDuration => _effectDuration;

        protected virtual void Awake()
        {
            __innerSetup();
        }

        protected virtual void OnDisable()
        {
            if (_isShown)
            {
                __runHideAnimation(true);
            }
        }
        
        private void __innerSetup()
        {
            if (_isSetup) return;

            _canvasGroup = gameObject.getOrAddComponent<CanvasGroup>();
            _closeBgButton = gameObject.getOrAddComponent<NButton>();
            if (!_notUseSubCanvas)
            {
                _canvas = gameObject.getOrAddComponent<Canvas>();
                gameObject.getOrAddComponent<GraphicRaycaster>();
            }

            if (_addBlockingUIForScaler)
            {
                if (!_scaler.TryGetComponent<Graphic>(out _))
                {
                    _scaler.gameObject.getOrAddComponent<NRaycastTargetUI>();
                }
                else
                {
                    _scaler.gameObject.getOrAddComponent<NHandleEventUI>();
                }
            }

            _anchoredPosition = _scaler.anchoredPosition;
            
            _closeBgButton.onButtonClickEvent.AddListener(() =>
            {
                hide();
            });

            _closeBgButton.Interactable = _hideWhenClickOnOutSide;

            onDerivedSetup();
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

        public bool isHidden() => !_isHiding && !_isShown;

        public async NTask asyncWaitHidden()
        {
            await new NWaitUntil(WaitHidden);
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

        protected void innerShow(bool isImmediate = false, bool isForceAnimation = false)
        {
            __innerSetup();
            if (!_isShown)
            {
                __showUIComponent();
                __runShowAnimation(isImmediate);
                _canvasGroup.blocksRaycasts = true;
            }
            else if (isForceAnimation)
            {
                __runHideAnimation(true);
                __runShowAnimation(isImmediate);
            }
        }

        protected void innerHide(bool isImmediate = false, bool isforce = false)
        {
            __innerSetup();
            if ((!_isShown || _isHiding) && !isforce)
            {
                return;
            }
            _isHiding = true;
            onDerivedBeforeHide();
            _isShown = false;
            _canvasGroup.blocksRaycasts = false;
            __runHideAnimation(isImmediate);
        }

        private void __runShowAnimation(bool isImmediate = false)
        {
            NTween.cancelAllTweeners(gameObject);
            if (isImmediate || _effectOption == EffectOption.None)
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
                        _scaleScalerToOneAction ??= __scaleScalerToOne;
                        NTween.scaleTo(_scaler, new float3(1 + _punchScaleValue), _effectDuration * 0.8f).onCompleted(_scaleScalerToOneAction).setCancelControlKey(gameObject);
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
                    _setScalerAnchorPositionAction ??= __setScalerAnchorPosition;
                    NTween.fromTo(_scaler.anchoredPosition, _anchoredPosition, _effectDuration, _setScalerAnchorPositionAction);
                }
                else
                {
                    _scaler.anchoredPosition = _anchoredPosition;
                }

                _setCanvasGroupAlphaAction ??= (a) => _canvasGroup.alpha = a;
                NTween.fromTo(_canvasGroup.alpha, 1, _effectDuration, _setCanvasGroupAlphaAction).setCancelControlKey(gameObject);
            }
        }

        private void __scaleScalerToOne()
        {
            NTween.scaleTo(_scaler, new float3(1), _effectDuration * 0.2f).setCancelControlKey(gameObject);
        }

        private void __runHideAnimation(bool isImmediate = false)
        {
            NTween.cancelAllTweeners(gameObject);
            if (isImmediate)
            {
                _canvasGroup.alpha = 0;
                __hideUIComponent();

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
                    _setScalerAnchorPositionAction ??= __setScalerAnchorPosition;
                    NTween.fromTo(_scaler.anchoredPosition, targetAnchorPos, _effectDuration, _setScalerAnchorPositionAction);
                }

                _hideUIComponentAction ??= __hideUIComponent;
                NTween.fromTo(_canvasGroup.alpha, 0, _effectDuration, v => _canvasGroup.alpha = v).onCompleted(_hideUIComponentAction).setCancelControlKey(gameObject);
            }
        }

        private void __setScalerAnchorPosition(float2 anchorPosition)
        {
            if (_scaler.isNull()) return;
            _scaler.anchoredPosition = anchorPosition;
        }

        private void __showUIComponent()
        {
            onDerivedBeforeShow();
            _isShown = true;
            _isHiding = false;
            if (_canvas != null) _canvas.setEnable(true);
            gameObject.setActive(true);
        }

        private void __hideUIComponent()
        {
            if (_forceDeactiveOnHide) gameObject.setActive(false);
            if (_canvas != null) _canvas.setEnable(false);
            _isHiding = false;
            onDerivedOnAfterHide();
        }

        protected virtual void onDerivedSetup()
        {

        }

        protected virtual void onDerivedBeforeShow()
        {

        }

        protected virtual void onDerivedBeforeHide()
        {

        }

        protected virtual void onDerivedOnAfterHide()
        {
        }

    }

    public abstract class S_UITransition<T> : UITransition, ISingletonable where T : UITransition, ISingletonable
    {
        public static T Instance => S_<T>.Instance;

        protected virtual void OnValidate()
        {
            if (GetType() != typeof(T))
            {
                NDebug.LogError($"Type is missmatch ({GetType()},{typeof(T)})", this);
            }
        }
    }
}

