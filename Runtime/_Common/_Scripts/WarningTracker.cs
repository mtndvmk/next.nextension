using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Nextension
{
    public enum WarningType
    {
        Warning,
        Error,
    }
    public static class WarningTracker
    {
        private static Dictionary<string, int> _tracks = new();
        public static void trackInfo(string message, int numberTrackThreshold = 2, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            if (numberTrackThreshold < 2)
            {
                Debug.Log(message);
            }
            else
            {
                var key = $"I|{Path.GetFileName(filePath)}|{lineNumber}|{message.GetHashCode()}";
                if (!_tracks.ContainsKey(key))
                {
                    _tracks.Add(key, 1);
                }
                else if (++_tracks[key] >= numberTrackThreshold)
                {
                    _tracks.Remove(key);
                    Debug.LogWarning(message);
                }
            }
        }
        public static void trackWarning(string message, int numberTrackThreshold = 2, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            if (numberTrackThreshold < 2)
            {
                Debug.LogWarning(message);
            }
            else
            {
                var key = $"W|{Path.GetFileName(filePath)}|{lineNumber}|{message.GetHashCode()}";
                if (!_tracks.ContainsKey(key))
                {
                    _tracks.Add(key, 1);
                }
                else if (++_tracks[key] >= numberTrackThreshold)
                {
                    _tracks.Remove(key);
                    Debug.LogWarning(message);
                }
            }
        }
        public static void trackError(string message, int numberTrackThreshold = 2, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            if (numberTrackThreshold < 2)
            {
                Debug.LogError(message);
            }
            else
            {
                var key = $"E|{Path.GetFileName(filePath)}|{lineNumber}|{message.GetHashCode()}";
                if (!_tracks.ContainsKey(key))
                {
                    _tracks.Add(key, 1);
                }
                else if (++_tracks[key] >= numberTrackThreshold)
                {
                    _tracks.Remove(key);
                    Debug.LogError(message);
                }
            }
        }
    }
}