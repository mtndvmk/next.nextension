using System;
using System.Runtime.CompilerServices;

namespace Nextension
{
    public class AbsAwaiter : INotifyCompletion
    {
        protected Action continuation;
        public void OnCompleted(Action continuation)
        {
            this.continuation = continuation;
        }

        public bool IsCompleted { get; private set; }

        protected virtual void invokeComplete()
        {
            if (!IsCompleted)
            {
                IsCompleted = true;
                continuation?.Invoke();
            }
        }
        public void GetResult() { }
    }
}
