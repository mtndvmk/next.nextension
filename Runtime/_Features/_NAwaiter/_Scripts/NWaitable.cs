using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Nextension
{
    public abstract class AbsNWaitable : CustomYieldInstruction, IWaitable, ICancellable
    {
        private List<ICancellable> cancellables = new List<ICancellable>();
        private Action onCompleted;
        private Action onCancelled;
        private Action onFinalized;
        private bool isFinalized;

        public RunState Status { get; protected internal set; }
        public Exception Exception { get; protected internal set; }
        public override bool keepWaiting => !isFinalized;

        WaiterLoopType IWaitable.LoopType => WaiterLoopType.Update;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool isFinished() => Status.isFinished();
        public void cancel()
        {
            if (isFinished())
            {
                return;
            }
            setState(RunState.Cancelled);
            foreach (var c in cancellables)
            {
                c.cancel();
            }
            try
            {
                onCancelled?.Invoke();
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
                    onCompleted?.Invoke();
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
        public void removeCompletedEvent(Action onCompleted)
        {
            this.onCompleted -= onCompleted;
        }
        public void addCancelledEvent(Action onCancelled)
        {
            if (Status == RunState.Cancelled)
            {
                try
                {
                    onCancelled?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            else if (Status <= RunState.Running)
            {
                this.onCancelled += onCancelled;
            }
        }
        public void removeCancelledEvent(Action onCancelled)
        {
            this.onCancelled -= onCancelled;
        }
        public void addFinalizedEvent(Action onFinalized)
        {
            if (isFinalized)
            {
                try
                {
                    onFinalized?.Invoke();
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
        public void removeFinalizedEvent(Action onFinalized)
        {
            this.onFinalized -= onFinalized;
        }

        private void invokeFinalizedEvent()
        {
            if (!isFinalized)
            {
                isFinalized = true;
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

        internal void addCancelleable(ICancellable cancellable)
        {
            if (Status.isFinished())
            {
                return;
            }
            cancellables.Add(cancellable);
        }
        internal void setState(RunState runState)
        {
            if (Status.isFinished())
            {
                return;
            }
            Status = runState;
            if (Status == RunState.Completed)
            {
                try
                {
                    onCompleted?.Invoke();
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
            setState(RunState.Exception);
            Exception = e;
        }
        internal void clearEvent()
        {
            onCancelled = null;
            onCompleted = null;
        }

        Func<NWaitableResult> IWaitable.buildCompleteFunc()
        {
            Func<NWaitableResult> func = () =>
            {
                switch (Status)
                {
                    case RunState.Completed: return NWaitableResult.Completed;
                    case RunState.Cancelled: return NWaitableResult.Cancelled;
                    case RunState.Exception: return NWaitableResult.Exception(Exception);
                    default: return NWaitableResult.None;
                }
            };
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
        private static NWaitable _completeWaitable;
        public static NWaitable CompletedWaitable
        {
            get
            {
                if (_completeWaitable == null) _completeWaitable = new NWaitable() { Status = RunState.Completed };
                return _completeWaitable;
            }
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
