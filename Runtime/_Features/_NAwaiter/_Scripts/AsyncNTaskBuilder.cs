using System;
using System.Runtime.CompilerServices;

namespace Nextension
{

    public struct AsyncNTaskBuilder
    {
        private readonly NTask _task;
        public readonly NTask Task => _task;
        public AsyncNTaskBuilder(NTask task)
        {
            _task = task;
        }
        public static AsyncNTaskBuilder Create()
        {
            var id = NTaskManager.nextId();
            return new AsyncNTaskBuilder(new NTask(id));
        }
        public readonly void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
        }
        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {

        }
        public void SetResult()
        {
            NTaskManager.setTaskResult(_task.id, NWaitableState.Completed);
        }
        public void SetException(Exception exception)
        {
            var state = exception is OperationCanceledException
                ? NWaitableState.Canceled
                : NWaitableState.Exception(exception);
            NTaskManager.setTaskResult(_task.id, state);
        }

        private StateMachineBox<TStateMachine> getStateMachineBox<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            var stateMachineBox = StateMachineBox<TStateMachine>.create();
            stateMachineBox.setStateMachine(stateMachine);
            return stateMachineBox;
        }

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            var stateMachineBox = getStateMachineBox(ref stateMachine);
            awaiter.OnCompleted(stateMachineBox.MoveNext);
            if (awaiter is ICancelable cancelableAwaiter)
            {
                NTaskManager.setNextCancelable(_task.id, cancelableAwaiter);
            }
        }
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            var stateMachineBox = getStateMachineBox(ref stateMachine);
            awaiter.OnCompleted(stateMachineBox.MoveNext);
            if (awaiter is ICancelable cancelableAwaiter)
            {
                NTaskManager.setNextCancelable(_task.id, cancelableAwaiter);
            }
        }
    }
    public struct AsyncNTaskBuilder<T>
    {
        private readonly NTask<T> _task;
        public readonly NTask<T> Task => _task;
        public AsyncNTaskBuilder(NTask<T> task)
        {
            _task = task;
        }
        public static AsyncNTaskBuilder<T> Create()
        {
            var id = NTaskManager.nextId();
            var builder = new AsyncNTaskBuilder<T>(new NTask<T>(id));
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
            NTaskGenericManager.setTaskResult(_task.id, NWaitableState<T>.Completed(result));
        }
        public void SetException(Exception exception)
        {
            var state = exception is OperationCanceledException
                ? NWaitableState<T>.Canceled
                : NWaitableState<T>.Exception(exception);
            NTaskGenericManager.setTaskResult<T>(_task.id, state);
        }
        private StateMachineBox<TStateMachine> getStateMachineBox<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            var stateMachineBox = StateMachineBox<TStateMachine>.create();
            stateMachineBox.setStateMachine(stateMachine);
            return stateMachineBox;
        }

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            var stateMachineBox = getStateMachineBox(ref stateMachine);
            awaiter.OnCompleted(stateMachineBox.MoveNext);
            if (awaiter is ICancelable cancelableAwaiter)
            {
                NTaskManager.setNextCancelable(_task.id, cancelableAwaiter);
            }
        }
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            var stateMachineBox = getStateMachineBox(ref stateMachine);
            awaiter.OnCompleted(stateMachineBox.MoveNext);
            if (awaiter is ICancelable cancelableAwaiter)
            {
                NTaskManager.setNextCancelable(_task.id, cancelableAwaiter);
            }
        }
    }

    internal class StateMachineBox<TStateMachine> where TStateMachine : IAsyncStateMachine
    {
        private TStateMachine _stateMachine;
        private bool _hasStateMachine;
        public Action MoveNext { get; private set; }
        private StateMachineBox()
        {
            MoveNext = __moveNext;
        }
        private void __moveNext()
        {
            if (_hasStateMachine)
            {
                try
                {
                    _stateMachine.MoveNext();
                }
                finally
                {
                    release();
                }
            }
        }

        public void setStateMachine(TStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
            _hasStateMachine = true;
        }

        public static StateMachineBox<TStateMachine> create()
        {
            return NLockedPool<StateMachineBox<TStateMachine>>.get();
        }
        public void release()
        {
            _stateMachine = default;
            _hasStateMachine = false;
            NLockedPool<StateMachineBox<TStateMachine>>.release(this);
        }
    }
}