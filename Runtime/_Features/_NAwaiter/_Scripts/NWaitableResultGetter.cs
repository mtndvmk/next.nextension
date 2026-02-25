using System;
using System.Threading;
using Unity.Jobs;

namespace Nextension
{
    internal class NWaitableResultGetter
    {
        private enum FuncType
        {
            None = 0,
            WaitUntil,
            WaitFrame,
            WaitSecond,
            WaitRealtimeSecond,
            WaitJobHandle,
            WaitRoutine,
            WaitMainThread,
            Waitable,
        }

        private static float __getCurrentTime()
        {
            return NUpdater.UnityTime;
        }
        private static float __getRealtimeSecondMs()
        {
            return NUpdater.CurrentTime;
        }

        public void reset()
        {
            funcType = FuncType.None;
            predicate = null;
            routine = null;
            customWaitable = null;
        }

        public NWaitableState getCurrentState()
        {
            try
            {
                switch (funcType)
                {
                    case FuncType.None:
                        {
                            return NWaitableState.None;
                        }
                    case FuncType.WaitUntil:
                        {
                            return __getResult_WaitUntil(predicate);
                        }
                    case FuncType.WaitFrame:
                        {
                            return __getResult_WaitFrame(targetFrame);
                        }
                    case FuncType.WaitSecond:
                        {
                            return __getResult_WaitSecond(targetSecond);
                        }
                    case FuncType.WaitRealtimeSecond:
                        {
                            return __getResult_WaitRealtimeSecond(targetSecond);
                        }
                    case FuncType.WaitJobHandle:
                        {
                            return __getResult_WaitJobHandle(jobHandle);
                        }
                    case FuncType.WaitRoutine:
                        {
                            return __getResult_WaitRoutine(routine);
                        }
                    case FuncType.WaitMainThread:
                        {
                            return __getResult_WaitMainThread();
                        }
                    case FuncType.Waitable:
                        {
                            return __getResult_Waitable(customWaitable);
                        }
                    default:
                        {
                            throw new NotImplementedException($"FuncType {funcType} is not implemented");
                        }

                }
            }
            catch (Exception e)
            {
                return NWaitableState.Exception(e);
            }
        }

        private static NWaitableState __getResult_WaitUntil(Func<bool> predicate)
        {
            if (predicate())
            {
                return NWaitableState.Completed;
            }
            return NWaitableState.None;
        }
        private static NWaitableState __getResult_WaitFrame(uint targetFrame)
        {
            if (NUpdater.UpdateCount >= targetFrame)
            {
                return NWaitableState.Completed;
            }
            return NWaitableState.None;
        }
        private static NWaitableState __getResult_WaitSecond(float targetSecond)
        {
            if (__getCurrentTime() >= targetSecond)
            {
                return NWaitableState.Completed;
            }
            return NWaitableState.None;
        }
        private static NWaitableState __getResult_WaitRealtimeSecond(float targetSecond)
        {
            if (__getRealtimeSecondMs() >= targetSecond)
            {
                return NWaitableState.Completed;
            }
            return NWaitableState.None;
        }
        private static NWaitableState __getResult_WaitJobHandle(JobHandle jobHandle)
        {
            if (jobHandle.IsCompleted)
            {
                jobHandle.Complete();
                return NWaitableState.Completed;
            }
            return NWaitableState.None;
        }
        private static NWaitableState __getResult_WaitRoutine(NCoroutine.Data routine)
        {
            return routine.Status switch
            {
                RunState.Completed => NWaitableState.Completed,
                RunState.Canceled => NWaitableState.Canceled,
                _ => NWaitableState.None,
            };
        }
        private static NWaitableState __getResult_WaitMainThread()
        {
            if (Thread.CurrentThread.ManagedThreadId == NStartRunner.MainThreadId)
            {
                return NWaitableState.Completed;
            }
            return NWaitableState.None;
        }
        private static NWaitableState __getResult_Waitable(ICustomWaitable customWaitable)
        {
            return customWaitable.getCurrentState();
        }

        private FuncType funcType;

        private Func<bool> predicate;

        private uint targetFrame;

        private float targetSecond;

        private JobHandle jobHandle;

        private NCoroutine.Data routine;

        private ICustomWaitable customWaitable;

        public void setWaitable(NWaitUntil waitable)
        {
            this.funcType = FuncType.WaitUntil;
            this.predicate = waitable.predicate;
        }
        public void setWaitable(NWaitFrame waitable)
        {
            this.funcType = FuncType.WaitFrame;
            this.targetFrame = NUpdater.UpdateCount + (waitable.waitFrame == 0 ? 1 : waitable.waitFrame);
        }
        public void setWaitable(NWaitSecond waitable)
        {
            this.funcType = FuncType.WaitSecond;
            this.targetSecond = __getCurrentTime() + waitable.waitSecond;
        }
        public void setWaitable(NWaitRealtimeSecond waitable)
        {
            this.funcType = FuncType.WaitRealtimeSecond;
            this.targetSecond = __getCurrentTime() + waitable.waitSecond;
        }
        public void setWaitable(NWaitJobHandle waitable)
        {
            this.funcType = FuncType.WaitJobHandle;
            this.jobHandle = waitable.jobHandle;
        }
        public void setWaitable(NWaitMainThread waitable)
        {
            this.funcType = FuncType.WaitMainThread;
        }
        public void setWaitable(NWaitRoutine waitable, out NCoroutine.Data routine)
        {
            this.funcType = FuncType.WaitRoutine;
            routine = this.routine = NCoroutine.startCoroutine(waitable.routine);
        }
        public void setWaitable(ICustomWaitable customWaitable)
        {
            this.funcType = FuncType.Waitable;
            this.customWaitable = customWaitable;
        }
    }
}
