using System.Collections;

namespace Nextension
{
    public static class NWaitableExtension
    {
        public static NWaitable startWaitable(this IWaitable waitable)
        {
            return asyncStartWaitable(waitable);
        }
        public static NWaitable startWaitable(this IWaitableFromCancellable waitable)
        {
            return asyncStartWaitableFromCancellable(waitable);
        }

        public static NWaitable startWaitable(this IEnumerator routine)
        {
            return startWaitable(new NWaitRoutine(routine));
        }
        internal static async NWaitable asyncStartWaitable(this IWaitable waitable)
        {
            await waitable;
        }
        internal static async NWaitable asyncStartWaitableFromCancellable(this IWaitableFromCancellable waitable)
        {
            await waitable;
        }

#if UNITY_EDITOR
        public static NWaitable startWaitable(this IWaitable_Editor waitable)
        {
            return asyncStartWaitable(waitable);
        }
        internal static async NWaitable asyncStartWaitable(this IWaitable_Editor waitable)
        {
            await waitable;
        }
#endif
    }
}
