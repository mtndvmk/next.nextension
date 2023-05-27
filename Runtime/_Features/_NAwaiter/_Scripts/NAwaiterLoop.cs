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

        static NAwaiterLoop()
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
                    return;
                }
                for (int i = waitables.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        var isFinished = waitables[i].checkComplete().isFinished();
                        if (isFinished)
                        {
                            waitables.RemoveAt(i);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        waitables.RemoveAt(i);
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
#if UNITY_EDITOR
    internal class NAwaiter_EdtitorLoop
    {
        private static List<NWaitableHandle> waitables = new List<NWaitableHandle>();
        public static ulong UpdateCount { get; private set; }
        public static double CurrentTime => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000d;
        public static void addAwaitable(NWaitableHandle waitable)
        {
            waitables.Add(waitable);
        }
        static NAwaiter_EdtitorLoop()
        {
            UnityEditor.EditorApplication.update -= update;
            UnityEditor.EditorApplication.update += update;
        }
        private static void update()
                {
            UpdateCount++;
                for (int i = waitables.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        var isFinished = waitables[i].checkComplete().isFinished();
                        if (isFinished)
                        {
                            waitables.RemoveAt(i);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        waitables.RemoveAt(i);
                    }
                }
            }
        }
#endif
}
