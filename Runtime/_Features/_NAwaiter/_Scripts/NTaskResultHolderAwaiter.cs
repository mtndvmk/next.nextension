using System;

namespace Nextension
{
    public sealed class NTaskResultHolderAwaiter : AbsPoolableAwaiter<NTaskResultHolderAwaiter>
    {
        public static NTaskResultHolderAwaiter create(NTaskResultHolder holder)
        {
            var awaiter = __getNext();
            awaiter.__setup(holder);
            return awaiter;
        }

        private NTaskResultHolder _holder;

        private NTaskResultHolderAwaiter() { }

        private void __setup(NTaskResultHolder holder)
        {
            _holder = holder;
            if (holder.IsFinished)
            {
                setAsCompletedAwaiter(holder.CurrentState);
            }
            else
            {
                holder.addAwaiter(this);
            }
        }

        protected override void __onFinalized()
        {
            base.__onFinalized();
            _holder = null;
            __release(this);
        }
    }

    public sealed class NTaskResultHolderAwaiter<T> : AbsPoolableAwaiter<NTaskResultHolderAwaiter<T>>
    {
        public static NTaskResultHolderAwaiter<T> create(NTaskResultHolder<T> holder)
        {
            var awaiter = __getNext();
            awaiter.__setup(holder);
            return awaiter;
        }

        private NTaskResultHolder<T> _holder;

        private NTaskResultHolderAwaiter() { }

        private void __setup(NTaskResultHolder<T> holder)
        {
            _holder = holder;
            if (holder.IsFinished)
            {
                setAsCompletedAwaiter(holder.CurrentState);
            }
            else
            {
                holder.addAwaiter(this);
            }
        }

        public new T GetResult()
        {
            try
            {
                __beforeGetResult();
                return _holder.CurrentState.result;
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
            _holder = null;
            __release(this);
        }
    }
}
