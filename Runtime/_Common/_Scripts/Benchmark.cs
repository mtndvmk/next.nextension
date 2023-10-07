using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Profiling;

namespace Nextension
{
    public class Benchmark
    {
        private static uint _count;
        public struct Result
        {
            public int runCount;
            public string name;
            public float avgTime => runCount == 0 ? 0 : (totalTime / runCount);
            public float maxTime;
            public float minTime;
            public float totalTime;

            public override string ToString()
            {
                return $"{name} --- Avg: {avgTime}, Min: {minTime}, Max: {maxTime} (ticks)\nTest count: {runCount}";
            }
        }
        public static Result run(Action action, int runCount = 1, string name = null)
        {
            if (runCount <= 0) runCount = 1;
            ++_count;
            Result result = new()
            {
                name = $"[Benchmark][{name ?? _count.ToString()}]",
                runCount = runCount,
                minTime = float.MaxValue,
                maxTime = float.MinValue
            };
            Profiler.BeginSample(result.name);
            Stopwatch stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < runCount; ++i)
            {
                stopwatch.Restart();
                action();
                var runTime = stopwatch.ElapsedTicks;
                result.totalTime += runTime;
                result.maxTime = Mathf.Max(result.maxTime, runTime);
                result.minTime = Mathf.Min(result.minTime, runTime);
            }
            stopwatch.Stop();
            Profiler.EndSample();
            return result;
        }
    }
}
