using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nextension
{
    internal static class NTaskManager
    {
        public const uint CompleteId = 1;
        private static readonly InterlockedId _idCounter = new InterlockedId(CompleteId);

        // State management for NTask
        private readonly static Dictionary<uint, NWaitableState> _results = new Dictionary<uint, NWaitableState>();
        private readonly static HashSet<uint> _inCompleteTaskIds = new HashSet<uint>();

        // Stores the next cancelable (the awaiter being awaited inside the NTask's async method) keyed by NTask id
        private static readonly Dictionary<uint, ICancelable> _nextCancelables = new Dictionary<uint, ICancelable>();

        // Stores the NTaskAwaiter that is awaiting on an NTask (i.e. someone outside awaiting the NTask) keyed by NTask id
        private static readonly Dictionary<uint, AbsAwaiter> _awaiters = new Dictionary<uint, AbsAwaiter>();

        // Stores NTaskResultHolder keyed by NTask id
        private static readonly Dictionary<uint, NTaskResultHolder> _holders = new Dictionary<uint, NTaskResultHolder>();

        private readonly static HashSet<uint> _forgotTaskIds = new HashSet<uint>();

        public static uint nextId() => _idCounter.nextId();

        public static void addInCompleteTask(uint id)
        {
            if (id == CompleteId) return;
            if (id == 0) throw new Exception("NTask id must not be 0");
            lock (_inCompleteTaskIds)
            {
                if (!_inCompleteTaskIds.Add(id))
                {
                    throw new Exception($"NTask[{id}] has already been added");
                }
            }
        }

        public static bool containsInCompleteTask(uint id)
        {
            lock (_inCompleteTaskIds)
            {
                return _inCompleteTaskIds.Contains(id);
            }
        }

        public static void removeInCompleteTaskId(uint id)
        {
            lock (_inCompleteTaskIds)
            {
                _inCompleteTaskIds.Remove(id);
            }
        }

        // ── NTask State Management ───────────────────────────────────────

        private static void __addTaskResult(uint id, NWaitableState result)
        {
            if (id == CompleteId) return;
            if (id == 0) throw new Exception("NTask id must not be 0");
            removeInCompleteTaskId(id);
            lock (_results)
            {
                _results.Add(id, result);
            }
        }

        public static void removeTaskResult(uint id)
        {
            if (id == CompleteId) return;
            if (id == 0) throw new Exception("NTask id must not be 0");
            removeInCompleteTaskId(id);
            lock (_results)
            {
                _results.Remove(id);
            }
        }

        public static bool tryGetCurrentState(uint id, out NWaitableState result)
        {
            if (id == CompleteId)
            {
                result = NWaitableState.Completed;
                return true;
            }
            if (containsInCompleteTask(id))
            {
                result = NWaitableState.None;
                return true;
            }
            lock (_results)
            {
                return _results.TryGetValue(id, out result);
            }
        }

        // ── nextCancelable ───────────────────────────────────────────────

        public static void setNextCancelable(uint taskId, ICancelable cancelable)
        {
            lock (_nextCancelables)
            {
                _nextCancelables[taskId] = cancelable;
            }
        }

        public static void removeNextCancelable(uint taskId)
        {
            lock (_nextCancelables)
            {
                _nextCancelables.Remove(taskId);
            }
        }

        // ── Awaiter management ───────────────────────────────────────────

        public static bool tryAddAwaiter(uint taskId, AbsAwaiter awaiter)
        {
            lock (_awaiters)
            {
                return _awaiters.TryAdd(taskId, awaiter);
            }
        }

        public static void removeAwaiter(uint taskId)
        {
            lock (_awaiters)
            {
                _awaiters.Remove(taskId);
            }
        }

        public static bool containsAwaiter(uint taskId)
        {
            lock (_awaiters)
            {
                return _awaiters.ContainsKey(taskId);
            }
        }

        public static bool tryGetAndRemoveAwaiter(uint taskId, out AbsAwaiter awaiter)
        {
            lock (_awaiters)
            {
                if (_awaiters.TryGetValue(taskId, out awaiter))
                {
                    _awaiters.Remove(taskId);
                    return true;
                }
                return false;
            }
        }

        // ── Result Holder management ─────────────────────────────────────

        public static NTaskResultHolder createHolder(uint taskId)
        {
            if (taskId == CompleteId)
            {
                return new NTaskResultHolder(taskId, NWaitableState.Completed);
            }

            lock (_results)
            {
                if (_results.TryGetValue(taskId, out var result))
                {
                    return new NTaskResultHolder(taskId, result);
                }
            }

            if (!containsInCompleteTask(taskId))
            {
                throw new Exception($"NTask[{taskId}] does not exist");
            }

            lock (_holders)
            {
                if (_holders.TryGetValue(taskId, out var holder))
                {
                    return holder;
                }
                holder = new NTaskResultHolder(taskId);
                _holders[taskId] = holder;
                return holder;
            }
        }

        public static void createHolderNonAlloc(NTaskResultHolder holder, uint taskId)
        {
            lock (_holders)
            {
                if (_holders.ContainsKey(holder.TaskId))
                {
                    throw new Exception($"NTaskResultHolder for NTask[{holder.TaskId}] already exists");
                }
            }
            if (taskId == CompleteId)
            {
                holder.reuse(taskId, NWaitableState.Completed);
                return;
            }

            lock (_results)
            {
                if (_results.TryGetValue(taskId, out var result))
                {
                    holder.reuse(taskId, result);
                    return;
                }
            }

            if (!containsInCompleteTask(taskId))
            {
                throw new Exception($"NTask[{taskId}] does not exist");
            }

            lock (_holders)
            {
                if (_holders.TryGetValue(taskId, out var otherHolder))
                {
                    holder.reuse(taskId, otherHolder.CurrentState);
                    return;
                }
                holder.reuse(taskId, NWaitableState.None);
                _holders[taskId] = holder;
            }
        }

        // ── Task Completion ──────────────────────────────────────────────

        public static void cancel(uint taskId)
        {
            if (!containsInCompleteTask(taskId))
            {
                Debug.LogWarning($"NTask[{taskId}] is not in progress");
                return;
            }
            cancelNextCancelable(taskId);
            if (!containsAwaiter(taskId))
            {
                // Task is in-progress — no result has been stored yet, so just remove from the in-progress set
                removeInCompleteTaskId(taskId);
            }
        }

        public static void forget(uint taskId)
        {
            if (containsAwaiter(taskId))
            {
                Debug.LogWarning($"NTask[{taskId}] is already being awaited, cannot forget");
                return;
            }

            if (containsInCompleteTask(taskId))
            {
                // Task is still running — defer cleanup until it finishes
                lock (_forgotTaskIds)
                {
                    _forgotTaskIds.Add(taskId);
                }
                return;
            }

            bool hasResult;
            lock (_results)
            {
                hasResult = _results.ContainsKey(taskId);
            }

            if (hasResult)
            {
                // Task is completed and nobody is awaiting it — clean up now
                removeTaskResult(taskId);
                return;
            }
        }

        public static void cancelNextCancelable(uint taskId)
        {
            ICancelable nextCancelable = null;
            lock (_nextCancelables)
            {
                if (_nextCancelables.TryGetValue(taskId, out nextCancelable))
                {
                    _nextCancelables.Remove(taskId);
                }
            }
            nextCancelable?.cancel();
        }

        public static void setTaskResult(uint taskId, NWaitableState result)
        {
            bool hasAwaiter = containsAwaiter(taskId);
            if (!hasAwaiter)
            {
                lock (_forgotTaskIds)
                {
                    if (_forgotTaskIds.Contains(taskId))
                    {
                        _forgotTaskIds.Remove(taskId);
                        removeTaskResult(taskId);
                        return;
                    }
                }
            }
            __addTaskResult(taskId, result);
            __onTaskFinished(taskId, result);
        }

        private static void __onTaskFinished(uint taskId, NWaitableState state)
        {
            lock (_nextCancelables)
            {
                _nextCancelables.Remove(taskId);
            }

            if (tryGetAndRemoveAwaiter(taskId, out var awaiter))
            {
                ((NTaskAwaiter)awaiter).setCompletion(awaiter.Id, state);
            }

            // Update Holders
            NTaskResultHolder holder = null;
            lock (_holders)
            {
                if (_holders.TryGetValue(taskId, out holder))
                {
                    _holders.Remove(taskId);
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
