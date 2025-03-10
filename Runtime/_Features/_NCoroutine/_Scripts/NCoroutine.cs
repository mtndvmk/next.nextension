﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Nextension
{
    public sealed class NCoroutine
    {
        private class Runner : MonoBehaviour { }
        public class Data : ICancelable
        {
            public readonly int groupId;
            internal readonly string name;
            public readonly Coroutine coroutine;
            private Action onCanceled;
            internal Scene? inScene;

            public RunState Status { get; private set; }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool isFinished() => Status.isFinished();

            public Data(IEnumerator func, int groupId)
            {
                this.groupId = groupId;
                this.name = func.ToString();
                if (!_runningCoroutines.TryGetValue(groupId, out var hashSet))
                {
                    hashSet = new(1) { this };
                    _runningCoroutines.Add(groupId, hashSet);
                }
                else
                {
                    hashSet.Add(this);
                }

                IEnumerator innerRoutine()
                {
                    Status = RunState.Running;
                    yield return func;
                    Status = RunState.Completed;
                    hashSet.Remove(this);
                }
                this.coroutine = _runner.StartCoroutine(innerRoutine());
            }
            public void cancel()
            {
                if (Status <= RunState.Running)
                {
                    _runner.StopCoroutine(this.coroutine);
                    _runningCoroutines[groupId].Remove(this);
                    Status = RunState.Canceled;
                    this.onCanceled?.Invoke();
                    this.onCanceled = null;
                }
            }
            public void addCanceledEvent(Action onCanceled)
            {
                if (Status == RunState.Canceled)
                {
                    try
                    {
                        onCanceled?.Invoke();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
                else if (Status <= RunState.Running)
                {
                    this.onCanceled += onCanceled;
                }
            }
            public void removeCanceledEvent(Action onCanceled)
            {
                this.onCanceled -= onCanceled;
            }

            /// <summary>
            /// if InScene is unloaded, this coroutine will be stopped
            /// </summary>
            /// <param name="scene"></param>
            public void setInScene(Scene? scene = null)
            {
                inScene = scene;
            }
        }
        public enum StopType
        {
            SameGroup,
            AllGroup,
        }

        private static Runner _runner;
        private static Dictionary<int, HashSet<Data>> _runningCoroutines = new Dictionary<int, HashSet<Data>>();
        private const int DEFAULT_GROUP_ID = 0;

        public static Data startCoroutine(IEnumerator func, StopType? stopType = null)
        {
            EditorCheck.checkEditorMode();
            return startCoroutine(func, DEFAULT_GROUP_ID, stopType);
        }
        public static Data startCoroutine(IEnumerator func, int groupId, StopType? stopType = null)
        {
            EditorCheck.checkEditorMode();
            if (stopType.HasValue)
            {
                stopCoroutine(func, groupId, stopType.Value);
            }
            var data = new Data(func, groupId);
            return data;
        }

        public static void stopCoroutine(IEnumerator func, StopType stopType = StopType.SameGroup)
        {
            stopCoroutine(func, DEFAULT_GROUP_ID, stopType);
        }
        public static void stopCoroutine(IEnumerator func, int groupId, StopType stopType = StopType.SameGroup)
        {
            EditorCheck.checkEditorMode();
            var funcName = func.ToString();
            switch (stopType)
            {
                case StopType.SameGroup:
                    if (_runningCoroutines.TryGetValue(groupId, out var hashSet))
                    {
                        var arr = hashSet.ToArray();
                        foreach (var d in arr)
                        {
                            if (d.name == funcName)
                            {
                                d.cancel();
                            }
                        }
                    }
                    break;
                case StopType.AllGroup:
                    foreach (var v in _runningCoroutines.Values)
                    {
                        var arr = v.ToArray();
                        foreach (var d in arr)
                        {
                            if (d.name == funcName)
                            {
                                d.cancel();
                            }
                        }
                    }
                    break;
            }
        }

        public static void stopCoroutinesInGroup(int groupId)
        {
#if UNITY_EDITOR
            if (!NStartRunner.IsPlaying)
            {
                return;
            }
#endif
            if (_runningCoroutines.TryGetValue(groupId, out var hs))
            {
                var arr = hs.ToArray();
                foreach (var d in arr)
                {
                    d.cancel();
                }
            }
        }
        public static void stopAllCoroutines()
        {
            foreach (var v in _runningCoroutines.Values)
            {
                var arr = v.ToArray();
                foreach (var d in arr)
                {
                    d.cancel();
                }
            }
            _runningCoroutines.Clear();
        }

        public static Data runDelay(float time, Action delayCallback, int groupId = 0)
        {
            EditorCheck.checkEditorMode();
            IEnumerator delayRoutine_Time()
            {
                yield return new WaitForSeconds(time);
                delayCallback?.Invoke();
            }
            return startCoroutine(delayRoutine_Time(), groupId);
        }
        public static Data runDelay(YieldInstruction yieldInstruction, Action delayCallback)
        {
            EditorCheck.checkEditorMode();
            IEnumerator delayRoutine_YI()
            {
                yield return yieldInstruction;
                delayCallback?.Invoke();
            }
            return startCoroutine(delayRoutine_YI());
        }
        public static Data runDelay(CustomYieldInstruction yieldInstruction, Action delayCallback)
        {
            EditorCheck.checkEditorMode();
            IEnumerator delayRoutine_CYI()
            {
                yield return yieldInstruction;
                delayCallback?.Invoke();
            }
            return startCoroutine(delayRoutine_CYI());
        }
        public static void stopCoroutine(Data data)
        {
            data?.cancel();
        }

        [StartupMethod]
        static void reset()
        {
            NUtils.destroyObject(_runner);
            _runningCoroutines.Clear();
#if UNITY_EDITOR
            SceneManager.sceneUnloaded -= onSceneUnloaded;
#endif
            SceneManager.sceneUnloaded += onSceneUnloaded;
            _runner = new GameObject("NCoroutineRunner").AddComponent<Runner>();
            _runner.gameObject.hideFlags = HideFlags.DontSave | HideFlags.NotEditable | HideFlags.HideInInspector;
            UnityEngine.Object.DontDestroyOnLoad(_runner.gameObject);
        }
        private static void onSceneUnloaded(Scene scene)
        {
#if UNITY_EDITOR
            if (!NStartRunner.IsPlaying)
            {
                return;
            }
#endif
            int clearCount = 0;
            foreach (var v in _runningCoroutines.Values)
            {
                var arr = v.ToArray();
                foreach (var d in arr)
                {
                    if (d.inScene.HasValue && d.inScene.Value == scene)
                    {
                        d.cancel();
                        clearCount++;
                    }
                }
            }
            if (clearCount > 0)
            {
                Debug.Log($"[NCoroutine] Clear data on load scene: {clearCount} routines");
            }
        }
    }
}
