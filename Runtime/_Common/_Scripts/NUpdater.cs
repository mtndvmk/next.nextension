using System.Collections;
using UnityEngine;

namespace Nextension
{
    public static class NUpdater
    {
        public static readonly NCallback onEndOfFrameEvent = new NCallback();
        public static readonly NCallback onEndOfFrameOnceTimeEvent = new NCallback();

        public static readonly NCallback onUpdateEvent = new NCallback();
        public static readonly NCallback onUpdateOnceTimeEvent = new NCallback();

        public static readonly NCallback onLateUpdateEvent = new NCallback();
        public static readonly NCallback onLateUpdateOnceTimeEvent = new NCallback();

        private static System.Diagnostics.Stopwatch _stopwatch = System.Diagnostics.Stopwatch.StartNew();
        /// <summary>
        /// Milliseconds
        /// </summary>
        public static long TimeSinceLastUpdate => _stopwatch.ElapsedMilliseconds;
        public static long LastFrameTime { get; private set; }
        public static uint UpdateCount
        {
            get
            {
                if (isUpdatedInNewFrame)
                {
                    return updateCount;
                }
                else
                {
                    return updateCount + 1;
                }
            }
        }
        
        private static bool isUpdatedInNewFrame;
        private static uint updateCount;

        public static void removeAllListeners()
        {
            onEndOfFrameEvent.clear();
            onEndOfFrameOnceTimeEvent.clear();

            onUpdateEvent.clear();
            onUpdateOnceTimeEvent.clear();

            onLateUpdateEvent.clear();
            onLateUpdateOnceTimeEvent.clear();
        }

        private class InternalObject : MonoBehaviour
        {
            private IEnumerator Start()
            {
                while (!NStartRunner.IsQuitApp)
                {
                    yield return new WaitForEndOfFrame();
                    invokeEvent(onEndOfFrameEvent);
                    invokeAndClear(onEndOfFrameOnceTimeEvent);
                    isUpdatedInNewFrame = false;
                }
            }
            private void Update()
            {
                updateCount++;
                isUpdatedInNewFrame = true;
                LastFrameTime = _stopwatch.ElapsedMilliseconds;
                _stopwatch.Restart();
                invokeEvent(onUpdateEvent);
                invokeAndClear(onUpdateOnceTimeEvent);
            }
            private void LateUpdate()
            {
                invokeEvent(onLateUpdateEvent);
                invokeAndClear(onLateUpdateOnceTimeEvent);
            }
            private void invokeEvent(NCallback callback)
            {
                callback.tryInvoke(Debug.LogException);
            }
            private void invokeAndClear(NCallback callback)
            {
                if (callback.ListenerCount > 0)
                {
                    invokeEvent(callback);
                    callback.clear();
                }
            }
        }

        internal static void initialize()
        {
            var go = new GameObject("[NUpdater]");
            go.AddComponent<InternalObject>();
            GameObject.DontDestroyOnLoad(go);
            go.hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInInspector;
        }
    }
}
