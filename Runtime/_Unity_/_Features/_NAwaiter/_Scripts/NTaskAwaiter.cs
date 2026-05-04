using System;

namespace Nextension
{

    public sealed class NTaskAwaiter : AbsPoolableAwaiter<NTaskAwaiter>, ICancelable
    {
        public static NTaskAwaiter create(NTask task)
        {
            var awaiter = __getNext();
            var state = task.getCurrentState();
            awaiter.__setup(task, state);
            return awaiter;
        }

        private NTask _task;

        private NTaskAwaiter() { }

        private void __setup(NTask task, NWaitableState state)
        {
            _task = task;
            switch (state.state)
            {
                case CompleteState.None:
                    {
                        // Register this awaiter so NTask can notify us when it finishes
                        if (!NTaskManager.tryAddAwaiter(_task.id, this))
                        {
                            throw new Exception($"NTask[{_task.id}] has already been awaited");
                        }
                        break;
                    }
                case CompleteState.Canceled:
                case CompleteState.Completed:
                case CompleteState.Exception:
                    {
                        setAsCompletedAwaiter(state);
                        break;
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        void ICancelable.cancel()
        {
            _task.tryCancel();
        }

        protected override void __onFinalized()
        {
            base.__onFinalized();
            if (_task.IsCreated)
            {
                NTaskManager.removeTaskResult(_task.id);
            }
            _task = default;
            __release(this);
        }

    }
    public sealed class NTaskAwaiter<T> : AbsPoolableAwaiter<NTaskAwaiter<T>>, ICancelable
    {
        public static NTaskAwaiter<T> create(NTask<T> task)
        {
            var awaiter = __getNext();
            var state = task.getCurrentState();
            awaiter.__setup(task, state);
            return awaiter;
        }

        private NTask<T> _task;
        private NTaskAwaiter() { }
        private void __setup(NTask<T> task, NWaitableState state)
        {
            _task = task;
            switch (state.state)
            {
                case CompleteState.None:
                    {
                        // Register this awaiter so NTask<T> can notify us when it finishes
                        if (!NTaskManager.tryAddAwaiter(task.id, this))
                        {
                            throw new Exception($"NTask<T>[{task.id}] has already been awaited");
                        }
                        break;
                    }
                case CompleteState.Canceled:
                case CompleteState.Completed:
                case CompleteState.Exception:
                    {
                        setAsCompletedAwaiter(state);
                        break;
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        void ICancelable.cancel()
        {
            _task.tryCancel();
        }
        public new T GetResult()
        {
            try
            {
                base.__beforeGetResult();
                var result = _task.getCurrentState().result;
                return result;
            }
            finally
            {
                if (_isCompletedAwaiter)
                {
                    __onFinalized();
                }
            }
        }
        protected override void __onFinalized()
        {
            base.__onFinalized();
            if (_task.IsCreated)
            {
                NTaskGenericManager.removeTaskResult<T>(_task.id);
            }
            _task = default;
            __release(this);
        }
    }
}
