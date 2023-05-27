using System;
using System.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Nextension
{
    public static class NAwaiter
    {
        public static NWaitable runDelay(float second, Action delayCallback)
        {
            return runDelayAsync(second, delayCallback);
        }
        public static async NWaitable runDelayAsync(float second, Action delayCallback)
        {
            if (delayCallback == null)
            {
                throw new NullReferenceException("NAwaiter.runDelay.delayCallback");
            }
            await new NWaitSecond(second);
            delayCallback.Invoke();
        }
    }

    public struct NWaitUntil : IWaitable
    {
        public WaiterLoopType loopType;
        internal readonly Func<bool> predicate;
        public NWaitUntil(Func<bool> predicate, WaiterLoopType loopType = WaiterLoopType.Update)
        {
            this.predicate = predicate;
            this.loopType = loopType;
        }

        bool IWaitable.IsWaitable => predicate != null;

        Func<CompleteState> IWaitable.buildCompleteFunc()
        {
            var predicate = this.predicate;
            Func<CompleteState> func = () =>
            {
                if (predicate())
                {
                    return CompleteState.Completed;
                }
                return CompleteState.None;
            };
            return func;
        }
    }
    public struct NWaitFrame : IWaitable
    {
        public WaiterLoopType loopType;
        internal readonly uint waitFrame;
        public NWaitFrame(uint waitFrame, WaiterLoopType loopType = WaiterLoopType.Update)
        {
            this.waitFrame = waitFrame;
            this.loopType = loopType;
        }

        bool IWaitable.IsWaitable => waitFrame > 0;

        Func<CompleteState> IWaitable.buildCompleteFunc()
        {
            var targetFrame = NUpdater.UpdateCount + waitFrame;
            Func<CompleteState> func = () =>
            {
                if (NUpdater.UpdateCount >= targetFrame)
                {
                    return CompleteState.Completed;
                }
                return CompleteState.None;
            };
            return func;
        }
    }
    public struct NWaitSecond : IWaitable
    {
        public WaiterLoopType loopType;
        internal readonly float waitSecond;
        public NWaitSecond(float waitSecond, WaiterLoopType loopType = WaiterLoopType.Update)
        {
            this.waitSecond = waitSecond;
            this.loopType = loopType;
        }

        bool IWaitable.IsWaitable => waitSecond > 0;

        Func<CompleteState> IWaitable.buildCompleteFunc()
        {
            var targetSecond = Time.time + waitSecond;
            Func<CompleteState> func = () =>
            {
                if (Time.time >= targetSecond)
                {
                    return CompleteState.Completed;
                }
                return CompleteState.None;
            };
            return func;
        }
    }
    public struct NWaitRealtimeSecond : IWaitable
    {
        public WaiterLoopType loopType;
        internal readonly float waitSecond;
        public NWaitRealtimeSecond(float waitSecond, WaiterLoopType loopType = WaiterLoopType.Update)
        {
            this.waitSecond = waitSecond;
            this.loopType = loopType;
        }

        bool IWaitable.IsWaitable => waitSecond > 0;

        Func<CompleteState> IWaitable.buildCompleteFunc()
        {
            var targetSecond = Time.realtimeSinceStartup + waitSecond;
            Func<CompleteState> func = () =>
            {
                if (Time.realtimeSinceStartup >= targetSecond)
                {
                    return CompleteState.Completed;
                }
                return CompleteState.None;
            };
            return func;
        }
    }
    public struct NWaitJobHandle : IWaitable
    {
        public WaiterLoopType loopType;
        internal readonly JobHandle jobHandle;
        public NWaitJobHandle(JobHandle jobHandle, WaiterLoopType loopType = WaiterLoopType.Update)
        {
            this.jobHandle = jobHandle;
            this.loopType = loopType;
        }

        bool IWaitable.IsWaitable => !jobHandle.Equals(default);

        Func<CompleteState> IWaitable.buildCompleteFunc()
        {
            var jobHandle = this.jobHandle;
            Func<CompleteState> func = () =>
            {
                if (jobHandle.IsCompleted)
                {
                    jobHandle.Complete();
                    return CompleteState.Completed;
                }
                return CompleteState.None;
            };
            return func;
        }
    }
    public struct NWaitRoutine : IWaitableFromCancellable
    {
        public WaiterLoopType loopType;
        internal readonly IEnumerator routine;
        public NWaitRoutine(IEnumerator routine, WaiterLoopType loopType = WaiterLoopType.Update)
        {
            this.routine = routine;
            this.loopType = loopType;
        }
        bool IWaitableFromCancellable.IsWaitable => !routine.Equals(default);

        (Func<CompleteState>, ICancellable) IWaitableFromCancellable.buildCompleteFunc()
        {
            var data = NCoroutine.startCoroutine(routine);
            Func<CompleteState> func = () =>
            {
                switch (data.Status)
                {
                    case RunState.Completed: return CompleteState.Completed;
                    case RunState.Cancelled: return CompleteState.Cancelled;
                    default: return CompleteState.None;
                }
            };
            return (func, data);
        }
    }
}
