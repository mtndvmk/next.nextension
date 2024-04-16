using UnityEngine;

namespace Nextension.Tween
{
    public abstract class AbsAutoAlternate<T> : MonoBehaviour
    {
        [SerializeField] protected T _fromValue;
        [SerializeField] protected T _toValue;
        [SerializeField] protected float _timePerHalfCycle = 0.5f;
        [SerializeField] protected bool _isStartOnEnable = true;
        [SerializeField] protected bool _isResetToFromValueOnEnable = true;
        [SerializeField] protected bool _onlyFromTo;
        [SerializeField] protected float _delayOnlyFromTo;

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
        [ContextMenu("Capture FromValue")]
        private void captureFromValue()
        {
            _fromValue = getCurrentValue();
            NAssetUtils.setDirty(this);
        }
        [ContextMenu("Capture ToValue")]
        private void captureToValue()
        {
            _toValue = getCurrentValue();
            NAssetUtils.setDirty(this);
        }
#endif

        public void start()
        {
            if (IsRunning)
            {
                return;
            }
            runFromTo();
        }
        public void stop()
        {
            if (!IsRunning)
            {
                return;
            }
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
            if (_onlyFromTo)
            {
                setValue(_fromValue);
                if (_delayOnlyFromTo > 0)
                {
                     _waitable = new NWaitSecond(_delayOnlyFromTo).startWaitable();
                    _waitable.addCompletedEvent(() =>
                    {
                        _ntweener = onFromTo().onCompleted(runFromTo);
                    });
                }
                else
                {
                    _ntweener = onFromTo().onCompleted(runFromTo);
                }
            }
            else
            {
                _ntweener = onFromTo().onCompleted(runToFrom);
            }
        }
        private void runToFrom()
        {
            _ntweener = onToFrom().onCompleted(runFromTo);
        }

        protected abstract T getCurrentValue();
        protected abstract void setValue(T value);
        protected abstract NTweener onFromTo();
        protected abstract NTweener onToFrom();
    }
}
