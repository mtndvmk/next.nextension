using UnityEngine;
using UnityEngine.Events;

namespace Nextension.Tween
{
    public sealed class AutoAlternateTimer : AbsAutoAlternate
    {
        [SerializeField, NDisplayName("From-To Duration")] private float _timePerHalfCycle = 0.5f;
        [SerializeField] private EaseType _easeType;
        [SerializeField] private float _delayOnStart;
        [SerializeField, NReadOnly(nameof(IsRunning)), Range(0, 1)] private float _startNormalizeTime;
        [SerializeField] private bool _isStartOnEnable = true;

        [SerializeField] private bool _onlyFromTo;
        [SerializeField, NIndent, NDisplayName("Delay From-To")] private float _delayFromTo;
        [SerializeField, NIndent, NDisplayName("Delay To-From"), NShowIf(nameof(_onlyFromTo), false)] private float _delayToFrom;
        [SerializeField, Tooltip("<u>LifeTime</u> less than or equal to 0 is infinite")] private float _lifeTime = 0;

        [NGroup("Event")] public UnityEvent<float> onTimeChanged;
        [NGroup("Event")] public UnityEvent<float> onNormalizedTimeChanged;
        [NGroup("Event")] public UnityEvent onRunFromTo;
        [NGroup("Event")] public UnityEvent onRunToFrom;

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
        private void OnEnable()
        {
            if (_isStartOnEnable)
            {
                startDelay(DeplayOnStart, LifeTime);
            }
        }
        private void OnDisable()
        {
#if UNITY_EDITOR
            if (NStartRunner.IsPlaying)
#endif
                stop();
        }

        public override void updateAtNormalizedTime(float normalizedTime)
        {
            onTimeUpdated(normalizedTime);
        }

        protected override void runFromTo(float normalizedTimeOffset)
        {
            _isFromTo = true;
            if (_delayFromTo > 0)
            {
                _task.forget();
                _task = runFromToWithDelay(_delayFromTo, normalizedTimeOffset);
            }
            else
            {
                runFromToWithoutDelay(normalizedTimeOffset);
            }
        }
        private async NTask runFromToWithDelay(float delayTime, float normalizedTimeOffset)
        {
            await new NWaitSecond(delayTime);
            runFromToWithoutDelay(normalizedTimeOffset);
        }
        protected override void runFromToWithoutDelay(float normalizedTimeOffset)
        {
            if (_timePerHalfCycle == 0)
            {
                if (_onlyFromTo)
                {
                    _task.forget();
                    _task = NTask.waitAndRunAsync(new NWaitFrame(1), runFromTo);
                }
                else
                {
                    _task.forget();
                    _task = NTask.waitAndRunAsync(new NWaitFrame(1), runToFrom);
                }
            }
            else if (_onlyFromTo)
            {
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
                _task.forget();
                _task = delayRunToFrom(_delayToFrom, normalizedTimeOffset);
            }
            else
            {
                runToFromWithoutDelay(normalizedTimeOffset);
            }
        }
        private async NTask delayRunToFrom(float deplayTime, float normalizedTimeOffset)
        {
            await new NWaitSecond(deplayTime);
            runToFromWithoutDelay(normalizedTimeOffset);
        }
        protected override void runToFromWithoutDelay(float normalizedTimeOffset)
        {
            if (_timePerHalfCycle == 0)
            {
                _task.forget();
                _task = NTask.waitAndRunAsync(new NWaitFrame(1), runFromTo);
                return;
            }
            _ntweener = applyDuration(onToFrom().onCompleted(runFromTo).setEase(_easeType));
            _ntweener.startNormalizedTime(normalizedTimeOffset);
            onRunToFrom.Invoke();
        }
        private NRunnableTweener onFromTo()
        {
            return NTween.fromTo(0f, 1, _timePerHalfCycle, onTimeUpdated);
        }
        private NRunnableTweener onToFrom()
        {
            return NTween.fromTo(1, 0f, _timePerHalfCycle, onTimeUpdated);
        }
        private void onTimeUpdated(float normalizedTime)
        {
            var time = normalizedTime * _timePerHalfCycle;
            onTimeChanged.Invoke(time);
            onNormalizedTimeChanged.Invoke(normalizedTime);
        }
    }
}
