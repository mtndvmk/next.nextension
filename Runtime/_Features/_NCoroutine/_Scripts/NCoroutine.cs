using System;
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
        public class Data : ICancellable
        {
            public readonly int groupId;
            internal readonly string name;
            public readonly Coroutine coroutine;
            private Action onCancelled;
            internal Scene? inScene;

            public RunState Status { get; private set; }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool isFinished() => Status >= RunState.Cancelled;

            public Data(IEnumerator func, int groupId)
            {
                this.groupId = groupId;
                this.name = func.ToString();
                IEnumerator innerRoutine()
                {
                    Status = RunState.Running;
                    yield return func;
                    Status = RunState.Completed;
                    runningCoroutines[groupId].Remove(this);
                }
                this.coroutine = _runner.StartCoroutine(innerRoutine());
                HashSet<Data> hashset;
                if (runningCoroutines.ContainsKey(groupId))
                {
                    hashset = runningCoroutines[groupId];
                }
                else
                {
                    hashset = new HashSet<Data>();
                    runningCoroutines.Add(groupId, hashset);
                }
                hashset.Add(this);
            }
            public void cancel()
            {
                if (Status <= RunState.Running)
                {
                    _runner.StopCoroutine(this.coroutine);
                    runningCoroutines[groupId].Remove(this);
                    Status = RunState.Cancelled;
                    this.onCancelled?.Invoke();
                    this.onCancelled = null;
                }
            }
            public void addCancelledEvent(Action onCancelled)
            {
                if (Status == RunState.Cancelled)
                {
                    try
                    {
                        onCancelled?.Invoke();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
                else if (Status <= RunState.Running)
                {
                    this.onCancelled += onCancelled;
                }
            }
            public void removeCancelledEvent(Action onCancelled)
            {
                this.onCancelled -= onCancelled;
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
            None = 0,
            SameGroup,
            AllGroup,
        }

        private static Runner _runner;
        private static Dictionary<int, HashSet<Data>> runningCoroutines = new Dictionary<int, HashSet<Data>>();
        private const int DEFAULT_GROUP_ID = 0;
        
        public static Data startCoroutine(IEnumerator func, StopType stopType = StopType.None)
        {
            InternalCheck.checkEditorMode();
            return startCoroutine(func, DEFAULT_GROUP_ID, stopType);
        }
        public static Data startCoroutine(IEnumerator func, int groupId, StopType stopType = StopType.None)
        {
            InternalCheck.checkEditorMode();
            stopCoroutine(func, groupId, stopType);
            var data = new Data(func, groupId);
            return data;
        }
        
        public static void stopCoroutine(IEnumerator func, StopType stopType = StopType.SameGroup)
        {
            stopCoroutine(func, DEFAULT_GROUP_ID, stopType);
        }
        public static void stopCoroutine(IEnumerator func, int groupId, StopType stopType = StopType.SameGroup)
        {
#if UNITY_EDITOR
            if (!NStartRunner.IsPlaying)
            {
                return;
            }
#endif
            if (stopType == StopType.None)
            {
                return;
            }
            var funcName = func.ToString();
            switch (stopType)
            {
                case StopType.SameGroup:
                    if (runningCoroutines.ContainsKey(groupId))
                    {
                        var arr = runningCoroutines[groupId].ToArray();
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
                    foreach (var v in runningCoroutines.Values)
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
            if (runningCoroutines.TryGetValue(groupId, out var hs))
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
            foreach (var v in runningCoroutines.Values)
            {
                var arr = v.ToArray();
                foreach (var d in arr)
                {
                    d.cancel();
                }
            }
            runningCoroutines.Clear();
        }

        public static Data runDelay(float time, Action delayCallback, int groupId = 0)
        {
            InternalCheck.checkEditorMode();
            IEnumerator delayRoutine_Time()
            {
                yield return new WaitForSeconds(time);
                delayCallback?.Invoke();
            }
            return startCoroutine(delayRoutine_Time(), groupId);
        }
        public static Data runDelay(YieldInstruction yieldInstruction, Action delayCallback)
        {
            InternalCheck.checkEditorMode();
            IEnumerator delayRoutine_YI()
            {
                yield return yieldInstruction;
                delayCallback?.Invoke();
            }
            return startCoroutine(delayRoutine_YI());
        }
        public static Data runDelay(CustomYieldInstruction yieldInstruction, Action delayCallback)
        {
            InternalCheck.checkEditorMode();
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
            runningCoroutines.Clear();
            SceneManager.sceneUnloaded -= onSceneUnloaded;
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
            foreach (var v in runningCoroutines.Values)
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
                Debug.Log("[NCoroutine] Clear data on load scene");
            }
        }
    }
}
