using System.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Nextension
{
    public static class NGetAwaiter
    {
        public static NTaskAwaiter GetAwaiter(this NTask task)
        {
            return NTaskAwaiter.create(task);
        }
        public static NTaskAwaiter<T> GetAwaiter<T>(this NTask<T> task)
        {
            return NTaskAwaiter<T>.create(task);
        }
        public static NLoopWaitableAwaiter GetAwaiter<T>(this T waitable) where T : IWaitable
        {
            return NLoopWaitableAwaiter.create(waitable);
        }

        public static NLoopWaitableAwaiter GetAwaiter(this AsyncOperation operation)
        {
            var waitable = new NWaitUntil(() => operation.isDone);
            return NLoopWaitableAwaiter.create(waitable);
        }
        public static NLoopWaitableAwaiter GetAwaiter(this CustomYieldInstruction operation)
        {
            var waitable = new NWaitUntil(() => !operation.keepWaiting);
            return NLoopWaitableAwaiter.create(waitable);
        }
        public static NLoopWaitableAwaiter GetAwaiter(this JobHandle jobHandle)
        {
            return NLoopWaitableAwaiter.create(new NWaitJobHandle(jobHandle));
        }
        public static NLoopWaitableAwaiter GetAwaiter(this IEnumerator routine)
        {
            return NLoopWaitableAwaiter.create(new NWaitRoutine(routine));
        }
    }
}
