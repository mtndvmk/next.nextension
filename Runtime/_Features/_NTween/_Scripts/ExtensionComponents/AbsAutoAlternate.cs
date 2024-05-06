using UnityEngine;

namespace Nextension.Tween
{
    public abstract class AbsAutoAlternate<T> : MonoBehaviour, IAutoAlternate
    {
        [SerializeField] protected T _fromValue;
        [SerializeField] protected T _toValue;
        [SerializeField] protected float _timePerHalfCycle = 0.5f;
        [SerializeField] protected EaseType _easeType;
        [SerializeField] protected bool _isStartOnEnable = true;
        [SerializeField] protected bool _isResetToFromValueOnEnable = true;
        [SerializeField] protected bool _onlyFromTo;
        [SerializeField] protected float _delayFromTo;
        [SerializeField] protected float _delayToFrom;

        protected NWaitable _waitable;
        protected NTweener _ntweener;

        public bool IsRunning { get; private set; }

        private void OnEnable()
        {
            if (_isResetToFromValueOnEnable)
            {
                setValue(_fromValue);
            }
            if (_isStartOnEnable)
            {
                start();
            }
        }
        private void OnDisable()
        {
#if UNITY_EDITOR
            if (NStartRunner.IsPlaying)
#endif
                stop();
        }

#if UNITY_EDITOR
        public void captureFromValue()
        {
            _fromValue = getCurrentValue();
            NAssetUtils.setDirty(this);
        }
        public void captureToValue()
        {
            _toValue = getCurrentValue();
            NAssetUtils.setDirty(this);
        }
        public void resetToFromValue()
        {
            setValue(_fromValue);
        }
        public void resetToToValue()
        {
            setValue(_toValue);
        }
#endif

        public void start()
        {
            if (IsRunning)
            {
                return;
            }
            IsRunning = true;
            runFromTo();
        }
        public void stop()
        {
            if (!IsRunning)
            {
                return;
            }
            IsRunning = false;
            if (_waitable != null)
            {
                _waitable.cancel();
                _waitable = null;
            }
            if (_ntweener != null)
            {
                _ntweener.cancel();
                _ntweener = null;
            }
        }

        public void setFromValue(T value)
        {
            _fromValue = value;
        }
        public void setToValue(T value) 
        { 
            _toValue = value;
        }
        public void setHalfCycleTime(float time)
        {
            _timePerHalfCycle = time;
        }

        private void runFromTo()
        {
            if (_delayFromTo > 0)
            {
                _waitable = new NWaitSecond(_delayFromTo).startWaitable();
                _waitable.addCompletedEvent(() =>
                {
                    if (_onlyFromTo)
                    {
                        setValue(_fromValue);
                        _ntweener = onFromTo().onCompleted(runFromTo).setEase(_easeType);
                    }
                    else
                    {
                        _ntweener = onFromTo().onCompleted(runToFrom).setEase(_easeType);
                    }
                });
            }
            else
            {
                if (_onlyFromTo)
                {
                    setValue(_fromValue);
                    _ntweener = onFromTo().onCompleted(runFromTo).setEase(_easeType);
                }
                else
                {
                    _ntweener = onFromTo().onCompleted(runToFrom).setEase(_easeType);
                }
            }
        }
        private void runToFrom()
        {
            if (_delayToFrom > 0)
            {
                _waitable = new NWaitSecond(_delayToFrom).startWaitable();
                _waitable.addCompletedEvent(() =>
                {
                    _ntweener = onToFrom().onCompleted(runFromTo).setEase(_easeType);
                });
            }
            else
            {
                _ntweener = onToFrom().onCompleted(runFromTo).setEase(_easeType);
            }
        }

        protected abstract T getCurrentValue();
        protected abstract void setValue(T value);
        protected abstract NRunnableTweener onFromTo();
        protected abstract NRunnableTweener onToFrom();
    }
    public interface IAutoAlternate
    {
#if UNITY_EDITOR
        void captureFromValue();
        void captureToValue();
        void resetToFromValue();
        void resetToToValue();
#endif
    }
}
