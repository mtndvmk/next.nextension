using UnityEngine;
using UnityEngine.Events;

namespace Nextension.Tween
{
    public abstract class AbsAutoAlternate : MonoBehaviour
    {
        protected enum ValState : byte
        {
            None,
            From,
            To,
        }
        protected float getTime()
        {
            return Time.time + 1;
        }

        protected abstract void runFromTo(float normalizedTimeOffset);
        protected abstract void runFromToWithoutDelay(float normalizedTimeOffset);
        protected abstract void runToFromWithoutDelay(float normalizedTimeOffset);

        public abstract void updateAtNormalizedTime(float normalizedTime);

        protected virtual void onResume() { }
        protected virtual void onPause() { }
        protected virtual void onStart() { }
        protected virtual void onStop() { }
        
        private float _pauseNormalizedTime = -1;
        private float _startTime;

        protected bool _isFromTo;
        protected NWaitable _waitable;
        protected NTweener _ntweener;

        public bool IsRunning => _startTime > 0;
        public bool IsPaused => _pauseNormalizedTime >= 0;
        public float TimeSinceStart
        {
            get
            {
                if (!IsRunning) return 0;
                return getTime() - _startTime;
            }
        }
        public float CurrentTime
        {
            get
            {
                if (IsPaused) return (_isFromTo ? _pauseNormalizedTime : (1 - _pauseNormalizedTime)) * FromToDuration;
                if (_ntweener == null) return 0;
                if (_isFromTo) return _ntweener.Time;
                else return FromToDuration - _ntweener.Time;
            }
        }
        
        public abstract float FromToDuration { get; set; }
        public abstract float LifeTime { get; set; }
        public abstract float DeplayOnStart { get; set; }
        public abstract float StartNormalizedTime { get; set; }

        /// <summary>
        /// <u>lifeTime</u> less or equal 0 is infinite
        /// </summary>
        public void startImmediate(float lifeTime)
        {
            LifeTime = lifeTime;
            startImmediate();
        }
        public void startImmediate()
        {
            if (IsRunning)
            {
                return;
            }
            _startTime = getTime();
            onStart();
            runFromTo(StartNormalizedTime);
        }
        /// <summary>
        /// <u>lifeTime</u> less or equal 0 is infinite
        /// </summary>
        public void startDelay(float delayTime, float lifeTime = 0)
        {
            LifeTime = lifeTime;
            if (delayTime > 0)
            {
                DeplayOnStart = delayTime;
                new NWaitSecond(DeplayOnStart).startWaitable().addCompletedEvent(startImmediate);
            }
            else
            {
                startImmediate();
            }
        }

        public void resume()
        {
            if (_pauseNormalizedTime >= 0)
            {
                if (_isFromTo)
                {
                    runFromToWithoutDelay(_pauseNormalizedTime);
                }
                else
                {
                    runToFromWithoutDelay(_pauseNormalizedTime);
                }
                _pauseNormalizedTime = -1;
                onResume();
            }
        }
        public void pause()
        {
            if (!IsRunning || IsPaused)
            {
                return;
            }
            if (_ntweener == null)
            {
                _pauseNormalizedTime = -1;
                return;
            }
            _pauseNormalizedTime = _ntweener.NormalizedTime;
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
            onPause();
        }
        public void stop()
        {
            if (!IsRunning)
            {
                return;
            }
            _startTime = -1;
            _pauseNormalizedTime = -1;
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
            onStop();
        }

        protected void runFromTo()
        {
            runFromTo(0);
        }
        protected NTweener applyDuration(NTweener tweener)
        {
            if (LifeTime > 0)
            {
                tweener.onUpdated(checkLifeTimeAndStop);
            }
            return tweener;
        }
        protected void checkLifeTimeAndStop()
        {
            if (getTime() - _startTime >= LifeTime)
            {
                stop();
            }
        }
    }

    public abstract class AbsAutoAlternate<T> : AbsAutoAlternate
    {
        [SerializeField, NShowIf(nameof(EnableOffset))] protected bool _useOffset;
        [SerializeField, NConstrainable, NShowIf(nameof(EnableOffset))] protected T _offset;
        [SerializeField, NConstrainable] protected T _fromValue;
        [SerializeField, NConstrainable] protected T _toValue;
        [SerializeField, NDisplayName("From-To Duration")] private float _timePerHalfCycle = 0.5f;
        [SerializeField] protected EaseType _easeType;
        [SerializeField] protected float _delayOnStart;
        [SerializeField] protected bool _updateOnValidate = false;
        [SerializeField, NReadOnly(nameof(IsRunning)), Range(0, 1)] protected float _startNormalizeTime;
        [SerializeField] protected bool _isStartOnEnable = true;

        [SerializeField, NGroup("Reset value"), NDisplayName("On Enable")] protected ValState _resetValueOnEnable;
        [SerializeField, NGroup("Reset value"), NDisplayName("On Start")] protected ValState _resetValueOnStart;
        [SerializeField, NGroup("Reset value"), NDisplayName("On Stop")] protected ValState _resetValueOnStop;
        [SerializeField, NGroup("Reset value"), NDisplayName("On Disable")] protected ValState _resetValueOnDisable;

