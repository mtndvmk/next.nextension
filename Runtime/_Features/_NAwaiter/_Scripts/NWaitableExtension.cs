using System.Collections;

namespace Nextension
{
    public static class NWaitableExtension
    {
        internal static NWaitableHandle createWaitableHandle(this IWaitable waitable)
        {
            NWaitableHandle nwaitable;
            if (waitable.IsWaitable)
            {
                nwaitable = new NWaitableHandle(waitable);
            }
            else
            {
                nwaitable = NWaitableHandle.NonWaitHandle;
            }
            return nwaitable;
        }
        internal static NWaitableHandle createWaitableHandle(this IWaitableFromCancellable waitable)
        {
            NWaitableHandle nwaitable;
            if (waitable.IsWaitable)
            {
                nwaitable = new NWaitableHandle(waitable);
            }
            else
            {
                nwaitable = NWaitableHandle.NonWaitHandle;
            }
            return nwaitable;
        }

        public static NWaitable startWaitable(this IWaitable waitable)
        {
            return startWaitable(waitable.createWaitableHandle());
        }
        public static NWaitable startWaitable(this IWaitableFromCancellable waitable)
        {
            return startWaitable(waitable.createWaitableHandle());
        }

        public static NWaitable startWaitable(this IEnumerator routine)
        {
            return startWaitable(new NWaitRoutine(routine).createWaitableHandle());
        }
        internal static async NWaitable startWaitable(this NWaitableHandle handle)
        {
            await handle;
        }
    }
}
