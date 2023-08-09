using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nextension
{
    public enum WaiterLoopType
    {
        Update,
        LateUpdate,
        EndOfFrameUpdate,
    } 
    internal class NAwaiterLoop
    {
        private static Dictionary<WaiterLoopType, List<NWaitableHandle>> allWaitables = new Dictionary<WaiterLoopType, List<NWaitableHandle>>();
        public static void addAwaitable(NWaitableHandle waitable)
        {
            if (allWaitables.ContainsKey(waitable.loopType))
            {
                allWaitables[waitable.loopType].Add(waitable);
            }
            else
            {
                allWaitables.Add(waitable.loopType, new List<NWaitableHandle>() { waitable });

            }
        }

        [StartupMethod]
        private static void init()
        {
            NUpdater.onUpdateEvent.add(onUpdate);
            NUpdater.onLateUpdateEvent.add(onLateUpdate);
            NUpdater.onEndOfFrameEvent.add(onEndOfFrameUpdate);
        }
        private static void update(WaiterLoopType loopType)
        {
            if (allWaitables.TryGetValue(loopType, out var waitables))
            {
                if (waitables == null)
                {
                    Debug.LogWarning("waitables list is null?");
                    allWaitables.Remove(loopType);
                    return;
                }
                for (int i = waitables.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        var isFinished = waitables[i].checkComplete().isFinished();
                        if (isFinished)
                        {
                            NWaitableHandle.Factory.release(waitables[i]);
                            waitables.removeAtSwapBack(i);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        NWaitableHandle.Factory.release(waitables[i]);
                        waitables.removeAtSwapBack(i);
                    }
                }
            }
        }

        private static void onUpdate()
        {
            update(WaiterLoopType.Update);
        }
        private static void onLateUpdate()
        {
            update(WaiterLoopType.LateUpdate);
        }
        private static void onEndOfFrameUpdate()
        {
            update(WaiterLoopType.EndOfFrameUpdate);
        }
    }
}
