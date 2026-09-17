using System;
using System.Collections.Generic;
using System.Threading;
using Unity.Jobs;

namespace Nextension
{
    internal unsafe abstract class NWaitableResultGetter
    {
        public abstract NWaitableState getCurrentState();

        private class ResultGetter_WaitUntil : NWaitableResultGetter
        {
            public static NWaitableResultGetter get() => NPool<ResultGetter_WaitUntil>.Shared.Rent().value;
            private Func<bool> _predicate;
            public void set(Func<bool> predicate) => _predicate = predicate;
            public override void release()
            {
                _predicate = null;
                NPool<ResultGetter_WaitUntil>.Shared.Return(this);
            }
            public override NWaitableState getCurrentState()
            {
                if (_predicate())
                {
                    return NWaitableState.Completed;
                }
                return NWaitableState.None;
            }
        }

        private class ResultGetter_WaitFrame : NWaitableResultGetter
        {
            public static NWaitableResultGetter get() => NPool<ResultGetter_WaitFrame>.Shared.Rent().value;
            private uint _targetFrame;
            public void set(uint targetFrame) => _targetFrame = targetFrame;
            public override void release()
            {
                NPool<ResultGetter_WaitFrame>.Shared.Return(this);
            }
            public override NWaitableState getCurrentState()
            {
                if (NUpdater.UpdateCount >= _targetFrame)
                {
                    return NWaitableState.Completed;
                }
                return NWaitableState.None;
            }
        }

        private class ResultGetter_WaitSecond : NWaitableResultGetter
        {
            public static NWaitableResultGetter get() => NPool<ResultGetter_WaitSecond>.Shared.Rent().value;
            private float _targetSecond;
            public void set(float targetSecond) => _targetSecond = targetSecond;
            public override void release()
            {
                NPool<ResultGetter_WaitSecond>.Shared.Return(this);
            }
            public override NWaitableState getCurrentState()
            {
                if (NUpdater.UnityTime >= _targetSecond)
                {
                    return NWaitableState.Completed;
                }
                return NWaitableState.None;
            }
        }

        private class ResultGetter_WaitRealtimeSecond : NWaitableResultGetter
        {
            public static NWaitableResultGetter get() => NPool<ResultGetter_WaitRealtimeSecond>.Shared.Rent().value;
            private float _targetSecond;
            public void set(float targetSecond) => _targetSecond = targetSecond;
            public override void release()
            {
                NPool<ResultGetter_WaitRealtimeSecond>.Shared.Return(this);
            }
            public override NWaitableState getCurrentState()
            {
                if (NUpdater.CurrentTime >= _targetSecond)
                {
                    return NWaitableState.Completed;
                }
                return NWaitableState.None;
            }
        }

        private class ResultGetter_WaitJobHandle : NWaitableResultGetter
        {
            public static NWaitableResultGetter get() => NPool<ResultGetter_WaitJobHandle>.Shared.Rent().value;
            private JobHandle _jobHandle;
            public void set(JobHandle jobHandle) => _jobHandle = jobHandle;
            public override void release()
            {
                NPool<ResultGetter_WaitJobHandle>.Shared.Return(this);
            }
            public override NWaitableState getCurrentState()
            {
                if (_jobHandle.IsCompleted)
                {
                    _jobHandle.Complete();
                    return NWaitableState.Completed;
                }
                return NWaitableState.None;
            }
        }

        private class ResultGetter_WaitRoutine : NWaitableResultGetter
        {
            public static NWaitableResultGetter get() => NPool<ResultGetter_WaitRoutine>.Shared.Rent().value;
            private NCoroutine.Data _routine;
            public void set(NCoroutine.Data routine) => _routine = routine;
            public override void release()
            {
                _routine = default;
                NPool<ResultGetter_WaitRoutine>.Shared.Return(this);
            }
            public override NWaitableState getCurrentState()
            {
                return _routine.Status switch
                {
                    RunState.Completed => NWaitableState.Completed,
                    RunState.Canceled => NWaitableState.Canceled,
                    _ => NWaitableState.None,
                };
            }
        }

        private class ResultGetter_WaitMainThread : NWaitableResultGetter
        {
            public static NWaitableResultGetter get() => NPool<ResultGetter_WaitMainThread>.Shared.Rent().value;
            public override void release()
            {
                NPool<ResultGetter_WaitMainThread>.Shared.Return(this);
            }
            public override NWaitableState getCurrentState()
            {
                if (Thread.CurrentThread.ManagedThreadId == NStartRunner.MainThreadId)
                {
                    return NWaitableState.Completed;
                }
                return NWaitableState.None;
            }
        }

