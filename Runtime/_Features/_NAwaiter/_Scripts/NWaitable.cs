using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Nextension
{
    [AsyncMethodBuilder(typeof(AsyncWaitableBuilder))]
    public class NWaitable : CustomYieldInstruction, IWaitable
    {
        private List<ICancellable> cancellables = new List<ICancellable>();
        private Action onCompleted;
        private Action onCancelled;

        public bool isThrowException = true;
        public RunState Status { get; protected internal set; }
        public Exception Exception { get; protected internal set; }
        public override bool keepWaiting => !isFinished();
        bool IWaitable.IsWaitable => !isFinished();

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
                onCancelled = null;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
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
                    onCompleted = null;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }
        internal void setException(Exception e)
        {
            if (Status.isFinished())
            {
                return;
            }
            setState(RunState.Exception);
            Exception = e;
            if (isThrowException)
            {
                throw e;
            }
        }

        Func<CompleteState> IWaitable.buildCompleteFunc()
        {
            Func<CompleteState> func = () =>
            {
                switch (Status)
                {
                    case RunState.Completed: return CompleteState.Completed;
                    case RunState.Cancelled: return CompleteState.Cancelled;
                    case RunState.Exception: return CompleteState.Exception;
                    default: return CompleteState.None;
                }
            };
            return func;
        }
    }
    [AsyncMethodBuilder(typeof(AsyncWaitableBuilder<>))]
    public class NWaitable<T> : NWaitable
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

    }
}
