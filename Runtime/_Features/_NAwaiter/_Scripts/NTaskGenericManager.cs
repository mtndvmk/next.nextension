using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nextension
{
    internal static class NTaskGenericManager
    {
        private static class State<T>
        {
            public readonly static Dictionary<uint, NWaitableState<T>> Results = new Dictionary<uint, NWaitableState<T>>();
            public readonly static Dictionary<uint, NTaskResultHolder<T>> Holder = new Dictionary<uint, NTaskResultHolder<T>>();
        }

        private readonly static HashSet<uint> _forgotTaskIds = new HashSet<uint>();

        private static void __addTaskResult<T>(uint id, NWaitableState<T> result)
        {
            if (id == 0) throw new Exception($"NTask<{typeof(T)}> id must not be 0");
            NTaskManager.removeInCompleteTaskId(id);
            lock (State<T>.Results)
            {
                State<T>.Results.Add(id, result);
            }
        }

        public static NWaitableState<T> setFromCompletedResult<T>(uint id, T result)
        {
            var state = NWaitableState<T>.Completed(result);
            __addTaskResult(id, state);
            return state;
        }

        public static void removeTaskResult<T>(uint id)
        {
            if (id == 0) throw new Exception($"NTask<{typeof(T)}> id must not be 0");
            NTaskManager.removeInCompleteTaskId(id);
            lock (State<T>.Results)
            {
                State<T>.Results.Remove(id);
            }
        }

        public static bool tryGetCurrentState<T>(uint id, out NWaitableState<T> result)
        {
            if (NTaskManager.containsInCompleteTask(id))
            {
                result = NWaitableState<T>.None;
                return true;
            }
            var results = State<T>.Results;
            lock (results)
            {
                return results.TryGetValue(id, out result);
            }
        }

        public static NTaskResultHolder<T> createHolder<T>(uint taskId)
        {
            lock (State<T>.Results)
            {
                if (State<T>.Results.TryGetValue(taskId, out var result))
                {
                    return new NTaskResultHolder<T>(taskId, result);
                }
            }

            if (!NTaskManager.containsInCompleteTask(taskId))
            {
                throw new Exception($"NTask<{typeof(T)}>[{taskId}] does not exist");
            }

            lock (State<T>.Holder)
            {
                if (State<T>.Holder.TryGetValue(taskId, out var holder))
                {
                    return holder;
                }
                holder = new NTaskResultHolder<T>(taskId);
                State<T>.Holder[taskId] = holder;
                return holder;
            }
        }

        public static void createHolderNonAlloc<T>(NTaskResultHolder<T> holder, uint taskId)
        {
            lock (State<T>.Holder)
            {
                if (State<T>.Holder.ContainsKey(holder.TaskId))
                {
                    throw new Exception($"NTaskResultHolder<{typeof(T)}> with id {holder.TaskId} already exists");
                }
            }

            if (taskId == NTaskManager.CompleteId)
            {
                holder.reuse(taskId, NWaitableState<T>.Completed(default));
                return;
            }

            lock (State<T>.Results)
            {
                if (State<T>.Results.TryGetValue(taskId, out var result))
                {
                    holder.reuse(taskId, result);
                    return;
                }
            }

            if (!NTaskManager.containsInCompleteTask(taskId))
            {
                throw new Exception($"NTask<{typeof(T)}>[{taskId}] does not exist");
            }

            lock (State<T>.Holder)
            {
                if (State<T>.Holder.TryGetValue(taskId, out var otherHolder))
                {
                    holder.reuse(taskId, otherHolder.CurrentState);
                    return;
                }
                holder.reuse(taskId, NWaitableState<T>.None);
                State<T>.Holder[taskId] = holder;
            }
        }

        public static void forget<T>(uint taskId)
        {
            if (NTaskManager.containsAwaiter(taskId))
            {
                Debug.LogWarning($"NTask<{typeof(T)}>[{taskId}] is already being awaited, cannot forget");
                return;
            }

            if (NTaskManager.containsInCompleteTask(taskId))
            {
                // Task is still running — defer cleanup until it finishes
                lock (_forgotTaskIds)
                {
                    _forgotTaskIds.Add(taskId);
                }
                return;
            }

            bool hasResult;
            lock (State<T>.Results)
            {
                hasResult = State<T>.Results.ContainsKey(taskId);
            }

            if (hasResult)
            {
                // Task is completed and nobody is awaiting it — clean up now
                removeTaskResult<T>(taskId);
                return;
            }
        }

        public static void setTaskResult<T>(uint taskId, NWaitableState<T> result)
        {
            bool hasAwaiter = NTaskManager.containsAwaiter(taskId);
            if (!hasAwaiter)
            {
                lock (_forgotTaskIds)
                {
                    if (_forgotTaskIds.Contains(taskId))
                    {
                        _forgotTaskIds.Remove(taskId);
                        removeTaskResult<T>(taskId);
                        return;
                    }
                }
            }
            __addTaskResult(taskId, result);
            __onTaskFinished(taskId, result);
        }

        public static void cancel<T>(uint taskId)
        {
            if (!NTaskManager.containsInCompleteTask(taskId))
            {
                Debug.LogWarning($"NTask<{typeof(T)}>[{taskId}] is not in progress");
                return;
            }
            NTaskManager.cancelNextCancelable(taskId);
            if (!NTaskManager.containsAwaiter(taskId))
            {
                // Task is in-progress — no result has been stored yet, so just remove from the in-progress set
                NTaskManager.removeInCompleteTaskId(taskId);
            }
        }

        private static void __onTaskFinished<T>(uint taskId, NWaitableState<T> state)
        {
            NTaskManager.removeNextCancelable(taskId);
      
            if (NTaskManager.tryGetAndRemoveAwaiter(taskId, out var awaiter))
            {
                ((NTaskAwaiter<T>)awaiter).setCompletion(awaiter.Id, state);
            }

            NTaskResultHolder<T> holder = null;
            lock (State<T>.Holder)
            {
                if (State<T>.Holder.TryGetValue(taskId, out holder))
                {
                    State<T>.Holder.Remove(taskId);
                }
            }
            holder?.setResultState(state);

            if (_forgotTaskIds.Count > 0)
            {
                lock (_forgotTaskIds)
                {
                    _forgotTaskIds.Remove(taskId);
                }
            }
        }
    }
}
