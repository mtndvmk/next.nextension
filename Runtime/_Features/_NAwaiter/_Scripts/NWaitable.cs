using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Nextension
{
    public abstract class AbsNWaitable : CustomYieldInstruction, IWaitable, ICancelable
    {
        private List<ICancelable> _cancelables;
        private Action onCompleted;
        private Action onCanceled;
        private Action onFinalized;
        private bool _isFinalized;

        public RunState Status { get; protected internal set; }
        public Exception Exception { get; protected internal set; }
        public override bool keepWaiting => !_isFinalized;

        NLoopType IWaitable.LoopType => NLoopType.Update;
        bool IWaitable.IsIgnoreFirstFrameCheck => true;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool isFinished() => Status.isFinished();
        public void cancel()
        {
            if (isFinished())
            {
                return;
            }
            setState(RunState.Canceled);
            if (_cancelables != null)
            {
                var span = _cancelables.asSpan();
                for (int i = span.Length - 1; i >= 0; i--)
                {
                    span[i].cancel();
                }
            }
            try
            {
                onCanceled?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            invokeFinalizedEvent();
        }
        public void addCompletedEvent(Action onCompleted)
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
            else if (Status <= RunState.Running)
            {
                this.onCompleted += onCompleted;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void removeCompletedEvent(Action onCompleted)
        {
            this.onCompleted -= onCompleted;
        }
        public void addCanceledEvent(Action onCanceled)
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
            else if (Status <= RunState.Running)
            {
                this.onCanceled += onCanceled;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void removeCanceledEvent(Action onCanceled)
        {
            this.onCanceled -= onCanceled;
        }
        public void addFinalizedEvent(Action onFinalized)
        {
            if (_isFinalized)
            {
                try
                {
                    onFinalized.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            else
            {
                this.onFinalized += onFinalized;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void removeFinalizedEvent(Action onFinalized)
        {
            this.onFinalized -= onFinalized;
        }

        private void invokeFinalizedEvent()
        {
            if (!_isFinalized)
            {
                _isFinalized = true;
                try
                {
                    onFinalized?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                finally
                {
                    clearEvent();
                }
            }
        }

        internal void addCancelable(ICancelable cancellable)
        {
            if (Status.isFinished())
            {
                return;
            }
            (_cancelables ??= new(1)).Add(cancellable);
        }
        internal void setState(RunState runState)
        {
            if (Status.isFinished())
            {
                return;
            }
            Status = runState;
            if (Status == RunState.Completed && onCompleted != null)
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
            invokeFinalizedEvent();
        }
        internal void setException(Exception e)
        {
            if (Status.isFinished())
            {
                return;
            }
            Exception = e;
            setState(RunState.Exception);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void clearEvent()
        {
            onCanceled = null;
            onCompleted = null;
            onFinalized = null;
        }

        Func<NWaitableResult> IWaitable.buildCompleteFunc()
        {
            NWaitableResult func()
            {
                return Status switch
                {
                    RunState.Completed => NWaitableResult.Completed,
                    RunState.Canceled => NWaitableResult.Canceled,
                    RunState.Exception => NWaitableResult.Exception(Exception),
                    _ => NWaitableResult.None,
                };
            }
            return func;
        }
        public async NWaitable waitCompleteOrException()
        {
            await new NWaitUntil(() => Status == RunState.Completed || Status == RunState.Exception);
        }
        public async NWaitable waitFinish()
        {
            await new NWaitUntil(() => Status.isFinished());
        }
    }
    [AsyncMethodBuilder(typeof(AsyncWaitableBuilder))]
    public class NWaitable : AbsNWaitable
    {
        private static NWaitable _completedWaitable;
        public static NWaitable CompletedWaitable => _completedWaitable ??= new NWaitable() { Status = RunState.Completed };
        public static NWaitable run(Action action)
        {
            var waitable = new NWaitMainThread().startWaitable();
            waitable.addCompletedEvent(action);
            return waitable;
        }
    }
    [AsyncMethodBuilder(typeof(AsyncWaitableBuilder<>))]
    public class NWaitable<T> : AbsNWaitable
    {
        public T Result { get; protected set; }
        internal void setResultAndComplete(T result)
        {
            if (isFinished())
            {
                return;
            }
            Result = result;
            setState(RunState.Completed);
        }
        public static NWaitable<T> fromResult(T result) { return new NWaitable<T>() { Status = RunState.Completed, Result = result }; }
    }
}
