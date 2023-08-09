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

        Func<NWaitableResult> IWaitable_Editor.buildCompleteFunc()
        {
            var predicate = this.predicate;
            Func<NWaitableResult> func = () =>
            {
                if (predicate())
                {
                    return NWaitableResult.Completed;
                }
                return NWaitableResult.None;
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

        Func<NWaitableResult> IWaitable_Editor.buildCompleteFunc()
        {
            var targetFrame = NAwaiter_EdtitorLoop.UpdateCount + waitFrame;
            Func<NWaitableResult> func = () =>
            {
                if (NAwaiter_EdtitorLoop.UpdateCount >= targetFrame)
                {
                    return NWaitableResult.Completed;
                }
                return NWaitableResult.None;
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

        Func<NWaitableResult> IWaitable_Editor.buildCompleteFunc()
        {
            var targetSecond = NAwaiter_EdtitorLoop.CurrentTime + waitSecond;
            Func<NWaitableResult> func = () =>
            {
                if (NAwaiter_EdtitorLoop.CurrentTime >= targetSecond)
                {
                    return NWaitableResult.Completed;
                }
                return NWaitableResult.None;
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

        Func<NWaitableResult> IWaitable_Editor.buildCompleteFunc()
        {
            var jobHandle = this.jobHandle;
            Func<NWaitableResult> func = () =>
            {
                if (jobHandle.IsCompleted)
                {
                    jobHandle.Complete();
                    return NWaitableResult.Completed;
                }
                return NWaitableResult.None;
            };
            return func;
        }
    }
}
#endif