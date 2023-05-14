using System;
using System.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Nextension
{
    public static class NGetAwaiter
    {
        public static NWaitableAwaiter GetAwaiter(this NWaitable waitable)
        {
            return new NWaitableAwaiter(waitable);
        }
        public static NWaitableAwaiter<T> GetAwaiter<T>(this NWaitable<T> waitable)
        {
            return new NWaitableAwaiter<T>(waitable);
        }
        internal static NWaitableAwaiter GetAwaiter(this NWaitableHandle waitable)
        {
            return new NWaitableAwaiter(waitable);
        }

        public static NWaitableAwaiter GetAwaiter(this IWaitable waitable)
        {
            return new NWaitableAwaiter(waitable);
        }
        public static NWaitableAwaiter GetAwaiter(this IWaitable_Editor waitable)
        {
            return new NWaitableAwaiter(waitable);
        }
        public static NWaitableAwaiter GetAwaiter(this AsyncOperation operation)
        {
            Func<bool> func = () => operation.isDone;
            return new NWaitableAwaiter(new NWaitableHandle(func));
        }
        public static NWaitableAwaiter GetAwaiter(this CustomYieldInstruction operation)
        {
            Func<bool> func = () => !operation.keepWaiting;
            return new NWaitableAwaiter(new NWaitableHandle(func));
        }
        public static NWaitableAwaiter GetAwaiter(this JobHandle jobHandle)
        {
            return new NWaitableAwaiter(new NWaitJobHandle(jobHandle));
        }
        public static NWaitableAwaiter GetAwaiter(this IEnumerator routine)
        {
            return GetAwaiter(new NWaitRoutine(routine));
        }
        public static NWaitableAwaiter GetAwaiter(this NWaitRoutine routine)
        {
            return new NWaitableAwaiter(routine);
        }
    }
}
