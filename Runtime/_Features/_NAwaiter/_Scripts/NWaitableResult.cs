using System;

namespace Nextension
{
    public struct NWaitableResult
    {
        public CompleteState state;
        public KeepStackTraceException exception;

        public static NWaitableResult None = new NWaitableResult() { state = CompleteState.None };
        public static NWaitableResult Cancelled = new NWaitableResult() { state = CompleteState.Cancelled };
        public static NWaitableResult Completed = new NWaitableResult() { state = CompleteState.Completed };
        public static NWaitableResult Exception(Exception exception) => new NWaitableResult() { state = CompleteState.Exception, exception = new KeepStackTraceException(exception) };
    }
}
