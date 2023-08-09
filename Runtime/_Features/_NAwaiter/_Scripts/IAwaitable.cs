using System;

namespace Nextension
{
    public interface IWaitable
    {
        internal Func<NWaitableResult> buildCompleteFunc();
        internal WaiterLoopType LoopType { get; }
    }
    public interface IWaitableFromCancellable
    {
        internal (Func<NWaitableResult>, ICancellable) buildCompleteFunc();
        internal WaiterLoopType LoopType { get; }
    }
    public interface IWaitable_Editor
    {
        internal Func<NWaitableResult> buildCompleteFunc();
    }
}