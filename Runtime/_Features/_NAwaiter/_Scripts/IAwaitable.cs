namespace Nextension
{
    public interface IWaitable
    {
        internal NLoopType LoopType { get; }
        internal bool IsIgnoreFirstFrameCheck { get; }
        internal ICancelable onStartWaitable(NWaitableResultGetter getter);
        internal NWaitableState getCurrentState();
    }
    public interface ICustomWaitable
    {
        internal NWaitableState getCurrentState();
    }
}