        private class ResultGetter_CustomWaitable : NWaitableResultGetter
        {
            public static NWaitableResultGetter get() => NPool<ResultGetter_CustomWaitable>.Shared.Rent().value;
            private CustomWaitable _customWaitable;
            public void set(CustomWaitable customWaitable) => _customWaitable = customWaitable;
            public override void release()
            {
                _customWaitable = null;
                NPool<ResultGetter_CustomWaitable>.Shared.Return(this);
            }
            public override NWaitableState getCurrentState() => _customWaitable.getCurrentState();
        }

        private class ResultGetter_AsyncOperation : NWaitableResultGetter
        {
            public static NWaitableResultGetter get() => NPool<ResultGetter_AsyncOperation>.Shared.Rent().value;
            private UnityEngine.AsyncOperation _asyncOperation;
            public void set(UnityEngine.AsyncOperation asyncOperation) => _asyncOperation = asyncOperation;
            public override void release()
            {
                _asyncOperation = null;
                NPool<ResultGetter_AsyncOperation>.Shared.Return(this);
            }
            public override NWaitableState getCurrentState()
            {
                return _asyncOperation.isDone ? NWaitableState.Completed : NWaitableState.None;
            }
        }

        private static readonly Dictionary<Type, IntPtr> _createTable;

        static NWaitableResultGetter()
        {
            _createTable = new()
            {
                {typeof(NWaitUntil), (IntPtr)(delegate* <NWaitableResultGetter>)&ResultGetter_WaitUntil.get},
                {typeof(NWaitFrame), (IntPtr)(delegate* <NWaitableResultGetter>)&ResultGetter_WaitFrame.get},
                {typeof(NWaitSecond), (IntPtr)(delegate* <NWaitableResultGetter>)&ResultGetter_WaitSecond.get},
                {typeof(NWaitRealtimeSecond), (IntPtr)(delegate* <NWaitableResultGetter>)&ResultGetter_WaitRealtimeSecond.get},
                {typeof(NWaitJobHandle), (IntPtr)(delegate* <NWaitableResultGetter>)&ResultGetter_WaitJobHandle.get},
                {typeof(NWaitMainThread), (IntPtr)(delegate* <NWaitableResultGetter>)&ResultGetter_WaitMainThread.get},
                {typeof(NWaitRoutine), (IntPtr)(delegate* <NWaitableResultGetter>)&ResultGetter_WaitRoutine.get},
                {typeof(NWaitAsyncOperation), (IntPtr)(delegate* <NWaitableResultGetter>)&ResultGetter_AsyncOperation.get},
                {typeof(NCustomWaitable), (IntPtr)(delegate* <NWaitableResultGetter>)&ResultGetter_CustomWaitable.get},
            };
        }

        public static NWaitableResultGetter create<T>() where T : IWaitable
        {
            return ((delegate*<NWaitableResultGetter>)_createTable[typeof(T)])();
        }

        public abstract void release();

        public void setWaitable(NWaitUntil waitable)
        {
            var getter = (ResultGetter_WaitUntil)this;
            getter.set(waitable.predicate);
        }

        public void setWaitable(NWaitFrame waitable)
        {
            var getter = (ResultGetter_WaitFrame)this;
            getter.set(NUpdater.UpdateCount + waitable.waitFrame);
        }

        public void setWaitable(NWaitSecond waitable)
        {
            var getter = (ResultGetter_WaitSecond)this;
            getter.set(NUpdater.UnityTime + waitable.waitSecond);
        }

        public void setWaitable(NWaitRealtimeSecond waitable)
        {
            var getter = (ResultGetter_WaitRealtimeSecond)this;
            getter.set(NUpdater.CurrentTime + waitable.waitSecond);
        }

        public void setWaitable(NWaitJobHandle waitable)
        {
            var getter = (ResultGetter_WaitJobHandle)this;
            getter.set(waitable.jobHandle);
        }

        public void setWaitable(NWaitMainThread _)
        {
        }

        public void setWaitable(NWaitRoutine waitable, out NCoroutine.Data routine)
        {
            routine = NCoroutine.startCoroutine(waitable.routine);
            var getter = (ResultGetter_WaitRoutine)this;
            getter.set(routine);
        }

        public void setWaitable(NWaitAsyncOperation waitable)
        {
            var getter = (ResultGetter_AsyncOperation)this;
            getter.set(waitable.asyncOperation);
        }

        public void setWaitable(CustomWaitable customWaitable)
        {
            var getter = (ResultGetter_CustomWaitable)this;
            getter.set(customWaitable);
        }
    }
}
