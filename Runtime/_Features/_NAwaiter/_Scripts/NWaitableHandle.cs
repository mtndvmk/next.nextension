using System;
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

#if UNITY_EDITOR
            [EditorQuittingMethod]
            private static void clearPool()
            {
                pool.clear();
            }
#endif
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
            public static NWaitableHandle createEditorHandle(AbsNWaitableAwaiter awaiter, Func<NWaitableResult> predicateFunc)
            {
                var handle = new NWaitableHandle();
                handle.setupEditor(awaiter, predicateFunc);
                return handle;
            }
#endif
            public static NWaitableHandle create(AbsNWaitableAwaiter awaiter, NLoopType loopType, Func<NWaitableResult> predicateFunc)
            {
                var handle = pool.get();
                handle.setup(awaiter, loopType, predicateFunc);
                return handle;
            }
            public static NWaitableHandle create(AbsNWaitableAwaiter awaiter, NLoopType loopType, Func<NWaitableResult> predicateFunc, ICancelable cancelable)
            {
                var handle = pool.get();
                handle.setup(awaiter, loopType, predicateFunc, cancelable);
                return handle;
            }
        }

#if UNITY_EDITOR
        internal bool isEditorWaitable;
        private void setupEditor(AbsNWaitableAwaiter awaiter, Func<NWaitableResult> predicateFunc)
        {
            this._predicateFunc = predicateFunc ?? throw new ArgumentNullException(nameof(predicateFunc));
            this._awaiter = awaiter;
            isEditorWaitable = true;
        }
#endif

        private void setup(AbsNWaitableAwaiter awaiter, NLoopType loopType, Func<NWaitableResult> predicateFunc)
        {
            EditorCheck.checkEditorMode();
            this._predicateFunc = predicateFunc;
            this._awaiter = awaiter;
            this.loopType = loopType;
        }
        private void setup(AbsNWaitableAwaiter awaiter, NLoopType loopType, Func<NWaitableResult> predicateFunc, ICancelable cancelable)
        {
            EditorCheck.checkEditorMode();
            this._predicateFunc = predicateFunc;
            this._awaiter = awaiter;
            this.loopType = loopType;
            _cancelable = cancelable;
        }

        private NWaitableHandle() { }
        private void resetState()
        {
            _cancelable = null;
            Status = default;
            loopType = DEFAULT_LOOP_TYPE;
            _awaiter = null;
            _predicateFunc = null;
#if UNITY_EDITOR
            isEditorWaitable = false;
#endif
        }

        private Func<NWaitableResult> _predicateFunc;
        private ICancelable _cancelable;
        private AbsNWaitableAwaiter _awaiter;

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
            _cancelable?.cancel();
        }
        void IPoolable.onDespawn()
        {
            resetState();
        }
    }
}
