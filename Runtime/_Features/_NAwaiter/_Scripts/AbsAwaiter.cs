using System;
using System.Runtime.CompilerServices;

namespace Nextension
{
    public class AbsAwaiter : INotifyCompletion
    {
        protected Action continuation;
        protected Exception exception;

        public void OnCompleted(Action continuation)
        {
            if (IsCompleted)
            {
                continuation.Invoke();
                onInnerCompleted();
            }
            else
            {
                this.continuation = continuation;
            }
        }
        public void GetResult()
        {
            onInnerGetResult();
        }

        public bool IsCompleted { get; private set; }

        internal void invokeComplete()
        {
            if (!IsCompleted)
            {
                IsCompleted = true;
                if (continuation != null)
                {
                    continuation.Invoke();
                    onInnerCompleted();
                }
            }
        }
        internal void invokeException(Exception ex)
        {
            if (!IsCompleted)
            {
                exception = ex;
                invokeComplete();
            }
        }

        protected virtual void onInnerGetResult()
        {
            if (exception != null)
            {
                throw exception;
            }
        }
        protected virtual void onInnerCompleted()
        {

        }
    }
}
