using System;
using System.Collections;
using System.Threading;
using Unity.Jobs;

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
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            this.predicate = predicate;
            this.loopType = loopType;
        }

        ICancelable IWaitable.onStartWaitable(NWaitableResultGetter getter)
        {
            getter.setWaitable(this);
            return null;
        }

        NWaitableState IWaitable.getCurrentState()
        {
            if (this.predicate())
            {
                return NWaitableState.Completed;
            }
            else
            {
                return NWaitableState.None;
            }
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

        ICancelable IWaitable.onStartWaitable(NWaitableResultGetter getter)
        {
            getter.setWaitable(this);
            return null;
        }

        NWaitableState IWaitable.getCurrentState()
        {
            return NWaitableState.None;
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

        ICancelable IWaitable.onStartWaitable(NWaitableResultGetter getter)
        {
            getter.setWaitable(this);
            return null;
        }

        NWaitableState IWaitable.getCurrentState()
        {
            if (waitSecond <= 0)
            {
                return NWaitableState.Completed;
            }
            else
            {
                return NWaitableState.None;
            }
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

        ICancelable IWaitable.onStartWaitable(NWaitableResultGetter getter)
        {
            getter.setWaitable(this);
            return null;
        }

        NWaitableState IWaitable.getCurrentState()
        {
            if (waitSecond <= 0)
            {
                return NWaitableState.Completed;
            }
            else
            {
                return NWaitableState.None;
            }
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
        ICancelable IWaitable.onStartWaitable(NWaitableResultGetter getter)
        {
            getter.setWaitable(this);
            return null;
        }

        NWaitableState IWaitable.getCurrentState()
        {
            if (jobHandle.IsCompleted)
            {
                jobHandle.Complete();
                return NWaitableState.Completed;
            }
            else
            {
                return NWaitableState.None;
            }
        }
    }
    public readonly struct NWaitRoutine : IWaitable
    {
        public readonly NLoopType loopType;
        internal readonly IEnumerator routine;
        bool IWaitable.IsIgnoreFirstFrameCheck => true;

        NLoopType IWaitable.LoopType => loopType;

        public NWaitRoutine(IEnumerator routine, NLoopType loopType = NLoopType.Update)
        {
            this.routine = routine;
            this.loopType = loopType;
        }
        ICancelable IWaitable.onStartWaitable(NWaitableResultGetter getter)
        {
            getter.setWaitable(this, out var routine);
            return routine;
        }

        NWaitableState IWaitable.getCurrentState()
        {
            return NWaitableState.None;
        }
    }
    public readonly struct NWaitMainThread : IWaitable
    {
        readonly NLoopType IWaitable.LoopType => NLoopType.Update;
        bool IWaitable.IsIgnoreFirstFrameCheck => false;
        ICancelable IWaitable.onStartWaitable(NWaitableResultGetter getter)
        {
            getter.setWaitable(this);
            return null;
        }

        NWaitableState IWaitable.getCurrentState()
        {
            if (Thread.CurrentThread.ManagedThreadId == NStartRunner.MainThreadId)
            {
                return NWaitableState.Completed;
            }
            return NWaitableState.None;
        }
    }
    public readonly struct NCustomWaitable : IWaitable
    {
        public readonly NLoopType loopType;
        internal readonly ICustomWaitable waitable;
        NLoopType IWaitable.LoopType => loopType;
        bool IWaitable.IsIgnoreFirstFrameCheck => true;
        public NCustomWaitable(ICustomWaitable waitable, NLoopType loopType = NLoopType.Update)
        {
            this.waitable = waitable;
            this.loopType = loopType;
        }
        ICancelable IWaitable.onStartWaitable(NWaitableResultGetter getter)
        {
            getter.setWaitable(waitable);
            if (waitable is ICancelable cancelable)
            {
                return cancelable;
            }
            return null;
        }
        NWaitableState IWaitable.getCurrentState()
        {
            return waitable.getCurrentState();
        }
    }
}
