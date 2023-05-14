using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Nextension
{
    public class NCoroutine : NSingleton<NCoroutine>
    {
        public class Data : ICancellable
        {
            public readonly int groupId;
            internal readonly string name;
            public readonly Coroutine coroutine;
            private Action onCancelled;
            public RunState Status { get; private set; }
            public bool IsClearOnLoadScene { get; set; }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool isFinished() => Status >= RunState.Cancelled;

            public Data(IEnumerator func, int groupId, bool isClearOnLoadScene)
            {
                this.groupId = groupId;
                this.name = func.ToString();
                this.IsClearOnLoadScene = isClearOnLoadScene;
                IEnumerator innerRoutine()
                {
                    Status = RunState.Running;
                    yield return func;
                    Status = RunState.Completed;
                    runningCoroutines[groupId].Remove(this);
                }
                this.coroutine = Instance.StartCoroutine(innerRoutine());
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
                    Instance.StopCoroutine(this.coroutine);
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
        }
        public enum StopType
        {
            None = 0,
            SAME_GROUP,
            ALL_GROUP,
        }

        protected override void onDestroy()
        {
            Instance.StopAllCoroutines();
        }

        protected override void onInitialized()
        {
            base.onInitialized();
            m_DontDestroyOnLoad = true;
            gameObject.hideFlags = HideFlags.HideAndDontSave;
        }

        private static Dictionary<int, HashSet<Data>> runningCoroutines = new Dictionary<int, HashSet<Data>>();
        private const int DEFAULT_GROUP_ID = 0;
        
        public static Data startCoroutine(IEnumerator func, StopType stopType = StopType.None, bool isClearOnLoadScene = true)
        {
            return startCoroutine(func, DEFAULT_GROUP_ID, stopType, isClearOnLoadScene);
        }
        public static Data startCoroutine(IEnumerator func, int groupId, StopType stopType = StopType.None, bool isClearOnLoadScene = true)
        {
            stopCoroutine(func, groupId, stopType);
            var data = new Data(func, groupId, isClearOnLoadScene);
            return data;
        }
        
        public static void stopCoroutine(IEnumerator func, StopType stopType = StopType.SAME_GROUP)
        {
            stopCoroutine(func, DEFAULT_GROUP_ID, stopType);
        }
        public static void stopCoroutine(IEnumerator func, int groupId, StopType stopType = StopType.SAME_GROUP)
        {
            if (IsQuitApp || stopType == StopType.None)
            {
                return;
            }
            var funcName = func.ToString();
            switch (stopType)
            {
                case StopType.SAME_GROUP:
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
                case StopType.ALL_GROUP:
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
            if (IsQuitApp)
            {
                return;
            }
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

        public static Data runDelay(float time, Action delayCallback)
        {
            IEnumerator delayRoutine_Time()
            {
                yield return new WaitForSeconds(time);
                delayCallback?.Invoke();
            }
            return startCoroutine(delayRoutine_Time());
        }
        public static Data runDelay(YieldInstruction yieldInstruction, Action delayCallback)
        {
            IEnumerator delayRoutine_YI()
            {
                yield return yieldInstruction;
                delayCallback?.Invoke();
            }
            return startCoroutine(delayRoutine_YI());
        }
        public static Data runDelay(CustomYieldInstruction yieldInstruction, Action delayCallback)
        {
            IEnumerator delayRoutine_CYI()
            {
                yield return yieldInstruction;
                delayCallback?.Invoke();
            }
            return startCoroutine(delayRoutine_CYI());
        }
        public static void stopCoroutine(Data data)
        {
            data.cancel();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void clearDataOnLoadScene()
        {
            int clearCount = 0;
            foreach (var v in runningCoroutines.Values)
            {
                var arr = v.ToArray();
                foreach (var d in arr)
                {
                    if (d.IsClearOnLoadScene)
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
