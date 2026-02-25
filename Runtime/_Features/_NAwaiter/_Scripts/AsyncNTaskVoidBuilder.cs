using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Nextension
{
    public readonly struct AsyncNTaskVoidBuilder
    {
        private readonly NTaskVoid _task;
        public readonly NTaskVoid Task => _task;
        public static AsyncNTaskVoidBuilder Create()
        {
            return default;
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

        }
        public void SetException(Exception exception)
        {
            Debug.LogException(exception);
        }
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            awaiter.OnCompleted(stateMachine.MoveNext);
        }
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            awaiter.UnsafeOnCompleted(stateMachine.MoveNext);
        }
    }
}