using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Nextension
{

    [AsyncMethodBuilder(typeof(AsyncNTaskBuilder))]
    public readonly struct NTask
    {
        internal readonly uint id;

        internal NTask(uint id)
        {
            this.id = id;
            NTaskManager.addInCompleteTask(id);
        }
        public readonly bool IsCreated => id != 0;
        public readonly bool IsInProgress
        {
            get
            {
                return NTaskManager.containsInCompleteTask(id);
            }
        }

        public Exception Exception
        {
            get
            {
                var result = getCurrentState();
                if (!result.state.isFinished())
                {
                    throw new Exception($"NTask[{id}] is not complete"); 
                }
                return result.exception;
            }
        }

        public readonly NWaitableState getCurrentState()
        {
            if (!NTaskManager.tryGetCurrentState(id, out var result))
            {
                throw new Exception($"Can not found NTask[{id}]");
            }
            return result;
        }

        public readonly NTaskResultHolder createHolder(bool forget = true)
        {
            var holder = NTaskManager.createHolder(id);
            if (forget)
            {
                this.forget();
            }
            return holder;
        }

        public readonly void createHolderNonAlloc(NTaskResultHolder holder, bool forget = true)
        {
            NTaskManager.createHolderNonAlloc(holder, id);
            if (forget)
            {
                this.forget();
            }
        }

        public readonly void tryCancel()
        {
            if (!IsCreated || !IsInProgress)
            {
                return;
            }
            NTaskManager.cancel(id);
        }

        public readonly void cancel()
        {
            if (!IsCreated || !IsInProgress)
            {
                Debug.LogWarning($"Can not cancel NTask[{id}]: not exists or not in progress");
                return;
            }
            NTaskManager.cancel(id);
        }
        public readonly void forget()
        {
            NTaskManager.forget(id);
        }

        private readonly static NTask _completedTask = new NTask(NTaskManager.CompleteId);
        public static NTask CompletedTask => _completedTask;
        public static NTask<T> fromResult<T>(T result)
        {
            var id = NTaskManager.nextId();
            var nTask = new NTask<T>(id);
            NTaskGenericManager.setFromCompletedResult(id, result);
            return nTask;
        }

        private readonly static NWaitFrame _yieldWaitable = new NWaitFrame(1);
        public static async NTask yield()
        {
            await _yieldWaitable;
        }
        public static async NTask waitAndRunAsync<T>(T waitable, Action next) where T : IWaitable
        {
            await waitable;
            next.Invoke();
        }
        public static NTask waitAndRunAsync(float second, Action next)
        {
            return waitAndRunAsync(new NWaitSecond(second), next);
        }
        public static void waitAndRun<T>(T waitable, Action next) where T : IWaitable
        {
            waitAndRunAsync(waitable, next).forget();
        }
        public static void waitAndRun(float second, Action next)
        {
            waitAndRunAsync(new NWaitSecond(second), next).forget();
        }
    }

    [AsyncMethodBuilder(typeof(AsyncNTaskBuilder<>))]
    public readonly struct NTask<T>
    {
        internal readonly uint id;

        internal NTask(uint id)
        {
            this.id = id;
            NTaskManager.addInCompleteTask(id);
        }

        public readonly bool IsCreated => id != 0;
        public readonly bool IsInProgress
        {
            get
            {
                return NTaskManager.containsInCompleteTask(id);
            }
        }
        public readonly Exception Exception
        {
            get
            {
                var result = getCurrentState();
                if (result.state.isFinished())
                {
                    return result.exception;
                }
                throw new Exception($"NTask<{typeof(T)}>[{id}] is not complete");
            }
        }
        public readonly T Result
        {
            get
            {
                var result = getCurrentState();
                if (result.state == CompleteState.Completed)
                {
                    return result.result;
                }
                throw new Exception($"NTask<{typeof(T)}>[{id}] is not completed: " + result.state);
            }
        }

        public readonly NWaitableState<T> getCurrentState()
        {
            if (!NTaskGenericManager.tryGetCurrentState<T>(id, out var result))
            {
                throw new Exception($"Can not found NTask<{typeof(T)}>[{id}]");
            }
            return result;
        }

        public readonly NTaskResultHolder<T> createHolder(bool forget = true)
        {
            var holder = NTaskGenericManager.createHolder<T>(id);
            if (forget)
            {
                this.forget();
            }
            return holder;
        }
        public readonly void createHolderNonAlloc(NTaskResultHolder<T> holder, bool forget = true)
        {
            NTaskGenericManager.createHolderNonAlloc(holder, id);
            if (forget)
            {
                this.forget();
            }
        }
        public readonly void cancel()
        {
            if (!IsCreated || !IsInProgress)
            {
                Debug.LogWarning($"Can not cancel NTask<{typeof(T)}>[{id}]: not exists or not in progress");
                return;
            }
            NTaskGenericManager.cancel<T>(id);
        }
        public readonly void tryCancel()
        {
            if (!IsCreated || !IsInProgress)
            {
                return;
            }
            NTaskManager.cancel(id);
        }
        public readonly void forget()
        {
            NTaskGenericManager.forget<T>(id);
        }
    }
}
