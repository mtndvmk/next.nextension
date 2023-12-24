using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nextension
{
    internal class NAwaiterLoop
    {
#if UNITY_EDITOR
        [EditorQuittingMethod]
        private void reset()
        {
            _updateWaitableHandles?.Clear();
            _lateUpdateWaitableHandles?.Clear();
            _endOfFrameWaitableHandles?.Clear();

            _updateHandleCount = 0;
            _lateUpdateHandleCount = 0;
            _eofFrameHandleCount = 0;
        }
#endif
        private static List<NWaitableHandle> _updateWaitableHandles;
        private static List<NWaitableHandle> _lateUpdateWaitableHandles;
        private static List<NWaitableHandle> _endOfFrameWaitableHandles;

        private static int _updateHandleCount;
        private static int _lateUpdateHandleCount;
        private static int _eofFrameHandleCount;

        public static void addAwaitable(NWaitableHandle waitable, bool isIgnoreFirstFrameCheck)
        {
            switch (waitable.loopType)
            {
                case NLoopType.Update:
                    {
                        (_updateWaitableHandles ??= new(1)).Add(waitable);
                        if (!isIgnoreFirstFrameCheck)
                        {
                            _updateHandleCount++;
                        }
                        break;
                    }
                case NLoopType.LateUpdate:
                    {
                        (_lateUpdateWaitableHandles ??= new(1)).Add(waitable); 
                        if (!isIgnoreFirstFrameCheck)
                        {
                            _lateUpdateHandleCount++;
                        }
                        break;
                    }
                case NLoopType.EndOfFrameUpdate:
                    {
                        (_endOfFrameWaitableHandles ??= new(1)).Add(waitable);
                        if (!isIgnoreFirstFrameCheck)
                        {
                            _eofFrameHandleCount++;
                        }
                        break;
                    }
            }
        }

        private static void update(List<NWaitableHandle> handleList, int endIndex)
        {
            var handleSpan = handleList.asSpan();
            for (int i = endIndex; i >= 0; i--)
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
                if (_updateHandleCount > 0)
                {
                    update(_updateWaitableHandles, _updateHandleCount - 1);
                }
                _updateHandleCount = _updateWaitableHandles.Count;
            }
        }
        [LoopMethod(NLoopType.LateUpdate)]
        private static void onLateUpdate()
        {
            if (_lateUpdateWaitableHandles?.Count > 0)
            {
                if (_lateUpdateHandleCount > 0)
                {
                    update(_lateUpdateWaitableHandles, _lateUpdateHandleCount - 1);
                }
                _lateUpdateHandleCount = _lateUpdateWaitableHandles.Count;
            }
        }
        [LoopMethod(NLoopType.EndOfFrameUpdate)]
        private static void onEndOfFrameUpdate()
        {
            if (_endOfFrameWaitableHandles?.Count > 0)
            {
                if (_eofFrameHandleCount > 0)
                {
                    update(_endOfFrameWaitableHandles, _eofFrameHandleCount - 1);
                }
                _eofFrameHandleCount = _endOfFrameWaitableHandles.Count;
            }
        }
    }
}
