using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nextension.Tween
{
    public abstract class NTweener : CustomWaitable, ICancelable
    {
        private static uint _maxId;

        internal static NUpdateMode defaultUpdateMode = NUpdateMode.ScaleTime;

        public NTweener()
        {
            id = ++_maxId;
            updateMode = defaultUpdateMode;
        }

        public override int GetHashCode()
        {
            return NConverter.bitConvertWithoutChecks<uint, int>(id);
        }

        protected bool _isStarted;
        protected int _totalLoopCount = 0;
        protected int _remainingLoopCount = 0;
        protected float _startNormalizedTime;
        protected float _currentNormalizedTime;

        internal float duration;
        internal bool isFinalized;
        internal NUpdateMode updateMode;
        internal readonly uint id;
        internal float scheduledTime;
        internal float scheduledUnscaleTime;
        internal CancelControlKey controlKey;

        internal bool isScheduled => scheduledTime > 0;
        internal bool isLooping => _totalLoopCount != _remainingLoopCount;

        internal float delayTime;
        internal float loopInterval;
        internal float startTime
        {
            get
            {
                float t;
                if (isLooping)
                {
                    t = loopInterval + (updateMode == NUpdateMode.ScaleTime ? scheduledTime : scheduledUnscaleTime);
                }
                else
                {
                    t = delayTime + (updateMode == NUpdateMode.ScaleTime ? scheduledTime : scheduledUnscaleTime);
                }
                t -= _currentNormalizedTime * duration;
                return t;
            }
        }
        internal ChunkIndex chunkIndex;
        internal EaseType easeType;

        public RunState Status { get; private set; }
        public float Time { get; internal set; }
        public float NormalizedTime => duration == 0 ? 0 : Mathf.Clamp01(Time / duration);

        private Action _onStarted;
        private Action _onUpdated;
        private Action _onCompleted;
        private Action _onCanceled;
        private Action _onFinalized;

        private List<Func<bool>> cancelWhenFuncList;

        /// <summary>
        /// `loopCount` is -1 is infinite loop
        /// </summary>
        public NTweener setLoop(int loopCount)
        {
            if (Status.isFinished())
            {
                NDebug.LogWarning("Tweener has been finished");
                return this;
            }
            _remainingLoopCount = _totalLoopCount = loopCount;
            return this;
        }

        public NTweener setLoopInterval(float timeInSeconds)
        {
            if (timeInSeconds < 0)
            {
                NDebug.LogError($"{nameof(timeInSeconds)} must equal or greater than 0");
                return this;
            }
            this.loopInterval = timeInSeconds;
            return this;
        }

        public NTweener setDuration(float duration)
        {
            this.duration = duration;
            return this;
        }

        public NTweener setDelay(float timeInSeconds)
        {
            if (timeInSeconds < 0)
            {
                NDebug.LogError($"{nameof(timeInSeconds)} must equal or greater than 0");
                return this;
            }
            this.delayTime = timeInSeconds;
            return this;
        }

        public NTweener setEase(EaseType easeType)
        {
            this.easeType = easeType;
            return this;
        }

        /// <summary>
        /// Execute when NTweener is started
        /// </summary>
        public NTweener onStarted(Action onStarted)
        {
            if (Status >= RunState.Running)
            {
                NDebug.LogWarning("Tweener has been started");
            }
            this._onStarted += onStarted;
            return this;
        }
        /// <summary>
        /// Execute when NTweener is updated
        /// </summary>
        public NTweener onUpdated(Action onUpdated)
        {
            if (Status.isFinished())
            {
                NDebug.LogWarning("Tweener has been finished");
            }
            this._onUpdated += onUpdated;
            return this;
        }
        /// <summary>
        /// Execute when Status is [Completed]
        /// </summary>
        public NTweener onCompleted(Action onCompleted)
        {
            this._onCompleted += onCompleted;
            if (Status == RunState.Completed)
            {
                try
                {
                    onCompleted.Invoke();
                }
                catch (Exception e)
                {
                    NDebug.LogException(e);
                }
            }
            return this;
        }
        /// <summary>
        /// Execute when Status is [Canceled]
        /// </summary>
        public NTweener onCanceled(Action onCanceled)
        {
            this._onCanceled += onCanceled;
            if (Status == RunState.Canceled)
            {
                try
                {
                    onCanceled.Invoke();
                }
                catch (Exception e)
                {
                    NDebug.LogException(e);
                }
            }
            return this;
        }
        /// <summary>
        /// Execute when Status is [Canceled] or [Completed] or [Exception]
        /// </summary>
        public NTweener onFinalized(Action onFinalized)
        {
            if (Status.isFinished())
            {
                NDebug.LogWarning("Tweener has been finished");
            }
            _onFinalized += onFinalized;
            return this;
        }

        public NTweener cancelWhen(Func<bool> condition)
        {
            (cancelWhenFuncList ??= new List<Func<bool>>(1)).Add(condition);
            return this;
        }

        public NTweener setCancelControlKey(UnityEngine.Object target)
        {
            if (!target)
            {
                NDebug.LogError("Object target is null");
                return this;
            }
            __innerSetCancelControlKey(NTweenManager.createKey(target));
            return this;
        }

        public NTweener setCancelControlKey(uint uintKey)
        {
            __innerSetCancelControlKey(NTweenManager.createKey(uintKey));
            return this;
        }

        public NTweener setUpdateMode(NUpdateMode updateMode)
        {
            if (Status != RunState.None)
            {
                NDebug.LogWarning("Tweener has been started, update mode will not be changed");
            }
            else
            {
                this.updateMode = updateMode;
            }
            return this;
        }

        /// <summary>
        /// Stop tweener
        /// </summary>
        public void cancel()
        {
            if (Status.isFinished())
            {
                return;
            }
            Status = RunState.Canceled;
            if (chunkIndex.chunkId != 0)
            {
                NTweenManager.cancelFromTweener(this);
            }
            try
            {
                _onCanceled?.Invoke();
            }
            catch (Exception e)
            {
                NDebug.LogException(e);
            }
            finally
            {
                __invokeOnFinalize();
            }
        }

        public void removeAllEvents()
        {
            _onStarted = null;
            _onUpdated = null;
            _onCompleted = null;
            _onCanceled = null;
            _onFinalized = null;
        }

        public void stopAndResetState()
        {
            cancel();
            resetState();
        }

        public void removeCancelControlKey()
        {
            __innerRemoveCancelControlKey();
        }

        private void __innerSetCancelControlKey(CancelControlKey cancelControlKey)
        {
            if (!controlKey.isDefault())
            {
                NTweenManager.removeControlledTweener(this);
            }
            controlKey = cancelControlKey;
            NTweenManager.addCancelControlledTweener(this);
        }

        private void __innerRemoveCancelControlKey()
        {
            if (!controlKey.isDefault())
            {
                NTweenManager.removeControlledTweener(this);
                controlKey = default;
            }
        }

        internal bool isCanceledFromFunc()
        {
            if (cancelWhenFuncList == null)
            {
                return false;
            }
            foreach (var condition in cancelWhenFuncList.asSpan())
            {
                if (condition())
                {
                    cancel();
                    return true;
                }
            }
            return false;
        }

        internal void run()
        {
            if (Status == RunState.None && !isScheduled)
            {
                scheduledTime = UnityEngine.Time.time;
                scheduledUnscaleTime = UnityEngine.Time.unscaledTime;
                NTweenManager.run(this);
            }
        }

        internal void schedule()
        {
            if (Status == RunState.None && !isScheduled)
            {
                scheduledTime = UnityEngine.Time.time;
                scheduledUnscaleTime = UnityEngine.Time.unscaledTime;
                NTweenManager.schedule(this);
            }
        }

        internal void resetState()
        {
            isFinalized = false;
            Status = RunState.None;
            scheduledTime = 0;
            scheduledUnscaleTime = 0;
            Time = 0;
            chunkIndex = default;

            onResetState();
        }

        /// <summary>
        /// Cancel an start new tween
        /// </summary>
        public void restart()
        {
            _remainingLoopCount = _totalLoopCount;
            _isStarted = false;
            resetState();
            run();
        }

        public NTweener startNormalizedTime(float normalizedTime)
        {
            if (Status != RunState.None)
            {
                NDebug.LogWarning("Tweener has been started, update mode will not be changed");
            }
            this._startNormalizedTime = Mathf.Clamp01(normalizedTime);
            return this;
        }

        internal void invokeOnStart()
        {
            if (Status >= RunState.Running)
            {
                return;
            }

            Status = RunState.Running;

            if (!_isStarted)
            {
                _isStarted = true;
                try
                {
                    _onStarted?.Invoke();
                }
                catch (Exception e)
                {
                    NDebug.LogException(e);
                }
            }
        }
        internal void invokeOnUpdate()
        {
            try
            {
                if (!isCanceledFromFunc())
                {
                    _onUpdated?.Invoke();
                }
            }
            catch (Exception e)
            {
                NDebug.LogException(e);
            }
        }
        internal void invokeOnComplete()
        {
            if (Status.isFinished())
            {
                return;
            }

            if (_totalLoopCount == -1)
            {
                _remainingLoopCount = 0;
                resetState();
                run();
                return;
            }

            if (_remainingLoopCount >= 1)
            {
                _remainingLoopCount--;
                resetState();
                run();
                return;
            }

            Status = RunState.Completed;
            try
            {
                _onCompleted?.Invoke();
            }
            catch (Exception e)
            {
                NDebug.LogException(e);
            }
            finally
            {
                __invokeOnFinalize();
            }
        }
        private void __invokeOnFinalize()
        {
            if (!isFinalized)
            {
                isFinalized = true;
                try
                {
                    _onFinalized?.Invoke();
                }
                catch (Exception e)
                {
                    NDebug.LogException(e);
                }
                finally
                {
                    __innerRemoveCancelControlKey();
                }
            }
        }

        protected virtual void onResetState() { }

        internal void invokeBeforeStart()
        {
            // update _currentNormalizedTime at first run
            if (_isStarted)
            {
                _currentNormalizedTime = 0;
            }
            else
            {
                _currentNormalizedTime = _startNormalizedTime;
            }
        }
        internal virtual void forceComplete() { }
        internal CommonJobData getCommonJobData()
        {
            return new CommonJobData(updateMode, easeType, startTime, duration);
        }

        internal unsafe abstract void invokeValueChanged(void* src);
        internal unsafe abstract void writeJobDataToAddr(void* dst);
        internal abstract ushort getRunnerId();

        public override NWaitableState getCurrentState()
        {
            return getWaitableResult();
        }

        internal NWaitableState getWaitableResult()
        {
            return Status switch
            {
                RunState.Completed => NWaitableState.Completed,
                RunState.Canceled => NWaitableState.Canceled,
                _ => NWaitableState.None,
            };
        }
    }

    internal interface ITransformTweener
    {
        Transform Target { get; }
    }
    internal interface ITransform2Tweener
    {
        Transform Target { get; }
        Transform Dst { get; }
    }
}

