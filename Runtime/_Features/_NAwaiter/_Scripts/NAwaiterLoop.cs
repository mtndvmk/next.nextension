using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nextension
{
    internal class NAwaiterLoop
    {
        [EditorQuittingMethod]
        private void reset()
        {
            _updateWaitableHandles?.Clear();
            _lateUpdateWaitableHandles?.Clear();
            _endOfFrameWaitableHandles?.Clear();
        }
        private static List<NWaitableHandle> _updateWaitableHandles;
        private static List<NWaitableHandle> _lateUpdateWaitableHandles;
        private static List<NWaitableHandle> _endOfFrameWaitableHandles;

        public static void addAwaitable(NWaitableHandle waitable)
        {
            switch (waitable.loopType)
            {
                case NLoopType.Update:
                    {
                        (_updateWaitableHandles ??= new(1)).Add(waitable);
                        break;
                    }
                case NLoopType.LateUpdate:
                    {
                        (_lateUpdateWaitableHandles ??= new(1)).Add(waitable);
                        break;
                    }
                case NLoopType.EndOfFrameUpdate:
                    {
                        (_endOfFrameWaitableHandles ??= new(1)).Add(waitable);
                        break;
                    }
            }
        }

        private static void update(List<NWaitableHandle> handleList)
        {
            var handleSpan = handleList.asSpan();
            for (int i = handleSpan.Length - 1; i >= 0; i--)
            {
                var handle = handleSpan[i];
                try
                {
                    var isFinished = handle.checkComplete().isFinished();
                    if (isFinished)
                    {
                        NWaitableHandle.Factory.release(handle);
                        handleList.removeAtSwapBack(i);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    NWaitableHandle.Factory.release(handle);
                    handleList.removeAtSwapBack(i);
                }
            }
        }

        [LoopMethod(NLoopType.Update)]
        private static void onUpdate()
        {
            if (_updateWaitableHandles?.Count > 0)
            {
                update(_updateWaitableHandles);
            }
        }
        [LoopMethod(NLoopType.LateUpdate)]
        private static void onLateUpdate()
        {
            if (_lateUpdateWaitableHandles?.Count > 0)
            {
                update(_lateUpdateWaitableHandles);
            }
        }
        [LoopMethod(NLoopType.EndOfFrameUpdate)]
        private static void onEndOfFrameUpdate()
        {
            if (_endOfFrameWaitableHandles?.Count > 0)
            {
                update(_endOfFrameWaitableHandles);
            }
        }
    }
}
