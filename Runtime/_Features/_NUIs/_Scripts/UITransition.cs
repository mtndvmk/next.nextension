using Nextension.Tween;
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
        protected bool _isShown = true;
        protected bool _isSetup;

        public RectTransform Scaler => _scaler;
        protected Vector2 _anchoredPosition;

        public bool IsShown => _isShown;
        public bool IsHidden => !gameObject.activeSelf;

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
                _scaler.getOrAddComponent<NInteractiveBlockingUI>();
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
                runHideAnimation(true);
                runShowAnimation(isImmediate);
            }
        }
        protected void innerHide(bool isImmediate = false, bool isforce = false)
        {
            innerSetup();
            if (!_isShown && !isforce)
            {
                return;
            }
            onDerivedBeforeHide();
            _isShown = false;
            runHideAnimation(isImmediate);
            _canvasGroup.blocksRaycasts = false;
        }

        private void runShowAnimation(bool isImmediate = false)
        {
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
                    NTween.fromTo(_scaler.anchoredPosition, _anchoredPosition, _effectDuration, setScalerAnchorPosition);
                }
                else
                {
                    _scaler.anchoredPosition = _anchoredPosition;
                }

                NTween.fromTo(_canvasGroup.alpha, 1, _effectDuration, v => _canvasGroup.alpha = v).setCancelControlKey(gameObject);
            }
        }
        private void runHideAnimation(bool isImmediate = false)
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
                    NTween.fromTo(_scaler.anchoredPosition, targetAnchorPos, _effectDuration, setScalerAnchorPosition);
                }

                NTween.fromTo(_canvasGroup.alpha, 0, _effectDuration, v => _canvasGroup.alpha = v).onCompleted(deactiveGameObject).setCancelControlKey(gameObject);
            }
        }

        protected void setScalerAnchorPosition(float2 anchorPosition)
        {
            _scaler.anchoredPosition = anchorPosition;
        }
        protected void deactiveGameObject()
        {
            gameObject.setActive(false);
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
    }

    public abstract class S_UITransition<T> : UITransition, ISingletonable where T : UITransition, ISingletonable
    {
        public static T Instance => S_<T>.Instance;

        protected virtual void OnValidate()
        {
            if (GetType() != typeof(T))
            {
                Debug.LogError($"Type is missmatch ({GetType()},{typeof(T)})", this);
            }
        }
    }
}
