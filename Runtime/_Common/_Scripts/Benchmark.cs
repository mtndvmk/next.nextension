using System;
using System.Diagnostics;
using UnityEngine;

namespace Nextension
{
    public class Benchmark
    {
        public struct Result
        {
            public int runCount;
            public float avgTime => totalTime / runCount;
            public float maxTime;
            public float minTime;
            public float totalTime;

            public override string ToString()
            {
                return $"Avg: {avgTime}, Min: {minTime}, Max: {maxTime}, Run count: {runCount}";
            }
        }
        public static Result run(Action action, int runCount = 1)
        {
            Result result = new Result();
            result.runCount = runCount;
            result.minTime = float.MaxValue;
            result.maxTime = float.MinValue;
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
            return result;
        }
    }
}
