using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Nextension
{
    public static class NUpdater
    {
        public static NCallback onEndOfFrameEvent
        {
            get
            {
                lock (_onEndOfFrameEvent)
                {
                    return _onEndOfFrameEvent;
                }
            }
        }
        public static NCallback onEndOfFrameOnceTimeEvent
        {
            get
            {
                lock (_onEndOfFrameOnceTimeEvent)
                {
                    return _onEndOfFrameOnceTimeEvent;
                }
            }
        }

        public static NCallback onUpdateEvent
        {
            get
            {
                lock (_onUpdateEvent)
                {
                    return _onUpdateEvent;
                }
            }
        }
        public static NCallback onUpdateOnceTimeEvent
        {
            get
            {
                lock (_onUpdateOnceTimeEvent)
                {
                    return _onUpdateOnceTimeEvent;
                }
            }
        }

        public static NCallback onLateUpdateEvent
        {
            get
            {
                lock (_onLateUpdateEvent)
                {
                    return _onLateUpdateEvent;
                }
            }
        }
        public static NCallback onLateUpdateOnceTimeEvent
        {
            get
            {
                lock (_onLateUpdateOnceTimeEvent)
                {
                    return _onLateUpdateOnceTimeEvent;
                }
            }
        }

        private static readonly NCallback _onEndOfFrameEvent = new NCallback();
        private static readonly NCallback _onEndOfFrameOnceTimeEvent = new NCallback();

        private static readonly NCallback _onUpdateEvent = new NCallback();
        private static readonly NCallback _onUpdateOnceTimeEvent = new NCallback();

        private static readonly NCallback _onLateUpdateEvent = new NCallback();
        private static readonly NCallback _onLateUpdateOnceTimeEvent = new NCallback();

        private static Action[] _staticUpdateEvent;
        private static Action[] _staticLateUpdateEvent;
        private static Action[] _staticEndOfFrameEvent;

        private static Stopwatch _stopwatch;
        /// <summary>
        /// time in milliseconds from initialization to latest frame
        /// </summary>
        public static long LatestUpdatedTimeMs { get; private set; }
        /// <summary>
        /// time in seconds from initialization to latest frame
        /// </summary>
        public static float LatestUpdatedTime => LatestUpdatedTimeMs * 0.001f;
        /// <summary>
        /// time in milliseconds from initialization to now 
        /// </summary>
        public static long CurrentTimeMs => _stopwatch.ElapsedMilliseconds;
        /// <summary>
        /// time in seconds from initialization to now 
        /// </summary>
        public static float CurrentTime => _stopwatch.ElapsedMilliseconds * 0.001f;

        /// <summary>
        /// scaled time in milliseconds between the last two frames
        /// </summary>
        public static int DeltaTimeMs { get; private set; }

        /// <summary>
        /// scaled time in seconds between the last two frames
        /// </summary>
        public static float DeltaTime => DeltaTimeMs * 0.001f;

        /// <summary>
        /// unscaled time in milliseconds between the last two frames
        /// </summary>
        public static int UnscaledDeltaTimeMs { get; private set; }
        /// <summary>
        /// unscaled time in seconds between the last two frames
        /// </summary>
        public static float UnscaledDeltaTime => UnscaledDeltaTimeMs * 0.001f;

        public static float UnityTime { get; private set; }

        public static float TimeSinceLatestUpdatedMs => CurrentTimeMs - LatestUpdatedTimeMs;
        public static float TimeSinceLatestUpdated => TimeSinceLatestUpdatedMs * 0.001f;

        public static uint UpdateCount => _isUpdatedInNewFrame ? _updateCount : (_updateCount + 1);

        private static bool _isUpdatedInNewFrame;
        private static uint _updateCount;

        public static void removeAllListeners()
        {
            onEndOfFrameEvent.clear();
            onEndOfFrameOnceTimeEvent.clear();

            onUpdateEvent.clear();
            onUpdateOnceTimeEvent.clear();

            onLateUpdateEvent.clear();
            onLateUpdateOnceTimeEvent.clear();
        }

        [DefaultExecutionOrder(-100)]
        private class InternalObject : MonoBehaviour
        {
            private IEnumerator Start()
            {
                var waitForEOF = new WaitForEndOfFrame();
                while (NStartRunner.IsPlaying)
                {
                    yield return waitForEOF;
                    invokeEvent(onEndOfFrameEvent);
                    invokeAndClear(onEndOfFrameOnceTimeEvent);
                    invokeStaticEvent(_staticEndOfFrameEvent);
                    _isUpdatedInNewFrame = false;
                }
            }
            private void Update()
            {
                _updateCount++;
                _isUpdatedInNewFrame = true;
                UnityTime = Time.time;

                var currentTimeMs = _stopwatch.ElapsedMilliseconds;
                UnscaledDeltaTimeMs = (int)(currentTimeMs - LatestUpdatedTimeMs);
                DeltaTimeMs = (int)(UnscaledDeltaTimeMs * Time.timeScale);

                LatestUpdatedTimeMs = currentTimeMs;

                invokeEvent(onUpdateEvent);
                invokeAndClear(onUpdateOnceTimeEvent);
                invokeStaticEvent(_staticUpdateEvent);
            }
            private void LateUpdate()
            {
                invokeEvent(onLateUpdateEvent);
                invokeAndClear(onLateUpdateOnceTimeEvent);
                invokeStaticEvent(_staticLateUpdateEvent);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void invokeEvent(NCallback callback)
            {
                lock (callback)
                {
                    callback.tryInvoke();
                }
            }
            private void invokeStaticEvent(Action[] actions)
            {
                foreach (var action in actions)
                {
                    try
                    {
                        action.Invoke();
                    }
                    catch (Exception ex)
                    {
                        NDebug.LogException(ex);
                    }
                }
            }
            private void invokeAndClear(NCallback callback)
            {
                lock (callback)
                {
                    if (callback.Count > 0)
                    {
                        invokeEvent(callback);
                        callback.clear();
                    }
                }
            }
        }

        [StartupMethod]
        private static void initialize()
        {
            EditorCheck.checkEditorMode();
            _stopwatch = Stopwatch.StartNew();
            findStaticEvents();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void createInternalObject()
        {
            var go = new GameObject("[NUpdater]");
            go.AddComponent<InternalObject>();
            Object.DontDestroyOnLoad(go);
#if UNITY_EDITOR
            go.hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInInspector;
#endif
        }

        private static void findStaticEvents()
        {
            var cache = StaticMethodCache.Instance;
            if (cache == null)
            {
                NDebug.LogWarning("[NUpdater] StaticMethodCache is unavailable, skipping loop method registration.");
                return;
            }

            var bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            var typeOfAction = typeof(Action);

            var list = new List<Action>(4);
            _staticUpdateEvent = resolveActions(list, cache.OnUpdates, bindingFlags, typeOfAction) ?? Array.Empty<Action>();
            _staticLateUpdateEvent = resolveActions(list, cache.OnLateUpdates, bindingFlags, typeOfAction) ?? Array.Empty<Action>();
            _staticEndOfFrameEvent = resolveActions(list, cache.OnEndOfFrames, bindingFlags, typeOfAction) ?? Array.Empty<Action>();
        }

        private static Action[] resolveActions(List<Action> list, StaticMethodData[] cachedMethods, BindingFlags bindingFlags, Type typeOfAction)
        {
            if (cachedMethods == null || cachedMethods.Length == 0) return null;

            foreach (var data in cachedMethods)
            {
                var type = Type.GetType(data.typeName);
                if (type == null)
                {
                    NDebug.LogWarning($"[NUpdater] Cannot resolve type: {data.typeName}");
                    continue;
                }
                var method = type.GetMethod(data.methodName, bindingFlags);
                if (method == null)
                {
                    NDebug.LogWarning($"[NUpdater] Cannot resolve method: {data.typeName}.{data.methodName}");
                    continue;
                }
                try
                {
                    var action = (Action)method.CreateDelegate(typeOfAction);
                    list.Add(action);
                }
                catch (Exception ex)
                {
                    NDebug.LogException(ex);
                }
            }
            return list.Count > 0 ? list.ToArray() : null;
        }

#if UNITY_EDITOR
        [EditorQuittingMethod]
        private static void reset()
        {
            LatestUpdatedTimeMs = 0;
            UnscaledDeltaTimeMs = 0;
            DeltaTimeMs = 0;
            _isUpdatedInNewFrame = false;
            _updateCount = 0;
            _stopwatch?.Stop();
            _stopwatch = null;

            onEndOfFrameEvent.clear();
            onEndOfFrameOnceTimeEvent.clear();
            onUpdateEvent.clear();
            onUpdateOnceTimeEvent.clear();
            onLateUpdateEvent.clear();
            onLateUpdateOnceTimeEvent.clear();
        }
#endif
    }
}

