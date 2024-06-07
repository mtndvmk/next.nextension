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
#if UNITY_EDITOR
                            if (waitable is ICancelable cancelable)
                            {
                                throw new Exception("waitable is ICancelable, please use IWaitableFromCancelable");
                            }
                            else
#endif
                            {
                                handle = NWaitableHandle.Factory.create(this, waitable.LoopType, predicateFunc);
                            }
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
        public void cancel()
        {
            handle?.cancel();
        }

        protected override void onInnerCompleted()
        {
            handle = null;
        }
    }
    public sealed class NWaitableAwaiter : AbsNWaitableAwaiter
    {
        public static NWaitableAwaiter create(IWaitable waitable)
        {
            var awaiter = new NWaitableAwaiter();
            awaiter.setup(waitable);
            return awaiter;
        }
        public static NWaitableAwaiter create(IWaitableFromCancelable waitable)
        {
            var awaiter = new NWaitableAwaiter();
            awaiter.setup(waitable);
            return awaiter;
        }
        public static NWaitableAwaiter create(NWaitable waitable)
        {
            var awaiter = new NWaitableAwaiter();
            awaiter.setupFrom(waitable);
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
