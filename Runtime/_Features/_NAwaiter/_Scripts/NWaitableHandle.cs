using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

namespace Nextension
{
    internal class NWaitableHandle : ICancellable
    {
        internal static NWaitableHandle NonWaitHandle = new NWaitableHandle() { Status = RunState.Completed };

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        internal static void preventEditorHandle()
        {
            if (!Application.isPlaying)
            {
                throw new Exception("Please use [IWaitable_Editor] in editmode");
            }
        }

#if UNITY_EDITOR
        internal readonly bool isEditorWaitable;
        internal NWaitableHandle(IWaitable_Editor waitable)
        {
            var predicateFunc = waitable.buildCompleteFunc();
            if (predicateFunc == null)
            {
                throw new ArgumentNullException(nameof(predicateFunc));
            }
            this.predicateFunc = predicateFunc;
            isEditorWaitable = true;
        }
        internal NWaitableHandle(Func<bool> predicateFunc, bool _)
        {
            if (predicateFunc == null)
            {
                throw new ArgumentNullException(nameof(predicateFunc));
            }
            this.predicateFunc = () =>
            {
                if (predicateFunc())
                {
                    return CompleteState.Completed;
                }
                return CompleteState.None;
            };
            isEditorWaitable = true;
        }
#endif

        internal NWaitableHandle(Func<bool> predicateFunc)
        {
            preventEditorHandle();
            if (predicateFunc == null)
            {
                throw new ArgumentNullException(nameof(predicateFunc));
            }
            this.predicateFunc = () =>
            {
                if (predicateFunc())
                {
                    return CompleteState.Completed;
                }
                return CompleteState.None;
            };
        }
        internal NWaitableHandle(Func<CompleteState> predicateFunc)
        {
            preventEditorHandle();
            if (predicateFunc == null)
            {
                throw new ArgumentNullException(nameof(predicateFunc));
            }
            this.predicateFunc = predicateFunc;
        }
        internal NWaitableHandle(IWaitable waitable)
        {
            preventEditorHandle();
            var predicateFunc = waitable.buildCompleteFunc();
            if (predicateFunc == null)
            {
                throw new ArgumentNullException(nameof(predicateFunc));
            }
            this.predicateFunc = predicateFunc;
        }
        internal NWaitableHandle(IWaitableFromCancellable waitable)
        {
            preventEditorHandle();
            var (predicateFunc, cancelable) = waitable.buildCompleteFunc();
            if (predicateFunc == null)
            {
                throw new ArgumentNullException(nameof(predicateFunc));
            }
            this.predicateFunc = predicateFunc;
            addCancelleable(cancelable);
        }

        private NWaitableHandle() { }
        private Func<CompleteState> predicateFunc;
        private Action continuationFunc;
        private List<ICancellable> cancellables;

        internal void addCancelleable(ICancellable cancellable)
        {
            if (cancellables == null)
            {
                cancellables = new List<ICancellable>();
            }
            cancellables.Add(cancellable);
        }
        internal void setContinuationFunc(Action continuationFunc)
        {
            this.continuationFunc = continuationFunc;
        }
        internal RunState checkComplete()
        {
            if (isFinished())
            {
                return Status;
            }
            var state = predicateFunc();
            switch (state)
            {
                case CompleteState.Cancelled:
                    cancel();
                    return RunState.Cancelled;
                case CompleteState.Completed:
                    Status = RunState.Completed;
                    try
                    {
                        onCompleteEvent?.Invoke();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                    continuationFunc.Invoke();
                    return RunState.Completed;
                default:
                    return RunState.Running;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool isFinished() => Status.isFinished();
        public RunState Status { get; internal set; }
        public event Action onCompleteEvent;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void cancel()
        {
            Status = RunState.Cancelled;
            if (cancellables != null)
            {
                foreach (var c in cancellables)
                {
                    c.cancel();
                }
            }
        }
    }
}
