using System;

namespace Nextension
{
    public readonly struct NWaitableState
    {
        public readonly CompleteState state;
        public readonly Exception exception;

        private NWaitableState(CompleteState state, Exception exception)
        {
            this.state = state;
            if (state == CompleteState.Canceled)
            {
                this.exception = exception;
            }
            else if (exception == null)
            {
                this.exception = null;
            }
            else
            {
                this.exception = new KeepStackTraceException(exception);
            }
        }

        public override string ToString()
        {
            return $"State={state}, Exception={exception}";
        }

        public readonly static NWaitableState None = new NWaitableState(CompleteState.None, default);
        public readonly static NWaitableState Canceled = new NWaitableState(CompleteState.Canceled, NExceptionHelper.CanceledException);
        public readonly static NWaitableState Completed = new NWaitableState(CompleteState.Completed, null);
        public static NWaitableState Exception(Exception exception) => new NWaitableState(CompleteState.Exception, exception);
    }

    public readonly struct NWaitableState<T>
    {
        public readonly CompleteState state;
        public readonly Exception exception;
        public readonly T result;

        private NWaitableState(CompleteState state, Exception exception, T result)
        {
            this.state = state;
            if (state == CompleteState.Canceled)
            {
                this.exception = exception;
            }
            else if (exception == null)
            {
                this.exception = null;
            }
            else
            {
                this.exception = new KeepStackTraceException(exception);
            }
            this.result = result;
        }

        public override string ToString()
        {
            return $"State={state}, Exception={exception}, Result={result}";
        }

        public readonly static NWaitableState<T> None = new NWaitableState<T>(CompleteState.None, default, default);
        public readonly static NWaitableState<T> Canceled = new NWaitableState<T>(CompleteState.Canceled, NExceptionHelper.CanceledException, default);
        public static NWaitableState<T> Completed(T value) => new NWaitableState<T>(CompleteState.Completed, default, value);
        public static NWaitableState<T> Exception(Exception exception) => new NWaitableState<T>(CompleteState.Exception, exception, default);

        public static implicit operator NWaitableState(NWaitableState<T> state)
        {
            return state.state switch
            {
                CompleteState.None => NWaitableState.None,
                CompleteState.Canceled => NWaitableState.Canceled,
                CompleteState.Completed => NWaitableState.Completed,
                CompleteState.Exception => NWaitableState.Exception(state.exception),
                _ => throw new NotImplementedException(),
            };
        }
    }
}
