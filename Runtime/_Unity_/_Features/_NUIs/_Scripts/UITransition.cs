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

        [SerializeField] protected RectTransform _scaler;
        [SerializeField] protected float _effectDuration = 0.15f;
        [SerializeField] protected float _fromScaleValue = 0.4f;
        [SerializeField] protected float _punchScaleValue = 0.04f;
        [SerializeField] protected bool _addBlockingUIForScaler = true;
        [SerializeField] protected bool _hideOnSetup = true;
        [SerializeField] protected bool _hideWhenClickOnOutSide = true;
        [SerializeField] protected EffectOption _effectOption = EffectOption.MoveDown;

        private NButton _closeBgButton;
        private CanvasGroup _canvasGroup;
        private Func<bool> _waitHidden;
        private Func<bool> _waitEndEffect;
        private byte _showHideEffectState;
        private Action<float2> _setScalerAnchorPositionAction;
        private Action _deactiveGameObjectAction;
        private Action _scaleScalerToOneAction;
        private Action<float> _setCanvasGroupAlphaAction;

        protected bool _isShown = true;
        protected bool _isHiding = false;
        protected bool _isSetup;

        public RectTransform Scaler => _scaler;
        public Func<bool> WaitHidden => _waitHidden ??= isHidden;
        public Func<bool> WaitEndEffect => _waitEndEffect ??= () => _showHideEffectState == 0;

        protected Vector2 _anchoredPosition;

        public bool IsShown => _isShown;
        public float EffectDuration => _effectDuration;


        protected virtual void Awake()
        {
            innerSetup();
        }

        private void innerSetup()
        {
            if (_isSetup) return;

            _canvasGroup = gameObject.getOrAddComponent<CanvasGroup>();
            _closeBgButton = gameObject.getOrAddComponent<NButton>();

            if (_addBlockingUIForScaler)
            {
                if (!_scaler.TryGetComponent<UnityEngine.UI.Graphic>(out _))
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

        public bool isHidden() => !gameObject.activeSelf;
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
            innerSetup();
            if (!_isShown)
            {
                onDerivedBeforeShow();
                _isShown = true;
                gameObject.setActive(true);
                runShowAnimation(isImmediate);
                _canvasGroup.blocksRaycasts = true;
            }
            else if (isForceAnimation)
            {
                __runHideAnimation(true);
                runShowAnimation(isImmediate);
            }
        }
        protected void innerHide(bool isImmediate = false, bool isforce = false)
        {
            innerSetup();
            if ((!_isShown || _isHiding) && !isforce)
            {
                return;
            }
            _isHiding = true;
            onDerivedBeforeHide();
            _isShown = false;
            __runHideAnimation(isImmediate);
            _canvasGroup.blocksRaycasts = false;
            _isHiding = false;
        }

        private void runShowAnimation(bool isImmediate = false)
        {
            NTween.cancelAllTweeners(gameObject);
            if (isImmediate || _effectOption == EffectOption.None)
            {
                _scaler.localScale = Vector3.one;
                _canvasGroup.alpha = 1;
                _showHideEffectState = 0;
            }
            else
            {
                __checkEffectRunningState().forget();
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

        private async NTaskVoid __checkEffectRunningState()
        {
            var state = ++_showHideEffectState;
            await new NWaitSecond(_effectDuration);
            if (state == _showHideEffectState)
            {
                _showHideEffectState = 0;
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
                gameObject.setActive(false);

                if (_effectOption == EffectOption.MoveDown)
                {
                    _scaler.anchoredPosition = _anchoredPosition.plusY(_scaler.rect.size.x / -10);
                }
                if (_effectOption == EffectOption.Scale)
                {
                    _scaler.localScale = new Vector3(_fromScaleValue, _fromScaleValue, _fromScaleValue);
                }
                _showHideEffectState = 0;
            }
            else
            {
                __checkEffectRunningState().forget();
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

                _deactiveGameObjectAction ??= __deactiveGameObject;
                NTween.fromTo(_canvasGroup.alpha, 0, _effectDuration, v => _canvasGroup.alpha = v).onCompleted(_deactiveGameObjectAction).setCancelControlKey(gameObject);
            }
        }

        private void __setScalerAnchorPosition(float2 anchorPosition)
        {
            _scaler.anchoredPosition = anchorPosition;
        }
        
        private void __deactiveGameObject()
        {
            gameObject.setActive(false);
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

        public async NTask asyncWaitHidden()
        {
            await new NWaitUntil(WaitHidden);
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

