using System;

namespace Nextension
{
    public struct NWaitableResult
    {
        public CompleteState state;
        public KeepStackTraceException exception;

        public readonly static NWaitableResult None = new() { state = CompleteState.None };
        public readonly static NWaitableResult Canceled = new() { state = CompleteState.Canceled };
        public readonly static NWaitableResult Completed = new() { state = CompleteState.Completed };

        public static NWaitableResult Exception(Exception exception) => new()
        {
            state = CompleteState.Exception,
            exception = new KeepStackTraceException(exception)
        };

        public readonly static Func<NWaitableResult> CompletedFunc = () => Completed;
    }
}
