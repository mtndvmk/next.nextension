using System;

namespace Nextension
{
    public interface IWaitable
    {
        internal bool IsWaitable { get; }
        internal Func<CompleteState> buildCompleteFunc();
    }
    public interface IRetartable
    {
        public void restart();
    }
    public interface IWaitableFromCancellable
    {
        internal bool IsWaitable { get; }
        internal (Func<CompleteState>, ICancellable) buildCompleteFunc();
    }
    public interface IWaitable_Editor : IWaitable
    {

    }
}
