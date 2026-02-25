using System;

namespace Nextension
{
    public static class NWaitableExtensions
    {
        public static NCustomWaitable wait<TCustomWaitable>(this TCustomWaitable waitable) where TCustomWaitable : ICustomWaitable
        {
            return new NCustomWaitable(waitable);
        }
        public static void waitAndRun<TWaitable>(this TWaitable waitable, Action next) where TWaitable : IWaitable
        {
            NTask.waitAndRun(waitable, next);
        }
    }
}
