using System;

namespace Nextension.Tween
{
    public struct NWaitTweener : IWaitableFromCancelable
    {
        private NTweener _tweener;

        NLoopType IWaitableFromCancelable.LoopType => NLoopType.Update;
        bool IWaitableFromCancelable.IsIgnoreFirstFrameCheck => true;

        public NWaitTweener(NTweener tweener)
        {
            _tweener = tweener;
        }

        (Func<NWaitableResult>, ICancelable) IWaitableFromCancelable.buildCompleteFunc()
        {
            var tweener = _tweener;
            NWaitableResult func()
            {
                return tweener.Status switch
                {
                    RunState.Completed => NWaitableResult.Completed,
                    RunState.Canceled => NWaitableResult.Canceled,
                    _ => NWaitableResult.None,
                };
            }
            return (func, tweener);
        }
    }
}
