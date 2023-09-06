using System;
using System.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Nextension
{
    public static class NAwaiter
    {
        public static void runDelay(float second, Action delayCallback)
        {
            _ = runDelayAsync(second, delayCallback);
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

        WaiterLoopType IWaitable.LoopType => loopType;

        public NWaitUntil(Func<bool> predicate, WaiterLoopType loopType = WaiterLoopType.Update)
        {
            this.predicate = predicate;
            this.loopType = loopType;
        }

        Func<NWaitableResult> IWaitable.buildCompleteFunc()
        {
            var predicate = this.predicate;
            Func<NWaitableResult> func = () =>
            {
                if (predicate())
                {
                    return NWaitableResult.Completed;
                }
                return NWaitableResult.None;
            };
            return func;
        }
    }
    public struct NWaitFrame : IWaitable
    {
        public WaiterLoopType loopType;
        internal readonly uint waitFrame;

        WaiterLoopType IWaitable.LoopType => loopType;

        public NWaitFrame(uint waitFrame, WaiterLoopType loopType = WaiterLoopType.Update)
        {
            this.waitFrame = waitFrame;
            this.loopType = loopType;
        }

        Func<NWaitableResult> IWaitable.buildCompleteFunc()
        {
            var targetFrame = NUpdater.UpdateCount + waitFrame;
            Func<NWaitableResult> func = () =>
            {
                if (NUpdater.UpdateCount >= targetFrame)
                {
                    return NWaitableResult.Completed;
                }
                return NWaitableResult.None;
            };
            return func;
        }
    }
    public struct NWaitSecond : IWaitable
    {
        public WaiterLoopType loopType;
        internal readonly float waitSecond;

        WaiterLoopType IWaitable.LoopType => loopType;

        public NWaitSecond(float waitSecond, WaiterLoopType loopType = WaiterLoopType.Update)
        {
            this.waitSecond = waitSecond;
            this.loopType = loopType;
        }

        Func<NWaitableResult> IWaitable.buildCompleteFunc()
        {
            var targetSecond = Time.time + waitSecond;
            Func<NWaitableResult> func = () =>
            {
                if (Time.time >= targetSecond)
                {
                    return NWaitableResult.Completed;
                }
                return NWaitableResult.None;
            };
            return func;
        }
    }
    public struct NWaitRealtimeSecond : IWaitable
    {
        public WaiterLoopType loopType;
        internal readonly float waitSecond;

        WaiterLoopType IWaitable.LoopType => loopType;

        public NWaitRealtimeSecond(float waitSecond, WaiterLoopType loopType = WaiterLoopType.Update)
        {
            this.waitSecond = waitSecond;
            this.loopType = loopType;
        }

        Func<NWaitableResult> IWaitable.buildCompleteFunc()
        {
            var targetSecond = Time.realtimeSinceStartup + waitSecond;
            Func<NWaitableResult> func = () =>
            {
                if (Time.realtimeSinceStartup >= targetSecond)
                {
                    return NWaitableResult.Completed;
                }
                return NWaitableResult.None;
            };
            return func;
        }
    }
    public struct NWaitJobHandle : IWaitable
    {
        public WaiterLoopType loopType;
        internal readonly JobHandle jobHandle;

        WaiterLoopType IWaitable.LoopType => loopType;

        public NWaitJobHandle(JobHandle jobHandle, WaiterLoopType loopType = WaiterLoopType.Update)
        {
            this.jobHandle = jobHandle;
            this.loopType = loopType;
        }

        Func<NWaitableResult> IWaitable.buildCompleteFunc()
        {
            var jobHandle = this.jobHandle;
            Func<NWaitableResult> func = () =>
            {
                if (jobHandle.IsCompleted)
                {
                    jobHandle.Complete();
                    return NWaitableResult.Completed;
                }
                return NWaitableResult.None;
            };
            return func;
        }
    }
    public struct NWaitRoutine : IWaitableFromCancellable
    {
        public WaiterLoopType loopType;
        internal readonly IEnumerator routine;

        WaiterLoopType IWaitableFromCancellable.LoopType => loopType;

        public NWaitRoutine(IEnumerator routine, WaiterLoopType loopType = WaiterLoopType.Update)
        {
            this.routine = routine;
            this.loopType = loopType;
        }

        (Func<NWaitableResult>, ICancellable) IWaitableFromCancellable.buildCompleteFunc()
        {
            var data = NCoroutine.startCoroutine(routine);
            Func<NWaitableResult> func = () =>
            {
                switch (data.Status)
                {
                    case RunState.Completed: return NWaitableResult.Completed;
                    case RunState.Cancelled: return NWaitableResult.Cancelled;
                    default: return NWaitableResult.None;
                }
            };
            return (func, data);
        }
    }
    public struct CombineNWaitable : IWaitable
    {
        public readonly NWaitable[] waitables;

        public CombineNWaitable(params NWaitable[] waitables)
        {
            if (waitables == null || waitables.Length == 0)
            {
                throw new Exception("waitables is null or empty");
            }
            this.waitables = waitables;
        }

        WaiterLoopType IWaitable.LoopType => WaiterLoopType.Update;

        Func<NWaitableResult> IWaitable.buildCompleteFunc()
        {
            var waitables = this.waitables;
            Func<NWaitableResult> func = () =>
            {
                foreach (var waitable in waitables)
                {
                    switch (waitable.Status)
                    {
                        case RunState.Cancelled:
                            return NWaitableResult.Cancelled;
                        case RunState.Exception:
                            return NWaitableResult.Exception(waitable.Exception);
                        case RunState.Completed:
                            continue;
                        case RunState.Running:
                        case RunState.None:
                        default: return NWaitableResult.None;
                    }
                }
                return NWaitableResult.Completed;
            };
            return func;
        }
    }
}
