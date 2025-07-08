﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Nextension.Tween
{
    public abstract class NTweener : ICancelable
    {
        public enum UpdateMode : byte
        {
            ScaleTime,
            UnscaledTime,
        }

        private static uint _maxId;

        internal static UpdateMode defaultUpdateMode = UpdateMode.ScaleTime;

        public NTweener()
        {
            id = ++_maxId;
            updateMode = defaultUpdateMode;
        }

        public override int GetHashCode()
        {
            return NConverter.bitConvertWithoutChecks<uint, int>(id);
        }

        internal readonly uint id;
        internal bool isFinalized;
        internal UpdateMode updateMode;
        internal float scheduledTime;
        internal float scheduledUnscaleTime;
        internal CancelControlKey controlKey;

        internal bool isScheduled => scheduledTime > 0;
        public RunState Status { get; private set; }

        private Action onStartedEvent;
        private Action onUpdatedEvent;
        private Action onCompletedEvent;
        private Action onCanceledEvent;
        private Action onFinalizedEvent;

        private List<Func<bool>> cancelWhenFuncList;

        public async NWaitable waitTweener()
        {
            await new NWaitTweener(this);
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
            this.onStartedEvent += onStarted;
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
            this.onUpdatedEvent += onUpdated;
            return this;
        }
        /// <summary>
        /// Execute when Status is [Completed]
        /// </summary>
        public NTweener onCompleted(Action onCompleted)
        {
            this.onCompletedEvent += onCompleted;
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
            return this;
        }
        /// <summary>
        /// Execute when Status is [Canceled]
        /// </summary>
        public NTweener onCanceled(Action onCanceled)
        {
            this.onCanceledEvent += onCanceled;
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
            onFinalizedEvent += onFinalized;
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
                Debug.LogError("Object target is null");
                return this;
            }
            innerSetCancelControlKey(NTweenManager.createKey(target));
            return this;
        }
        public NTweener setCancelControlKey(uint uintKey)
        {
            innerSetCancelControlKey(NTweenManager.createKey(uintKey));
            return this;
        }
        public NTweener setUpdateMode(UpdateMode updateMode)
        {
            if (Status != RunState.None)
            {
                Debug.LogWarning("Tweener has been started, update mode will not be changed");
            }
            else
            {
                this.updateMode = updateMode;
            }
            return this;
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
                scheduledUnscaleTime = Time.unscaledTime;
                onRun();
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void schedule()
        {
            if (Status == RunState.None && !isScheduled)
            {
                scheduledTime = Time.time;
                scheduledUnscaleTime = Time.unscaledTime;
                onSchedule();
            }
        }

        internal void resetState()
        {
            isFinalized = false;
            Status = RunState.None;
            scheduledTime = 0;
            scheduledUnscaleTime = 0;
            onResetState();
        }
        public void removeAllEvents()
        {
            onStartedEvent = null;
            onUpdatedEvent = null;
            onCompletedEvent = null;
            onCanceledEvent = null;
            onFinalizedEvent = null;
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
        internal float startTime => delayTime + (updateMode == UpdateMode.ScaleTime ? scheduledTime : scheduledUnscaleTime);

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
        public new T setUpdateMode(UpdateMode updateMode)
        {
            base.setUpdateMode(updateMode);
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
