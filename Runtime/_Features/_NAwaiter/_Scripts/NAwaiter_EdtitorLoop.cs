#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nextension
{
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
}
#endif