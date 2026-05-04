using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Nextension
{
    [Serializable]
    public struct StaticMethodData : IEquatable<StaticMethodData>, IComparable<StaticMethodData>
    {
        public string methodName;
        public string typeName;

        public StaticMethodData(string typeName, string methodName)
        {
            this.methodName = methodName;
            this.typeName = typeName;
        }

        public int CompareTo(StaticMethodData other)
        {
            var nameComparison = string.Compare(this.methodName, other.methodName, StringComparison.Ordinal);
            if (nameComparison != 0) return nameComparison;
            int typeComparison = string.Compare(this.typeName, other.typeName, StringComparison.Ordinal);
            return typeComparison;
        }


        public override bool Equals(object obj)
        {
            if (obj is StaticMethodData other)
            {
                return this.typeName == other.typeName && this.methodName == other.methodName;
            }
            return false;
        }

        public bool Equals(StaticMethodData other)
        {
            return this.typeName == other.typeName && this.methodName == other.methodName;
        }


        public override int GetHashCode()
        {
            return HashCode.Combine(typeName, methodName);
        }

        public override string ToString()
        {
            return $"{typeName}.{methodName}";
        }

    }

    [SingletonScriptable]
    public class StaticMethodCache : SingletonScriptableGettable<StaticMethodCache>
    {
        internal const string FILE_NAME_IN_RESOURCE = "StaticMethodCache";
        [NReadOnly, SerializeField] private StaticMethodData[] _onStartups = Array.Empty<StaticMethodData>();
        [NReadOnly, SerializeField] private StaticMethodData[] _onQuittings = Array.Empty<StaticMethodData>();
        [NReadOnly, SerializeField] private StaticMethodData[] _onUpdates = Array.Empty<StaticMethodData>();
        [NReadOnly, SerializeField] private StaticMethodData[] _onLateUpdates = Array.Empty<StaticMethodData>();
        [NReadOnly, SerializeField] private StaticMethodData[] _onEndOfFrames = Array.Empty<StaticMethodData>();

        public StaticMethodData[] OnStartups => _onStartups;
        public StaticMethodData[] OnQuittings => _onQuittings;
        public StaticMethodData[] OnUpdates => _onUpdates;
        public StaticMethodData[] OnLateUpdates => _onLateUpdates;
        public StaticMethodData[] OnEndOfFrames => _onEndOfFrames;

#if UNITY_EDITOR
        private class OnCompiled : IOnCompiled
        {
            static int Priority => 1000;
            static void onLoadOrRecompiled()
            {
                StaticMethodCache cache;
                if (NAssetUtils.hasObjectInMainResources(FILE_NAME_IN_RESOURCE))
                {
                    cache = NAssetUtils.getMainObjectInMainResources<StaticMethodCache>(FILE_NAME_IN_RESOURCE);
                }
                else
                {
                    cache = NEditorAssetUtils.createInMainResources<StaticMethodCache>(FILE_NAME_IN_RESOURCE);
                }
                if (cache == null)
                {
                    NDebug.LogError("[StaticMethodCache] Failed to create or load asset.");
                    return;
                }
                cache.scanAndCache();
            }
        }

        [ContextMenu("Scan and Cache Static Methods")]
        private void scanAndCache()
        {
            var types = NUtils.getCustomTypes();
            var bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

            var startupList = new List<(StaticMethodData, int)>();
            var quittingList = new List<(StaticMethodData, int)>();
            var updateList = new List<StaticMethodData>();
            var lateUpdateList = new List<StaticMethodData>();
            var endOfFrameList = new List<StaticMethodData>();

            foreach (var type in types)
            {
                if (type.ContainsGenericParameters) continue;

                var typeName = $"{type.FullName}, {type.Assembly.GetName().Name}";
                var methods = type.GetMethods(bindingFlags);

                foreach (var method in methods)
                {
                    if (method.GetParameters().Length > 0) continue;

                    var startupAttr = method.GetCustomAttribute<StartupMethodAttribute>();
                    if (startupAttr != null)
                    {
                        startupList.Add((new StaticMethodData(typeName, method.Name), startupAttr.priority));
                    }

                    var quittingAttr = method.GetCustomAttribute<QuittingMethodAttribute>();
                    if (quittingAttr != null)
                    {
                        quittingList.Add((new StaticMethodData(typeName, method.Name), quittingAttr.priority));
                    }

                    var loopAttr = method.GetCustomAttribute<LoopMethodAttribute>();
                    if (loopAttr != null)
                    {
                        var data = new StaticMethodData(typeName, method.Name);
                        switch (loopAttr.loopType)
                        {
                            case NLoopType.Update: updateList.Add(data); break;
                            case NLoopType.LateUpdate: lateUpdateList.Add(data); break;
                            case NLoopType.EndOfFrameUpdate: endOfFrameList.Add(data); break;
                        }
                    }
                }
            }

            static int compareMethod((StaticMethodData, int) a, (StaticMethodData, int) b)
            {
                var priorityComparison = b.Item2.CompareTo(a.Item2); // Descending order by priority
                if (priorityComparison != 0) return priorityComparison;
                return a.Item1.CompareTo(b.Item1);
            }

            // Sort startups/quittings by priority descending (higher priority first)
            startupList.Sort(compareMethod);
            quittingList.Sort(compareMethod);
            updateList.Sort();
            lateUpdateList.Sort();
            endOfFrameList.Sort();

            bool isDirty = false;

            var onStartups = toArray(startupList);
            var onQuittings = toArray(quittingList);
            var onUpdates = updateList.ToArray();
            var onLateUpdates = lateUpdateList.ToArray();
            var onEndOfFrames = endOfFrameList.ToArray();

            if (!onStartups.isSameItem(_onStartups))
            {
                _onStartups = onStartups;
                isDirty = true;
            }
            if (!onQuittings.isSameItem(_onQuittings))
            {
                _onQuittings = onQuittings;
                isDirty = true;
            }
            if (!onUpdates.isSameItem(_onUpdates))
            {
                _onUpdates = onUpdates;
                isDirty = true;
            }
            if (!onLateUpdates.isSameItem(_onLateUpdates)) 
            {
                _onLateUpdates = onLateUpdates;
                isDirty = true;
            }
            if (!onEndOfFrames.isSameItem(_onEndOfFrames))
            {
                _onEndOfFrames = onEndOfFrames;
                isDirty = true;
            }

            if (isDirty)
            {
                NAssetUtils.saveAsset(this);
                NDebug.Log($"[StaticMethodCache] Cached: {_onStartups.Length} startups, {_onQuittings.Length} quittings, " +
                       $"{_onUpdates.Length} updates, {_onLateUpdates.Length} lateUpdates, {_onEndOfFrames.Length} endOfFrames.");
            }
        }
#endif

        private static StaticMethodData[] toArray(List<(StaticMethodData, int)> list)
        {
            var arr = new StaticMethodData[list.Count];
            for (int i = 0; i < list.Count; i++) arr[i] = list[i].Item1;
            return arr;
        }
    }
}
