using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Jobs;
using UnityEngine;

namespace Nextension
{
    public readonly struct NWaitUntil : IWaitable
    {
        public readonly NLoopType loopType;
        internal readonly Func<bool> predicate;

        NLoopType IWaitable.LoopType => loopType;
        bool IWaitable.IsIgnoreFirstFrameCheck => true;

        public NWaitUntil(Func<bool> predicate, NLoopType loopType = NLoopType.Update)
        {
            this.predicate = predicate;
            this.loopType = loopType;
        }

        Func<NWaitableResult> IWaitable.buildCompleteFunc()
        {
            var predicate = this.predicate;
            NWaitableResult func()
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
    public readonly struct NWaitFrame : IWaitable
    {
        public readonly NLoopType loopType;
        internal readonly uint waitFrame;

        NLoopType IWaitable.LoopType => loopType;
        bool IWaitable.IsIgnoreFirstFrameCheck => true;

        public NWaitFrame(uint waitFrame, NLoopType loopType = NLoopType.Update)
        {
            this.waitFrame = waitFrame;
            this.loopType = loopType;
        }

        Func<NWaitableResult> IWaitable.buildCompleteFunc()
        {
            var targetFrame = NUpdater.UpdateCount + (waitFrame == 0 ? 1 : waitFrame);
            NWaitableResult func()
            {
                if (NUpdater.UpdateCount >= targetFrame)
                {
                    return NWaitableResult.Completed;
                }
                return NWaitableResult.None;
            }
            return func;
        }
    }
    public readonly struct NWaitSecond : IWaitable
    {
        public readonly NLoopType loopType;
        internal readonly float waitSecond;

        NLoopType IWaitable.LoopType => loopType;
        bool IWaitable.IsIgnoreFirstFrameCheck => true;

        public NWaitSecond(float waitSecond, NLoopType loopType = NLoopType.Update)
        {
            this.waitSecond = waitSecond;
            this.loopType = loopType;
        }

        Func<NWaitableResult> IWaitable.buildCompleteFunc()
        {
            var targetSecond = Time.time + waitSecond;
            NWaitableResult func()
            {
                if (Time.time >= targetSecond)
                {
                    return NWaitableResult.Completed;
                }
                return NWaitableResult.None;
            }
            return func;
        }
    }
    public readonly struct NWaitRealtimeSecond : IWaitable
    {
        public readonly NLoopType loopType;
        internal readonly float waitSecond;

        NLoopType IWaitable.LoopType => loopType;
        bool IWaitable.IsIgnoreFirstFrameCheck => true;

        public NWaitRealtimeSecond(float waitSecond, NLoopType loopType = NLoopType.Update)
        {
            this.waitSecond = waitSecond;
            this.loopType = loopType;
        }

        Func<NWaitableResult> IWaitable.buildCompleteFunc()
        {
            var targetSecond = Time.realtimeSinceStartup + waitSecond;
            NWaitableResult func()
            {
                if (Time.realtimeSinceStartup >= targetSecond)
                {
                    return NWaitableResult.Completed;
                }
                return NWaitableResult.None;
            }
            return func;
        }
    }
    public readonly struct NWaitJobHandle : IWaitable
    {
        public readonly NLoopType loopType;
        internal readonly JobHandle jobHandle;
        bool IWaitable.IsIgnoreFirstFrameCheck => true;

        NLoopType IWaitable.LoopType => loopType;

        public NWaitJobHandle(JobHandle jobHandle, NLoopType loopType = NLoopType.Update)
        {
            this.jobHandle = jobHandle;
            this.loopType = loopType;
        }

        Func<NWaitableResult> IWaitable.buildCompleteFunc()
        {
            var jobHandle = this.jobHandle;
            NWaitableResult func()
            {
                if (jobHandle.IsCompleted)
                {
                    jobHandle.Complete();
                    return NWaitableResult.Completed;
                }
                return NWaitableResult.None;
            }
            return func;
        }
    }
    public readonly struct NWaitRoutine : IWaitableFromCancelable
    {
        public readonly NLoopType loopType;
        internal readonly IEnumerator routine;
        bool IWaitableFromCancelable.IsIgnoreFirstFrameCheck => true;

        NLoopType IWaitableFromCancelable.LoopType => loopType;

        public NWaitRoutine(IEnumerator routine, NLoopType loopType = NLoopType.Update)
        {
            this.routine = routine;
            this.loopType = loopType;
        }

        (Func<NWaitableResult>, ICancelable) IWaitableFromCancelable.buildCompleteFunc()
        {
            var data = NCoroutine.startCoroutine(routine);
            NWaitableResult func()
            {
                return data.Status switch
                {
                    RunState.Completed => NWaitableResult.Completed,
                    RunState.Canceled => NWaitableResult.Canceled,
                    _ => NWaitableResult.None,
                };
            }
            return (func, data);
        }
    }
    public readonly struct CombineNWaitable : IWaitable
    {
        public readonly NLoopType loopType;
        public readonly NWaitable[] waitables;

        NLoopType IWaitable.LoopType => loopType;
        bool IWaitable.IsIgnoreFirstFrameCheck => true;

        public CombineNWaitable(params NWaitable[] waitables)
        {
            if (waitables == null || waitables.Length == 0)
            {
                throw new Exception("waitables is null or empty");
            }
            this.waitables = waitables;
            loopType = NLoopType.Update;
        }

        Func<NWaitableResult> IWaitable.buildCompleteFunc()
        {
            var waitables = new List<NWaitable>(this.waitables);
            NWaitableResult func()
            {
                var span = waitables.asSpan();

                for (int i = waitables.Count - 1; i >= 0; --i)
                {
                    var waitable = span[i];
                    switch (waitable.Status)
                    {
                        case RunState.Canceled:
                            return NWaitableResult.Canceled;
                        case RunState.Exception:
                            return NWaitableResult.Exception(waitable.Exception);
                        case RunState.Completed:
                            waitables.removeAtSwapBack(i);
                            continue;
                        case RunState.Running:
                        case RunState.None:
                        default: return NWaitableResult.None;
                    }
                }
                return NWaitableResult.Completed;
            }
            return func;
        }
    }
    public readonly struct NWaitMainThread : IWaitable
    {
        readonly NLoopType IWaitable.LoopType => NLoopType.Update;
        bool IWaitable.IsIgnoreFirstFrameCheck => false;
        Func<NWaitableResult> IWaitable.buildCompleteFunc()
        {
            static NWaitableResult func()
            {
                if (Thread.CurrentThread.ManagedThreadId == NStartRunner.MainThreadId)
                {
                    return NWaitableResult.Completed;
                }
                return NWaitableResult.None;
            }
            return func;
        }
    }
    public readonly struct NWaitDeltaTime : IWaitable
    {
        NLoopType IWaitable.LoopType => NLoopType.Update;
        bool IWaitable.IsIgnoreFirstFrameCheck => false;

        public readonly uint timeMs;

        public NWaitDeltaTime(uint timeMs = 10)
        {
            this.timeMs = timeMs;
        }

        Func<NWaitableResult> IWaitable.buildCompleteFunc()
        {            
            return conditionCheck;
        }

        NWaitableResult conditionCheck()
        {
            var deltaTimeMs = NUpdater.CurrentTimeMs - NUpdater.LatestUpdatedTimeMs;
            if (deltaTimeMs <= timeMs)
            {
                return NWaitableResult.Completed;
            }
            return NWaitableResult.None;
        }
    }
}
