#if UNITY_EDITOR
using System;
using Unity.Jobs;

namespace Nextension
{
    public struct NWaitUntil_Editor : IWaitable_Editor
    {
        internal readonly Func<bool> predicate;
        public NWaitUntil_Editor(Func<bool> predicate)
        {
            this.predicate = predicate;
        }

        bool IWaitable.IsWaitable => predicate != null;

        Func<CompleteState> IWaitable.buildCompleteFunc()
        {
            var predicate = this.predicate;
            Func<CompleteState> func = () =>
            {
                if (predicate())
                {
                    return CompleteState.Completed;
                }
                return CompleteState.None;
            };
            return func;
        }
    }
    public struct NWaitFrame_Editor : IWaitable_Editor
    {
        internal readonly uint waitFrame;
        public NWaitFrame_Editor(uint waitFrame)
        {
            this.waitFrame = waitFrame;
        }

        bool IWaitable.IsWaitable => waitFrame > 0;

        Func<CompleteState> IWaitable.buildCompleteFunc()
        {
            var targetFrame = NAwaiter_EdtitorLoop.UpdateCount + waitFrame;
            Func<CompleteState> func = () =>
            {
                if (NAwaiter_EdtitorLoop.UpdateCount >= targetFrame)
                {
                    return CompleteState.Completed;
                }
                return CompleteState.None;
            };
            return func;
        }
    }
    public struct NWaitSecond_Editor : IWaitable_Editor
    {
        internal readonly float waitSecond;
        public NWaitSecond_Editor(float waitSecond)
        {
            this.waitSecond = waitSecond;
        }

        bool IWaitable.IsWaitable => waitSecond > 0;

        Func<CompleteState> IWaitable.buildCompleteFunc()
        {
            var targetSecond = NAwaiter_EdtitorLoop.CurrentTime + waitSecond;
            Func<CompleteState> func = () =>
            {
                if (NAwaiter_EdtitorLoop.CurrentTime >= targetSecond)
                {
                    return CompleteState.Completed;
                }
                return CompleteState.None;
            };
            return func;
        }
    }
    public struct NWaitJobHandle_Editor : IWaitable_Editor
    {
        internal readonly JobHandle jobHandle;
        public NWaitJobHandle_Editor(JobHandle jobHandle)
        {
            this.jobHandle = jobHandle;
        }

        bool IWaitable.IsWaitable => !jobHandle.Equals(default);

        Func<CompleteState> IWaitable.buildCompleteFunc()
        {
            var jobHandle = this.jobHandle;
            Func<CompleteState> func = () =>
            {
                if (jobHandle.IsCompleted)
                {
                    jobHandle.Complete();
                    return CompleteState.Completed;
                }
                return CompleteState.None;
            };
            return func;
        }
    }
}
#endif