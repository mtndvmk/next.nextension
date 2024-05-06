#if !UNITY_EDITOR
#define NNEXT_DISABLE_NPOOL_TRACKING
#endif
#if !NNEXT_DISABLE_NPOOL_TRACKING
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nextension
{
    internal class PoolTracker
    {
        private static HashSet<PoolTracker> _usingPool = new();
        private static int WARNING_TIME_MS = 60000;

        [EditorQuittingMethod]
        private static void reset()
        {
            _usingPool.Clear();
        }

        [LoopMethod(NLoopType.LateUpdate)]
        private static void update()
        {
            var currentTimeMs = NUpdater.LatestUpdatedTimeMs;
            int count = 0;
            foreach (var poolTrackable in _usingPool)
            {
                var noAccessTimeMs = currentTimeMs - poolTrackable._lastAccessTimeMs;
                if (noAccessTimeMs > WARNING_TIME_MS)
                {
                    count++;
#if NPOOL_TRACKING_PRINT_STACK_TRACE
                    Debug.LogWarning($"NPoolCollection [{poolTrackable._id}] has not accessed for a long time ({noAccessTimeMs} ms)\n<color=yellow>Get stacktrace</color>:{poolTrackable._getStackTrace}");
#else
                    Debug.LogWarning($"NPoolCollection [{poolTrackable._id}] has not been accessed for a long time ({noAccessTimeMs} ms)");
#endif
                }
            }

            if (count > 0)
            {
                Debug.LogWarning($"Exists {count} NPoolCollection that have not been accessed for a long time");
            }
        }

        private long _lastAccessTimeMs;
        private string _id;

#if NPOOL_TRACKING_PRINT_STACK_TRACE
        private System.Diagnostics.StackTrace _getStackTrace;
#endif

        public PoolTracker(string id)
        {
            _id = id;
        }

        public void start()
        {
            _usingPool.Add(this);
#if NPOOL_TRACKING_PRINT_STACK_TRACE
            _getStackTrace = new System.Diagnostics.StackTrace();
#endif
            updateAccessInfo();
        }
        public void stop()
        {
            _usingPool.Remove(this);
#if NPOOL_TRACKING_PRINT_STACK_TRACE
            _getStackTrace = null;
#endif
        }
        public void updateAccessInfo()
        {
            _lastAccessTimeMs = NUpdater.LatestUpdatedTimeMs;
        }
    }
}
#endif