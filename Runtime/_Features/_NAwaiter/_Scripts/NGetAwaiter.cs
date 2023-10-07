using System.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Nextension
{
    public static class NGetAwaiter
    {
        public static NWaitableAwaiter GetAwaiter(this NWaitable waitable)
        {
            return NWaitableAwaiter.create(waitable);
        }
        public static NWaitableAwaiter<T> GetAwaiter<T>(this NWaitable<T> waitable)
        {
            return new NWaitableAwaiter<T>(waitable);
        }
        public static NWaitableAwaiter GetAwaiter(this IWaitable waitable)
        {
            InternalCheck.checkEditorMode();
            return NWaitableAwaiter.create(waitable);
        }
        public static NWaitableAwaiter GetAwaiter(this IWaitableFromCancelable waitable)
        {
            InternalCheck.checkEditorMode();
            return NWaitableAwaiter.create(waitable);
        }
        public static NWaitableAwaiter GetAwaiter(this AsyncOperation operation)
        {
            InternalCheck.checkEditorMode();
            var waitable = new NWaitUntil(() => operation.isDone);
            return NWaitableAwaiter.create(waitable);
        }
        public static NWaitableAwaiter GetAwaiter(this CustomYieldInstruction operation)
        {
            InternalCheck.checkEditorMode();
            var waitable = new NWaitUntil(() => !operation.keepWaiting);
            return NWaitableAwaiter.create(waitable);
        }
        public static NWaitableAwaiter GetAwaiter(this JobHandle jobHandle)
        {
            InternalCheck.checkEditorMode();
            return NWaitableAwaiter.create(new NWaitJobHandle(jobHandle));
        }
        public static NWaitableAwaiter GetAwaiter(this IEnumerator routine)
        {
            InternalCheck.checkEditorMode();
            return NWaitableAwaiter.create(new NWaitRoutine(routine));
        }
        public static NWaitableAwaiter GetAwaiter(this NWaitRoutine routine)
        {
            InternalCheck.checkEditorMode();
            return NWaitableAwaiter.create(routine);
        }
#if UNITY_EDITOR
        public static NWaitableAwaiter GetAwaiter(this IWaitable_Editor waitable)
        {
            return NWaitableAwaiter.create(waitable);
        }
#endif
    }
}
