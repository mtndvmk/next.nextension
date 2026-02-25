using System;

namespace Nextension
{
    public class NTaskResultHolder
    {
        private uint _taskId;
        private NWaitableState _state;
        private AbsAwaiter _awaiter;

        internal NTaskResultHolder(uint taskId)
        {
            _taskId = taskId;
            _state = NWaitableState.None;
        }

        internal NTaskResultHolder(uint taskId, NWaitableState state)
        {
            _taskId = taskId;
            _state = state;
        }

        internal void reuse(uint taskId, NWaitableState state)
        {
            _taskId = taskId;
            _state = state;
        }

        public uint TaskId => _taskId;
        public bool IsFinished => _state.state.isFinished();
        public NWaitableState CurrentState => _state;

        internal void setResultState(NWaitableState state)
        {
            if (_state.state.isFinished())
            {
                throw new Exception($"NTaskResultHolder[{_taskId}] has already been completed");
            }
            _state = state;
            if (_awaiter != null)
            {
                _awaiter.setCompletion(_awaiter.Id, state);
                _awaiter = null;
            }
        }

        internal void addAwaiter(AbsAwaiter awaiter)
        {
            if (IsFinished)
            {
                awaiter.setCompletion(awaiter.Id, _state);
                return;
            }
            if (_awaiter != null)
            {
                throw new Exception($"NTaskResultHolder[{_taskId}] has already been awaited");
            }
            _awaiter = awaiter;
        }

        public NTaskResultHolderAwaiter GetAwaiter()
        {
            return NTaskResultHolderAwaiter.create(this);
        }

        public void cancel()
        {
            if (_state.state.isFinished())
            {
                return;
            }
            NTaskManager.cancel(_taskId);
        }
    }

    public class NTaskResultHolder<T>
    {
        private uint _taskId;
        private NWaitableState<T> _state;
        private AbsAwaiter _awaiter;

        internal NTaskResultHolder(uint taskId)
        {
            _taskId = taskId;
            _state = NWaitableState<T>.None;
        }

        internal NTaskResultHolder(uint taskId, NWaitableState<T> state)
        {
            _taskId = taskId;
            _state = state;
        }

        internal void reuse(uint taskId, NWaitableState<T> state)
        {
            _taskId = taskId;
            _state = state;
        }

        public uint TaskId => _taskId;
        public bool IsFinished => _state.state.isFinished();
        public NWaitableState<T> CurrentState => _state;

        internal void setResultState(NWaitableState<T> state)
        {
            if (_state.state.isFinished())
            {
                throw new Exception($"NTaskResultHolder<{typeof(T)}>[{_taskId}] has already been completed");
            }
            _state = state;
            if (_awaiter != null)
            {
                _awaiter.setCompletion(_awaiter.Id, state);
                _awaiter = null;
            }
        }

        internal void addAwaiter(AbsAwaiter awaiter)
        {
            if (IsFinished)
            {
                awaiter.setCompletion(awaiter.Id, _state);
                return;
            }
            if (_awaiter != null)
            {
                throw new Exception($"NTaskResultHolder<T>[{_taskId}] has already been awaited");
            }
            _awaiter = awaiter;
        }

        public NTaskResultHolderAwaiter<T> GetAwaiter()
        {
            return NTaskResultHolderAwaiter<T>.create(this);
        }
        public void cancel()
        {
            if (_state.state.isFinished())
            {
                return;
            }
            NTaskGenericManager.cancel<T>(_taskId);
        }
    }
}