        [SerializeField] protected bool _onlyFromTo;
        [SerializeField, NIndent, NDisplayName("Delay From-To")] protected float _delayFromTo;
        [SerializeField, NIndent, NDisplayName("Delay To-From"), NShowIf(nameof(_onlyFromTo), false)] protected float _delayToFrom;
        [SerializeField, Tooltip("<u>LifeTime</u> less than or equal to 0 is infinite")] protected float _lifeTime = 0;

        [NGroup("Event")] public UnityEvent<T> onValueChanged;
        [NGroup("Event")] public UnityEvent onRunFromTo;
        [NGroup("Event")] public UnityEvent onRunToFrom;

        public virtual bool EnableOffset => false;

        public override float FromToDuration
        {
            get => _timePerHalfCycle;
            set => _timePerHalfCycle = value;
        }
        public override float LifeTime
        {
            get => _lifeTime;
            set => _lifeTime = value;
        }
        public override float DeplayOnStart 
        { 
            get => _delayOnStart; 
            set => _delayOnStart = value; 
        }
        public override float StartNormalizedTime
        {
            get => _startNormalizeTime;
            set => _startNormalizeTime = value;
        }
        protected virtual void OnValidate()
        {
            if (_updateOnValidate && !IsRunning)
            {
                updateAtNormalizedTime(_startNormalizeTime);
            }
        }
        protected virtual void OnEnable()
        {
            switch (_resetValueOnEnable)
            {
                case ValState.From:
                    setValue(_fromValue);
                    break;
                case ValState.To:
                    setValue(_toValue);
                    break;
            }
            if (_isStartOnEnable)
            {
                startDelay(DeplayOnStart, LifeTime);
            }
        }
        protected virtual void OnDisable()
        {
            switch (_resetValueOnDisable)
            {
                case ValState.From:
                    setValue(_fromValue);
                    break;
                case ValState.To:
                    setValue(_toValue);
                    break;
            }
#if UNITY_EDITOR
            if (NStartRunner.IsPlaying)
#endif
                stop();
        }

        /// <summary>
        /// <u>duration</u> less or equal 0 is infinite
        /// </summary>
        /// <param name="duration"></param>


        protected override void onStart()
        {
            base.onStart();
            switch (_resetValueOnStart)
            {
                case ValState.From:
                    setValue(_fromValue);
                    break;
                case ValState.To:
                    setValue(_toValue);
                    break;
            }
        }
        protected override void onStop()
        {
            base.onStop();
            switch (_resetValueOnStop)
            {
                case ValState.From:
                    setValue(_fromValue);
                    break;
                case ValState.To:
                    setValue(_toValue);
                    break;
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

        public override void updateAtNormalizedTime(float normalizedTime)
        {
            setValue(getValueFromNormalizedTime(normalizedTime));
        }
       
        protected override void runFromTo(float normalizedTimeOffset)
        {
            _isFromTo = true;
            if (_delayFromTo > 0)
            {
                _waitable = runFromToWithDelay(_delayFromTo, normalizedTimeOffset);
            }
            else
            {
                runFromToWithoutDelay(normalizedTimeOffset);
            }
        }
        private async NWaitable runFromToWithDelay(float delayTime, float normalizedTimeOffset)
        {
            await new NWaitSecond(delayTime);
            runFromToWithoutDelay(normalizedTimeOffset);
        }
        protected override void runFromToWithoutDelay(float normalizedTimeOffset)
        {
            if (_timePerHalfCycle == 0)
            {
                _waitable = new NWaitFrame(1).startWaitable();
                if (_onlyFromTo)
                {
                    _waitable.addCompletedEvent(runFromTo);
                }
                else
                {
                    _waitable.addCompletedEvent(runToFrom);
                }
            }
            else if (_onlyFromTo)
            {
                setValue(_fromValue);
                _ntweener = applyDuration(onFromTo().onCompleted(runFromTo).setEase(_easeType));
            }
            else
            {
                _ntweener = applyDuration(onFromTo().onCompleted(runToFrom).setEase(_easeType));
            }
            _ntweener.startNormalizedTime(normalizedTimeOffset);
            onRunFromTo?.Invoke();
        }

        private void runToFrom()
        {
            runToFrom(0);
        }
        private void runToFrom(float normalizedTimeOffset)
        {
            _isFromTo = false;
            if (_delayToFrom > 0)
            {
                _waitable = delayRunToFrom(_delayToFrom, normalizedTimeOffset);
            }
            else
            {
                runToFromWithoutDelay(normalizedTimeOffset);
            }
        }
        private async NWaitable delayRunToFrom(float deplayTime, float normalizedTimeOffset)
        {
            await new NWaitSecond(deplayTime);
            runToFromWithoutDelay(normalizedTimeOffset);
        }
        protected override void runToFromWithoutDelay(float normalizedTimeOffset)
        {
            if (_timePerHalfCycle == 0)
            {
                _waitable = new NWaitFrame(1).startWaitable();
                _waitable.addCompletedEvent(runFromTo);
                return;
            }
            _ntweener = applyDuration(onToFrom().onCompleted(runFromTo).setEase(_easeType));
            _ntweener.startNormalizedTime(normalizedTimeOffset);
            onRunToFrom?.Invoke();
        }
        
        protected abstract void setValue(T value);
        protected abstract NRunnableTweener onFromTo();
        protected abstract NRunnableTweener onToFrom();
        protected abstract T getValueFromNormalizedTime(float normalizedTime);
    }
}
