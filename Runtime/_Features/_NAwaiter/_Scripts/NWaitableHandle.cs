using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Nextension
{
    internal class NWaitableHandle : ICancellable, IPoolable
    {
        internal static NWaitableHandle NonWaitHandle = new NWaitableHandle() { Status = RunState.Completed };
        private const WaiterLoopType DEFAULT_LOOP_TYPE = WaiterLoopType.Update;

        internal static class Factory
        {
            private static NPool<NWaitableHandle> pool;

            [StartupMethod]
            private static void init()
            {
                pool = new NPool<NWaitableHandle>();
            }
            private static NWaitableHandle get()
            {
                return pool.get();
            }
            public static void release(NWaitableHandle handle)
            {
                if (handle != null)
                {
#if UNITY_EDITOR
                    if (handle.isEditorWaitable)
                    {
                        return;
                    }
                    if (!NStartRunner.IsPlaying)
                    {
                        return;
                    }
#endif
                    pool.release(handle);
                }
            }

#if UNITY_EDITOR
            public static NWaitableHandle create(AbsNWaitableAwaiter awaiter, IWaitable_Editor waitable)
            {
                var handle = new NWaitableHandle();
                handle.setup(awaiter, waitable);
                return handle;
            }
#endif
            public static NWaitableHandle create(AbsNWaitableAwaiter awaiter, IWaitable waitable)
            {
                var handle = get();
                handle.setup(awaiter, waitable);
                return handle;
            }
            public static NWaitableHandle create(AbsNWaitableAwaiter awaiter, IWaitableFromCancellable waitable)
            {
                var handle = get();
                handle.setup(awaiter, waitable);
                return handle;
            }
        }

#if UNITY_EDITOR
        internal bool isEditorWaitable;
        private void setup(AbsNWaitableAwaiter awaiter, IWaitable_Editor waitable)
        {
            var predicateFunc = waitable.buildCompleteFunc();
            if (predicateFunc == null)
            {
                throw new ArgumentNullException(nameof(predicateFunc));
            }
            this.predicateFunc = predicateFunc;
            this.awaiter = awaiter;
            isEditorWaitable = true;
        }
#endif

        private void setup(AbsNWaitableAwaiter awaiter, IWaitable waitable)
        {
            InternalCheck.checkEditorMode();
            var predicateFunc = waitable.buildCompleteFunc();
            if (predicateFunc == null)
            {
                throw new ArgumentNullException(nameof(predicateFunc));
            }
            this.predicateFunc = predicateFunc;
            this.awaiter = awaiter;
            this.loopType = waitable.LoopType;
        }
        private void setup(AbsNWaitableAwaiter awaiter, IWaitableFromCancellable waitable)
        {
            InternalCheck.checkEditorMode();
            var (predicateFunc, cancelable) = waitable.buildCompleteFunc();
            if (predicateFunc == null)
            {
                throw new ArgumentNullException(nameof(predicateFunc));
            }
            this.predicateFunc = predicateFunc;
            this.awaiter = awaiter;
            this.loopType = waitable.LoopType;
            addCancelleable(cancelable);
        }

        private NWaitableHandle() { }
        private void reseState()
        {
            cancellables?.Clear();
            cancellables = null;
            Status = default;
            loopType = DEFAULT_LOOP_TYPE;
            awaiter = null;
            predicateFunc = null;
#if UNITY_EDITOR
            isEditorWaitable = false;
#endif
        }

        private Func<NWaitableResult> predicateFunc;
        private List<ICancellable> cancellables;
        private AbsNWaitableAwaiter awaiter;

        internal void addCancelleable(ICancellable cancellable)
        {
            if (cancellables == null)
            {
                cancellables = new List<ICancellable>();
            }
            cancellables.Add(cancellable);
        }

        internal RunState checkComplete()
        {
            if (isFinished())
            {
                return Status;
            }
            var result = predicateFunc();
            switch (result.state)
            {
                case CompleteState.Cancelled:
                    {
                        cancel();
                        return Status;
                    }
                case CompleteState.Completed:
                    {
                        Status = RunState.Completed;
                        awaiter.invokeComplete();
                        return Status;
                    }
                case CompleteState.Exception:
                    {
                        awaiter.invokeException(result.exception);
                        Status = RunState.Exception;
                        return Status;
                    }
                case CompleteState.None:
                    {
                        return RunState.Running;
                    }
                default:
                    {
                        Debug.LogWarning("Not implement state: " + result.state);
                        return RunState.Running;
                    }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool isFinished() => Status.isFinished();
        public RunState Status { get; internal set; }
        public WaiterLoopType loopType = DEFAULT_LOOP_TYPE;

        public void cancel()
        {
            if (isFinished())
            {
                return;
            }
            Status = RunState.Cancelled;
            if (cancellables != null)
            {
                foreach (var c in cancellables)
                {
                    c.cancel();
                }
            }
        }
        void IPoolable.onDespawned()
        {
            reseState();
        }
    }
}
