using System.Runtime.CompilerServices;
using UnityEngine;

namespace Nextension
{
    public abstract class AbsNWaitableAwaiter : AbsAwaiter, ICancelable
    {
        internal NWaitableHandle handle;
        protected AbsNWaitableAwaiter() { }
        protected void setupFrom(AbsNWaitable waitable)
        {
            NWaitableHandle handle;
            switch (waitable.Status)
            {
                case RunState.Exception:
                    invokeException(waitable.Exception);
                    return;
                case RunState.Completed:
                    invokeComplete();
                    return;
                case RunState.Canceled:
                    return;
                case RunState.Running:
                    handle = NWaitableHandle.Factory.create(this, new NWaitUntil(() => waitable.Status.isFinished()));
                    setup(handle);
                    return;
                case RunState.None:
                    handle = NWaitableHandle.Factory.create(this, waitable);
                    setup(handle);
                    return;
                default:
                    Debug.LogWarning("Not implement staus: " + waitable.Status);
                    return;
            }
        }
        internal void setup(NWaitableHandle handle)
        {
            this.handle = handle;
            handle.Status = RunState.Running;
#if UNITY_EDITOR
            if (handle.isEditorWaitable)
            {
                NAwaiter_EdtitorLoop.addAwaitable(handle);
                return;
            }
#endif
            NAwaiterLoop.addAwaitable(handle);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void cancel()
        {
            handle?.cancel();
        }
    }
    public sealed class NWaitableAwaiter : AbsNWaitableAwaiter, IPoolable
    {
        [EditorQuittingMethod]
        private static void clearPool()
        {
            _pool?.clear();
        }
        private static NPool<NWaitableAwaiter> _pool = new NPool<NWaitableAwaiter>();

        public static NWaitableAwaiter create(IWaitable waitable)
        {
            var awaiter = _pool.get();
            var handle = NWaitableHandle.Factory.create(awaiter, waitable);
            awaiter.setup(handle);
            return awaiter;
        }
        public static NWaitableAwaiter create(IWaitableFromCancelable waitable)
        {
            var awaiter = _pool.get();
            var handle = NWaitableHandle.Factory.create(awaiter, waitable);
            awaiter.setup(handle);
            return awaiter;
        }
#if UNITY_EDITOR
        public static NWaitableAwaiter create(IWaitable_Editor waitable)
        {
            var awaiter = new NWaitableAwaiter();
            var handle = NWaitableHandle.Factory.create(awaiter, waitable);
            awaiter.setup(handle);
            return awaiter;
        }
#endif
        public static NWaitableAwaiter create(NWaitable waitable)
        {
            var awaiter = _pool.get();
            awaiter.setupFrom(waitable);
            return awaiter;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void cancel()
        {
            base.cancel();
            releaseToPool();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void onInnerCompleted()
        {
            releaseToPool();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IPoolable.onDespawned()
        {
            reset();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void releaseToPool()
        {
#if UNITY_EDITOR
            if (handle.isEditorWaitable)
            {
                handle = null;
                return;
            }
#endif
            handle = null;
            _pool.release(this);
        }
    }
    public sealed class NWaitableAwaiter<T> : AbsNWaitableAwaiter
    {
        private NWaitable<T> waitable;
        public NWaitableAwaiter(NWaitable<T> waitable)
        {
            this.waitable = waitable;
            setupFrom(waitable);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new T GetResult() => waitable.Result;
    }
}
