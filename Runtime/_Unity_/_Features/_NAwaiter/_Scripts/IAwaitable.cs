namespace Nextension
{
    public interface IWaitable
    {
        NLoopType LoopType { get; }
        internal ICancelable onStartWaitable(NWaitableResultGetter getter);
        NWaitableState getCurrentState();
    }
    public abstract class CustomWaitable
    {
        public NLoopType LoopType => NLoopType.Update;
        public abstract NWaitableState getCurrentState();
    }
}