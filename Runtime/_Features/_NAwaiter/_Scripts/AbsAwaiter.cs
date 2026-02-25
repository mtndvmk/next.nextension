using System;
using System.Runtime.CompilerServices;

namespace Nextension
{
    public abstract class AbsAwaiter : INotifyCompletion
    {
        protected readonly static InterlockedId _idCounter = new InterlockedId();

        protected bool _isCompletedAwaiter;
        protected Action _continuation;
        protected NWaitableState _currentState;

        public uint Id { get; private set; }

        public void OnCompleted(Action continuation)
        {
            this._continuation = continuation;
        }
        public void GetResult()
        {
            try
            {
                __beforeGetResult();
            }
            finally
            {
                if (_isCompletedAwaiter)
                {
                    __onFinalized();
                }
            }
        }

        public bool IsCompleted => _currentState.state.isFinished();

        protected void updateId()
        {
            Id = _idCounter.nextId();
        }

        internal void setCompletion(uint id, NWaitableState state)
        {
            if (Id != id || Id == 0)
            {
                return;
            }
            if (!IsCompleted)
            {
                _currentState = state;
                __continue();
            }
        }

        protected void setAsCompletedAwaiter(NWaitableState state)
        {
            _isCompletedAwaiter = true;
            setCompletion(Id, state);
        }

        private void __continue()
        {
            if (_continuation == null) return;
            try
            {
                _continuation.Invoke();
            }
            finally
            {
                __onFinalized();
            }
        }

        protected void __beforeGetResult()
        {
            if (_currentState.state == CompleteState.Canceled)
            {
                throw NExceptionHelper.CanceledException;
            }
            if (_currentState.exception != null)
            {
                throw _currentState.exception;
            }
        }
        protected virtual void __onFinalized()
        {
            if (Id != 0)
            {
                _isCompletedAwaiter = false;
                _continuation = null;
                _currentState = default;
                Id = 0;
            }
        }
    }
}
