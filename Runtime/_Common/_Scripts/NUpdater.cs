using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Nextension
{
    public static class NUpdater
    {
        public static readonly NCallback onEndOfFrameEvent = new();
        public static readonly NCallback onEndOfFrameOnceTimeEvent = new();

        public static readonly NCallback onUpdateEvent = new();
        public static readonly NCallback onUpdateOnceTimeEvent = new();

        public static readonly NCallback onLateUpdateEvent = new();
        public static readonly NCallback onLateUpdateOnceTimeEvent = new();

        private static Action[] _staticUpdateEvent;
        private static Action[] _staticLateUpdateEvent;
        private static Action[] _staticEndOfFrameEvent;

        private static Stopwatch _stopwatch;
        public static long LatestUpdatedTimeMs { get; private set; }
        public static long CurrentTimeMs => _stopwatch.ElapsedMilliseconds;
        public static int DeltaTimeMs { get; private set; }

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

                var currentTime = _stopwatch.ElapsedMilliseconds;

                DeltaTimeMs = (int)(currentTime - LatestUpdatedTimeMs);
                LatestUpdatedTimeMs = currentTime;

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
                callback.tryInvoke();
            }
            private void invokeStaticEvent(Action[] actions)
            {
                if (actions != null)
                {
                    foreach (var action in actions)
                    {
                        try
                        {
                            action.Invoke();
                        }
                        catch (Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                    }
                }
            }
            private void invokeAndClear(NCallback callback)
            {
                if (callback.Count > 0)
                {
                    invokeEvent(callback);
                    callback.clear();
                }
            }
        }

        [StartupMethod]
        private static void initialize()
        {
            InternalCheck.checkEditorMode();
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
            var types = NUtils.getCustomTypes();
            var bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            var typeOfAction = typeof(Action);

            List<Action> updateList = new();
            List<Action> lateUpdateList = new();
            List<Action> endOfFrameList = new();

            foreach (var type in types)
            {
                if (type.ContainsGenericParameters)
                {
                    continue;
                }
                var methods = type.GetMethods(bindingFlags);
                foreach (var method in methods)
                {
                    var attr = method.GetCustomAttribute<LoopMethodAttribute>();
                    if (attr != null)
                    {
                        if (method.GetParameters().Length > 0)
                        {
                            Debug.LogWarning($"[{type.Name}.{method.Name}]: `{nameof(LoopMethodAttribute)}` requires parameterless");
                            continue;
                        }

                        var action = (Action)method.CreateDelegate(typeOfAction);

                        switch (attr.loopType)
                        {
                            case NLoopType.Update: updateList.Add(action); break;
                            case NLoopType.LateUpdate: lateUpdateList.Add(action); break;
                            case NLoopType.EndOfFrameUpdate: endOfFrameList.Add(action); break;
                        }
                    }
                }
            }

            if (updateList.Count > 0) _staticUpdateEvent = updateList.ToArray();
            else _staticUpdateEvent = null;

            if (lateUpdateList.Count > 0) _staticLateUpdateEvent = lateUpdateList.ToArray();
            else _staticLateUpdateEvent = null;

            if (endOfFrameList.Count > 0) _staticEndOfFrameEvent = endOfFrameList.ToArray();
            else _staticEndOfFrameEvent = null;
        }

        [EditorQuittingMethod]
        private static void reset()
        {
            LatestUpdatedTimeMs = 0;
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
    }
}
