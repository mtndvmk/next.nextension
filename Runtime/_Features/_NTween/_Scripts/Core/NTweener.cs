using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Nextension.Tween
{
    public abstract class NTweener : ICancelable
    {
        private static uint _maxId;
        public NTweener()
        {
            id = ++_maxId;
        }

        public override int GetHashCode()
        {
            return NConverter.bitConvert<uint, int>(id);
        }

        internal readonly uint id;
        internal bool isFinalized;
        public RunState Status { get; private set; }
        internal float scheduledTime;
        internal CancelControlKey controlKey;

        internal bool isScheduled => scheduledTime > 0;

        private Action onStartedEvent;
        private Action onUpdatedEvent;
        private Action onCompletedEvent;
        private Action onCanceledEvent;
        private Action onFinalizedEvent;

        private List<Func<bool>> cancelWhenFuncList;

        public async NWaitable waitFinalized()
        {
            await new NWaitUntil(() => isFinalized);
        }
        public async NWaitable waitCompleted()
        {
            await new NWaitUntil(() => Status == RunState.Completed);
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
            try
            {
                onInnerCanceled();
                onCanceledEvent?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                invokeOnFinalize();
            }
        }
        /// <summary>
        /// Execute when NTweener is started
        /// </summary>
        public NTweener onStarted(Action onStarted)
        {
            if (Status >= RunState.Running)
            {
                Debug.LogWarning("Tweener has been started");
            }
            else
            {
                this.onStartedEvent += onStarted;
            }
            return this;
        }
        /// <summary>
        /// Execute when NTweener is updated
        /// </summary>
        public NTweener onUpdated(Action onUpdated)
        {
            if (Status.isFinished())
            {
                Debug.LogWarning("Tweener has been finished");
            }
            else
            {
                this.onUpdatedEvent += onUpdated;
            }
            return this;
        }
        /// <summary>
        /// Execute when Status is [Completed]
        /// </summary>
        public NTweener onCompleted(Action onCompleted)
        {
            if (Status == RunState.Completed)
            {
                try
                {
                    onCompleted.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            else if (!Status.isFinished())
            {
                this.onCompletedEvent += onCompleted;
            }
            return this;
        }
        /// <summary>
        /// Execute when Status is [Canceled]
        /// </summary>
        public NTweener onCanceled(Action onCanceled)
        {
            if (Status == RunState.Canceled)
            {
                try
                {
                    onCanceled.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            else if (!Status.isFinished())
            {
                this.onCanceledEvent += onCanceled;
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
                Debug.LogWarning("Tweener has been finished");
            }
            else
            {
                onFinalizedEvent += onFinalized;
            }
            return this;
        }
        public NTweener cancelWhen(Func<bool> condition)
        {
            (cancelWhenFuncList ??= new List<Func<bool>>()).Add(condition);
            return this;
        }
        public void setCancelControlKey(UnityEngine.Object target)
        {
            if (!target)
            {
                Debug.LogError("Object target is null");
                return;
            }
            innerSetCancelControlKey(NTweenManager.createKey(target));
        }
        public void setCancelControlKey(uint uintKey)
        {
            innerSetCancelControlKey(NTweenManager.createKey(uintKey));
        }
        private void innerSetCancelControlKey(CancelControlKey cancelControlKey)
        {
            if (!controlKey.isDefault())
            {
                NTweenManager.removeControlledTweener(this);
            }
            controlKey = cancelControlKey;
            NTweenManager.addCancelControlledTweener(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void removeCancelControlKey()
        {
            innerRemoveCancelControlKey();
        }

        private void innerRemoveCancelControlKey()
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
                scheduledTime = Time.time;
                onRun();
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void schedule()
        {
            if (Status == RunState.None && !isScheduled)
            {
                scheduledTime = Time.time;
                onSchedule();
            }
        }
        internal void resetState()
        {
            Status = RunState.None;
            scheduledTime = 0;
            onResetState();
        }
        public void stopAndResetState()
        {
            cancel();
            resetState();
        }
        public void restart()
        {
            stopAndResetState();
            run();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void invokeOnStart()
        {
            if (Status >= RunState.Running)
            {
                return;
            }
            Status = RunState.Running;

            try
            {
                onInnerStarted();
                onStartedEvent?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void invokeOnUpdate()
        {
            try
            {
                if (!isCanceledFromFunc())
                {
                    onUpdatedEvent?.Invoke();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void invokeOnComplete()
        {
            if (Status.isFinished())
            {
                return;
            }
            Status = RunState.Completed;
            try
            {
                onCompletedEvent?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                invokeOnFinalize();
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void invokeOnFinalize()
        {
            if (!isFinalized)
            {
                isFinalized = true;
                try
                {
                    onFinalizedEvent?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                finally
                {
                    innerRemoveCancelControlKey();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void onInnerStarted() { }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void onInnerCanceled() { }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void onResetState() { }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual void forceComplete() { }

        protected abstract void onRun();
        protected abstract void onSchedule();
    }
    public abstract class GenericNTweener<T> : NTweener where T : NTweener
    {
        private readonly T _tweener;

        internal float delayTime;
        internal float startTime => scheduledTime + delayTime;

        internal GenericNTweener()
        {
            _tweener = this as T;
        }

        public T setDelay(float delayTime)
        {
            if (delayTime < 0)
            {
                Debug.LogError($"{nameof(delayTime)} must equal or greater than 0");
                return _tweener;
            }
            this.delayTime = delayTime;
            return _tweener;
        }
        public new T onStarted(Action onStarted)
        {
            base.onStarted(onStarted);
            return _tweener;
        }
        /// <summary>
        /// Execute when NTweener is updated
        /// </summary>
        public new T onUpdated(Action onUpdated)
        {
            base.onUpdated(onUpdated);
            return _tweener;
        }
        /// <summary>
        /// Execute when Status is [Completed]
        /// </summary>
        public new T onCompleted(Action onCompleted)
        {
            base.onCompleted(onCompleted);
            return _tweener;
        }
        /// <summary>
        /// Execute when Status is [Canceled]
        /// </summary>
        public new T onCanceled(Action onCanceled)
        {
            base.onCanceled(onCanceled);
            return _tweener;
        }
        /// <summary>
        /// Execute when Status is [Canceled] or [Completed] or [Exception]
        /// </summary>
        public new T onFinalized(Action onFinalized)
        {
            base.onFinalized(onFinalized);
            return _tweener;
        }
        public new T cancelWhen(Func<bool> condition)
        {
            base.cancelWhen(condition);
            return _tweener;
        }
    }
    public class CombinedNTweener : GenericNTweener<CombinedNTweener>
    {
        private readonly NTweener[] _tweeners;
        private readonly List<uint> _updatedTweeners;
        private int _finalizedCount;

        public CombinedNTweener(params NTweener[] tweeners)
        {
            _tweeners = tweeners;
            _updatedTweeners = new(_tweeners.Length);
        }

        protected override void onInnerStarted()
        {
            base.onInnerStarted();
            foreach (var tweener in _tweeners)
            {
                tweener.onUpdated(() => onTweenerUpdated(tweener));
                tweener.onFinalized(onTweenerFinalized);
                tweener.schedule();
            }
        }
        protected override void onInnerCanceled()
        {
            base.onInnerCanceled();
            foreach (var tweener in _tweeners)
            {
                tweener.cancel();
            }
        }
        protected override void onResetState()
        {
            base.onResetState();
            foreach (var tweener in _tweeners)
            {
                tweener.resetState();
            }
        }
        protected override void onRun()
        {
            NTweenManager.run(this);
        }
        protected override void onSchedule()
        {
            NTweenManager.schedule(this);
        }
        private void onTweenerUpdated(NTweener tweener)
        {
            if (_updatedTweeners.addIfNotPresent(tweener.id))
            {
                if (_updatedTweeners.Count == _tweeners.Length - _finalizedCount)
                {
                    _updatedTweeners.Clear();
                    invokeOnUpdate();
                }
            }
        }
        private void onTweenerFinalized()
        {
            if (++_finalizedCount >= _tweeners.Length)
            {
                invokeOnComplete();
            }
        }
    }
}
