using System;
using UnityEngine;
using UnityEngine.Events;

namespace Nextension
{
    public sealed class TimeRunner : MonoBehaviour
    {
        [Serializable]
        internal class Runner
        {
            [NSlider(0, nameof(__getDurationForSlider))] public NMinMax triggerTime;
            public bool isNormalizedTime = false;
            [NFoldable] public UnityEvent startAction;
            [NFoldable] public UnityEvent endAction;

            [NonSerialized] public bool hasStarted = false;
            [NonSerialized] public float ownerDuration;
            private float __getDurationForSlider()
            {
                if (isNormalizedTime)
                {
                    return 1;
                }
                return ownerDuration;
            }

            public float getStartTriggerTimeInSeconds(float duration)
            {
                if (isNormalizedTime)
                {
                    return triggerTime.min * duration;
                }
                return triggerTime.min;
            }

            public float getEndTriggerTimeInSeconds(float duration)
            {
                if (isNormalizedTime)
                {
                    return triggerTime.max * duration;
                }
                return triggerTime.max;
            }

#if UNITY_EDITOR
            [NonSerialized] public bool? isNormalizedTime_Editor;
#endif
        }

        [SerializeField] private float _duration = 1;
        [SerializeField] private float _timeScale = 1;
        [SerializeField] private bool _isLoop = true;
        [SerializeField] private bool _isStartOnEnable = true;
        [SerializeField] private Runner[] _runners = Array.Empty<Runner>();

        [SerializeField, NShowIf(nameof(IsRunning)), NReadOnly, NSlider(0, nameof(_duration))] private float _currentTime;
        [SerializeField, NShowIf(nameof(IsRunning))] private bool _isPlaying;

        private float _prevDeltaTime;
        private float _startTime;

        public bool IsRunning => _startTime > 0;
        public bool IsPlaying => _isPlaying;

        public float Duration
        {
            get => _duration;
            set
            {
                _duration = value;
                restart(_isLoop);
            }
        }
        public float CurrentTime => _currentTime;

#if UNITY_EDITOR
        private float? _duration_Editor;
        private void OnValidate()
        {
            if (_duration < 0.001f)
            {
                _duration = 0.001f;
            }
            if (_timeScale < 0.001f)
            {
                _timeScale = 0.001f;
            }
            bool hasDurationChanged = false;
            float prevDuration = 0;
            if (!_duration_Editor.HasValue)
            {
                _duration_Editor = _duration;
            }
            else
            {
                if (_duration_Editor.Value != _duration)
                {
                    hasDurationChanged = true;
                    prevDuration = _duration_Editor.Value;
                    _duration_Editor = _duration;
                }
            }

            for (int i = 0; i < _runners.Length; i++)
            {
                var runner = _runners[i];
                runner.ownerDuration = _duration;
                if (!runner.isNormalizedTime_Editor.HasValue)
                {
                    runner.isNormalizedTime_Editor = runner.isNormalizedTime;
                }
                else
                {
                    if (runner.isNormalizedTime_Editor.Value != runner.isNormalizedTime)
                    {
                        runner.isNormalizedTime_Editor = runner.isNormalizedTime;
                        if (runner.isNormalizedTime)
                        {
                            // to normalized time
                            runner.triggerTime.min = _duration > 0 ? runner.triggerTime.min / _duration : 0;
                            runner.triggerTime.max = _duration > 0 ? runner.triggerTime.max / _duration : 1;
                        }
                        else
                        {
                            // to seconds
                            runner.triggerTime.min *= _duration;
                            runner.triggerTime.max *= _duration;
                        }
                    }

                    if (hasDurationChanged)
                    {
                        if (!runner.isNormalizedTime)
                        {
                            var normalizedMin = prevDuration > 0 ? runner.triggerTime.min / prevDuration : 0;
                            var normalizedMax = prevDuration > 0 ? runner.triggerTime.max / prevDuration : 1;
                            runner.triggerTime.min = normalizedMin * _duration;
                            runner.triggerTime.max = normalizedMax * _duration;
                        }
                    }
                }
            }
        }
#endif
        private void OnEnable()
        {
            if (_isStartOnEnable)
            {
                start(_isLoop);
            }
        }

        private void Update()
        {
            if (_isPlaying && _duration > 0)
            {
                var fromTime_0 = _currentTime - _prevDeltaTime;
                var toTime_0 = _currentTime;


                if (fromTime_0 < 0)
                {
                    var fromTime_1 = fromTime_0 + _duration;
                    var toTime_1 = toTime_0 + _duration;

                    for (int i = 0; i < _runners.Length; i++)
                    {
                        var runner = _runners[i];
                        __checkRunnerTime(runner, fromTime_1, toTime_1);
                    }
                }

                for (int i = 0; i < _runners.Length; i++)
                {
                    var runner = _runners[i];
                    __checkRunnerTime(runner, fromTime_0, toTime_0);
                }

                _prevDeltaTime = __getDeltaTime() * _timeScale;
                _currentTime += _prevDeltaTime;

                if (_isLoop && _currentTime >= _duration)
                {
                    _currentTime -= _duration;
                }
            }
        }

        private void __checkRunnerTime(Runner runner, float fromTime, float toTime)
        {
            var startTriggerTime = runner.getStartTriggerTimeInSeconds(_duration);
            var endTriggerTime = runner.getEndTriggerTimeInSeconds(_duration);

            if (!runner.hasStarted)
            {
                if (startTriggerTime >= fromTime && startTriggerTime < toTime)
                {
                    runner.hasStarted = true;
                    runner.startAction?.Invoke();
                }
            }
            if (runner.hasStarted)
            {
                if (endTriggerTime >= fromTime && endTriggerTime < toTime)
                {
                    runner.hasStarted = false;
                    runner.endAction?.Invoke();
                }
            }
        }

        private float __getTime()
        {
            return Time.realtimeSinceStartup;
        }
        private float __getDeltaTime()
        {
            return Time.deltaTime;
        }

        [ContextMenu("Start")]
        public void start()
        {
            start(_duration, _isLoop);
        }
        public void start(bool loop)
        {
            start(_duration, loop);
        }
        public void start(float duration, bool loop)
        {
            if (IsRunning)
            {
                return;
            }
            _duration = duration;
            if (_duration <= 0)
            {
                Debug.LogError("Duration must be greater than 0");
                return;
            }
            _isLoop = loop;
            _isPlaying = true;
            _startTime = __getTime();
            _prevDeltaTime = Time.deltaTime;
        }

        public void startFromTime(float fromTime, bool loop)
        {
            start(loop);
            _currentTime += fromTime;
        }
        public void startFromTime(float fromTime, float duration, bool loop)
        {
            start(duration, loop);
            _currentTime += fromTime;
        }

        public void startFromNormalizedTime(float fromTime, bool loop)
        {
            start(loop);
            _currentTime += fromTime * _duration;
        }
        public void startFromNormalizedTime(float fromTime, float duration, bool loop)
        {
            start(duration, loop);
            _currentTime += fromTime * _duration;
        }

        public void restart(bool loop = true)
        {
            stop();
            start(loop);
        }

        public void pause()
        {
            if (!IsRunning)
            {
                return;
            }
            _isPlaying = false;
        }
        public void resume()
        {
            if (!IsRunning)
            {
                return;
            }
            _isPlaying = true;
        }
        [ContextMenu("Stop")]
        public void stop()
        {
            _startTime = 0;
            _currentTime = 0;
            _isPlaying = false;
        }
    }
}
