using System;

namespace Nextension
{
    public interface IWaitable
    {
        internal Func<NWaitableResult> buildCompleteFunc();
        internal NLoopType LoopType { get; }
    }
    public interface IWaitableFromCancelable
    {
        internal (Func<NWaitableResult>, ICancelable) buildCompleteFunc();
        internal NLoopType LoopType { get; }
    }
    public interface IWaitable_Editor
    {
        internal Func<NWaitableResult> buildCompleteFunc();
    }
}