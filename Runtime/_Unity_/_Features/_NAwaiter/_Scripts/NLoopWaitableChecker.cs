
namespace Nextension
{
    internal class NLoopWaitableChecker
    {
        public static NLoopWaitableChecker create<T>(T waitable, NLoopWaitableAwaiter awaiter) where T : IWaitable
        {
            var checker = NPool<NLoopWaitableChecker>.Shared.Rent().value;
            checker._awaiter = awaiter;
            checker._resultGetter = NWaitableResultGetter.create<T>();
            checker._waitableCancelable = waitable.onStartWaitable(checker._resultGetter);
            return checker;
        }

        private NLoopWaitableChecker()
        {

        }

        private bool _isFinished;
        private NLoopWaitableAwaiter _awaiter;
        private NWaitableResultGetter _resultGetter;
        private ICancelable _waitableCancelable;

        public bool IsCreated => _awaiter != null;

        private bool __setState(NWaitableState state)
        {
            switch (state.state)
            {
                case CompleteState.Canceled:
                    {
                        _waitableCancelable?.cancel();
                        _isFinished = true;
                        _awaiter.setCompletionWithoutChecks(state);
                        return true;
                    }
                case CompleteState.Completed:
                case CompleteState.Exception:
                    {
                        _isFinished = true;
                        _awaiter.setCompletionWithoutChecks(state);
                        return true;
                    }
                case CompleteState.None:
                    {
                        return false;
                    }
                default:
                    {
                        NDebug.LogWarning("Not implement state: " + state);
                        return false;
                    }
            }
        }

        public bool checkComplete()
        {
            if (_isFinished)
            {
                return true;
            }

            var state = _resultGetter.getCurrentState();
            return __setState(state);
        }

        public void cancel()
        {
            if (!IsCreated || _isFinished)
            {
                return;
            }
            __setState(NWaitableState.Canceled);
        }

        public void release()
        {
            if (IsCreated)
            {
                _isFinished = false;
                _resultGetter.release();
                _resultGetter = null;
                _awaiter = null;
                _waitableCancelable = null;
                NPool<NLoopWaitableChecker>.Shared.Return(this);
            }
        }
    }
}