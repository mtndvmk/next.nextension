using System;
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
                    setup(waitable);
                    return;
                case RunState.None:
                    setup(waitable);
                    return;
                default:
                    Debug.LogWarning("Not implement staus: " + waitable.Status);
                    return;
            }
        }
        protected void setup(IWaitable waitable)
        {
            var predicateFunc = waitable.buildCompleteFunc();
            try
            {
                var result = predicateFunc();
                switch (result.state)
                {
                    default:
                    case CompleteState.None:
                        {
                            handle = NWaitableHandle.Factory.create(this, waitable.LoopType, predicateFunc);
                            setup(handle, waitable.IsIgnoreFirstFrameCheck);
                            return;
                        }
                    case CompleteState.Canceled:
                        {
                            return;
                        }
                    case CompleteState.Completed:
                        {
                            invokeComplete();
                            return;
                        }
                    case CompleteState.Exception:
                        {
                            invokeException(result.exception);
                            return;
                        }
                }
            }
            catch (Exception e)
            {
                invokeException(e);
            }
        }
        protected void setup(IWaitableFromCancelable waitable)
        {
            (var predicateFunc, var cancelable) = waitable.buildCompleteFunc();
            try
            {
                var result = predicateFunc();
                switch (result.state)
                {
                    default:
                    case CompleteState.None:
                        {
                            handle = NWaitableHandle.Factory.create(this, waitable.LoopType, predicateFunc, cancelable);
                            setup(handle, waitable.IsIgnoreFirstFrameCheck);
                            return;
                        }
                    case CompleteState.Canceled:
                        {
                            cancelable.cancel();
                            return;
                        }
                    case CompleteState.Completed:
                        {
                            invokeComplete();
                            return;
                        }
                    case CompleteState.Exception:
                        {
                            invokeException(result.exception);
                            return;
                        }
                }
            }
            catch (Exception e)
            {
                invokeException(e);
            }
        }
        internal void setup(NWaitableHandle handle, bool isIgnoreFisrtFrameCheck)
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
            NAwaiterLoop.addAwaitable(handle, isIgnoreFisrtFrameCheck);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void cancel()
        {
            handle?.cancel();
        }
    }
    public sealed class NWaitableAwaiter : AbsNWaitableAwaiter, IPoolable
    {
#if UNITY_EDITOR
        [EditorQuittingMethod]
        private static void clearPool()
        {
            _pool?.clear();
        }
#endif
        private static NPool<NWaitableAwaiter> _pool = new NPool<NWaitableAwaiter>();

        public static NWaitableAwaiter create(IWaitable waitable)
        {
            var awaiter = _pool.get();
            awaiter.setup(waitable);
            return awaiter;
        }
        public static NWaitableAwaiter create(IWaitableFromCancelable waitable)
        {
            var awaiter = _pool.get();
            awaiter.setup(waitable);
            return awaiter;
        }
#if UNITY_EDITOR
        public static NWaitableAwaiter create(IWaitable_Editor waitable)
        {
            var awaiter = new NWaitableAwaiter();
            var predicateFunc = waitable.buildCompleteFunc();

            try
            {
                var result = predicateFunc();
                switch (result.state)
                {
                    default:
                    case CompleteState.None:
                        {
                            var handle = NWaitableHandle.Factory.createEditorHandle(awaiter, predicateFunc);
                            awaiter.setup(handle, true);
                            break;
                        }
                    case CompleteState.Canceled:
                        {
                            break;
                        }
                    case CompleteState.Completed:
                        {
                            awaiter.invokeComplete();
                            break;
                        }
                    case CompleteState.Exception:
                        {
                            awaiter.invokeException(result.exception);
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                awaiter.invokeException(e);
            }
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
