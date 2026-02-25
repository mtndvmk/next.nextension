using UnityEngine;

namespace Nextension
{
    internal class NLoopWaitableChecker
    {
        private static NLoopWaitableChecker __getNext()
        {
            var checker = NLockedPool<NLoopWaitableChecker>.get();
            return checker;
        }
        private static void __release(NLoopWaitableChecker checker)
        {
            NLockedPool<NLoopWaitableChecker>.release(checker);
        }
        public static NLoopWaitableChecker create<T>(T awaitable, NLoopWaitableAwaiter awaiter) where T : IWaitable
        {
            var checker = __getNext();
            checker.setup(awaiter, awaitable);
            return checker;
        }

        private NLoopWaitableChecker()
        {

        }
        private void setup<T>(NLoopWaitableAwaiter awaiter, T waitable) where T : IWaitable
        {
            this._awaiter = awaiter;
            _waitableCancelable = waitable.onStartWaitable(_resultGetter);
        }

        private bool _isFinished;
        private NWaitableResultGetter _resultGetter = new();
        private ICancelable _waitableCancelable;
        private NLoopWaitableAwaiter _awaiter;

        public bool IsCreated => _awaiter != null;

        private bool __setState(NWaitableState state)
        {
            switch (state.state)
            {
                case CompleteState.Canceled:
                    {
                        _isFinished = true;
                        _waitableCancelable?.cancel();
                        _awaiter.setCompletion(_awaiter.Id, state);
                        return true;
                    }
                case CompleteState.Completed:
                    {
                        _isFinished = true;
                        _awaiter.setCompletion(_awaiter.Id, state);
                        return true;
                    }
                case CompleteState.Exception:
                    {
                        _isFinished = true;
                        _awaiter.setCompletion(_awaiter.Id, state);
                        return true;
                    }
                case CompleteState.None:
                    {
                        return false;
                    }
                default:
                    {
                        Debug.LogWarning("Not implement state: " + state);
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
                _resultGetter.reset();
                _awaiter = null;
                _waitableCancelable = null;
                __release(this);
            }
        }
    }
}
