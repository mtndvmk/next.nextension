using System;
using System.Runtime.CompilerServices;

namespace Nextension
{
    public class AsyncWaitableBuilder
    {
        private NWaitable waitable;
        public NWaitable Task => waitable;
        public AsyncWaitableBuilder() { waitable = new NWaitable(); }
        public static AsyncWaitableBuilder Create()
        {
            var builder = new AsyncWaitableBuilder();
            return builder;
        }
        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
        }
        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {

        }
        public void SetResult()
        {
            waitable.setState(RunState.Completed);
        }
        public void SetException(Exception exception)
        {
            waitable.setException(exception);
        }
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            if (waitable.Status.isFinished())
            {
                return;
            }
            awaiter.OnCompleted(stateMachine.MoveNext);
            if (awaiter is ICancellable)
            {
                waitable.addCancelleable(awaiter as ICancellable);
            }
        }
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            if (waitable.Status == RunState.Cancelled)
            {
                return;
            }
            awaiter.UnsafeOnCompleted(stateMachine.MoveNext);
            if (awaiter is ICancellable)
            {
                waitable.addCancelleable(awaiter as ICancellable);
            }
        }
    }
    public class AsyncWaitableBuilder<T>
    {
        private NWaitable<T> waitable;
        public NWaitable<T> Task => waitable;
        public AsyncWaitableBuilder() { waitable = new NWaitable<T>(); }
        public static AsyncWaitableBuilder<T> Create()
        {
            var builder = new AsyncWaitableBuilder<T>();
            return builder;
        }
        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
        }
        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {

        }
        public void SetResult(T result)
        {
            waitable.setResultAndComplete(result);
        }
        public void SetException(Exception exception)
        {
            waitable.setException(exception);
        }
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            if (waitable.Status.isFinished())
            {
                return;
            }
            awaiter.OnCompleted(stateMachine.MoveNext);
            if (awaiter is ICancellable)
            {
                waitable.addCancelleable(awaiter as ICancellable);
            }
        }
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            if (waitable.Status == RunState.Cancelled)
            {
                return;
            }
            awaiter.UnsafeOnCompleted(stateMachine.MoveNext);
            if (awaiter is ICancellable)
            {
                waitable.addCancelleable(awaiter as ICancellable);
            }
        }
    }
}
