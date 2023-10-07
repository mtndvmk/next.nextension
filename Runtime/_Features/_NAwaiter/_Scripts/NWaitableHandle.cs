using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Nextension
{
    internal class NWaitableHandle : ICancelable, IPoolable
    {
        internal readonly static NWaitableHandle CompletedHandle = new() { Status = RunState.Completed };
        private const NLoopType DEFAULT_LOOP_TYPE = NLoopType.Update;

        internal static class Factory
        {
            private static NPool<NWaitableHandle> pool = new();

            [EditorQuittingMethod]
            private static void clearPool()
            {
                pool.clear();
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
                var handle = pool.get();
                handle.setup(awaiter, waitable);
                return handle;
            }
            public static NWaitableHandle create(AbsNWaitableAwaiter awaiter, IWaitableFromCancelable waitable)
            {
                var handle = pool.get();
                handle.setup(awaiter, waitable);
                return handle;
            }
        }

#if UNITY_EDITOR
        internal bool isEditorWaitable;
        private void setup(AbsNWaitableAwaiter awaiter, IWaitable_Editor waitable)
        {
            var predicateFunc = waitable.buildCompleteFunc();
            this._predicateFunc = predicateFunc ?? throw new ArgumentNullException(nameof(predicateFunc));
            this._awaiter = awaiter;
            isEditorWaitable = true;
        }
#endif

        private void setup(AbsNWaitableAwaiter awaiter, IWaitable waitable)
        {
            InternalCheck.checkEditorMode();
            this._predicateFunc = waitable.buildCompleteFunc() ?? throw new ArgumentNullException(nameof(_predicateFunc));
            this._awaiter = awaiter;
            this.loopType = waitable.LoopType;
        }
        private void setup(AbsNWaitableAwaiter awaiter, IWaitableFromCancelable waitable)
        {
            InternalCheck.checkEditorMode();
            var (predicateFunc, cancelable) = waitable.buildCompleteFunc();
            this._predicateFunc = predicateFunc ?? throw new ArgumentNullException(nameof(predicateFunc));
            this._awaiter = awaiter;
            this.loopType = waitable.LoopType;
            addCancelable(cancelable);
        }

        public NWaitableHandle() { }
        private void resetState()
        {
            _cancelables?.Clear();
            Status = default;
            loopType = DEFAULT_LOOP_TYPE;
            _awaiter = null;
            _predicateFunc = null;
#if UNITY_EDITOR
            isEditorWaitable = false;
#endif
        }

        private Func<NWaitableResult> _predicateFunc;
        private List<ICancelable> _cancelables;
        private AbsNWaitableAwaiter _awaiter;

        internal void addCancelable(ICancelable cancellable)
        {
            (_cancelables ??= new(1)).Add(cancellable);
        }

        internal RunState checkComplete()
        {
            if (isFinished())
            {
                return Status;
            }
            var result = _predicateFunc();
            switch (result.state)
            {
                case CompleteState.Canceled:
                    {
                        cancel();
                        return Status;
                    }
                case CompleteState.Completed:
                    {
                        Status = RunState.Completed;
                        _awaiter.invokeComplete();
                        return Status;
                    }
                case CompleteState.Exception:
                    {
                        Status = RunState.Exception;
                        _awaiter.invokeException(result.exception);
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
        public NLoopType loopType = DEFAULT_LOOP_TYPE;

        public void cancel()
        {
            if (isFinished())
            {
                return;
            }
            Status = RunState.Canceled;
            if (_cancelables != null)
            {
                foreach (var c in _cancelables)
                {
                    c.cancel();
                }
            }
        }
        void IPoolable.onDespawned()
        {
            resetState();
        }
    }
}
