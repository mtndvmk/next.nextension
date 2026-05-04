using System;

namespace Nextension
{
    public class NLoopWaitableAwaiter : AbsPoolableAwaiter<NLoopWaitableAwaiter>, ICancelable
    {
        private NLoopWaitableChecker _checker;
        public static NLoopWaitableAwaiter create<T>(T waitable) where T : IWaitable
        {
            var awaiter = __getNext();
            awaiter.setup_Waitable(waitable);
            return awaiter;
        }
        protected void setup_Waitable<T>(T waitable) where T : IWaitable
        {
            try
            {
                var state = waitable.getCurrentState();
                switch (state.state)
                {
                    case CompleteState.None:
                        {
                            _checker = NLoopWaitableChecker.create(waitable, this);
                            NLoopWaitableCheckerManager.addChecker(_checker, waitable.LoopType, waitable.IsIgnoreFirstFrameCheck);
                            return;
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
            catch (Exception e)
            {
                setCompletion(Id, NWaitableState.Exception(e));
            }
        }
        public void cancel()
        {
            _checker.cancel();
            setCompletion(Id, NWaitableState.Canceled);
        }
        protected override void __onFinalized()
        {
            base.__onFinalized();
            _checker = null;
            __release(this);
        }
    }
}
