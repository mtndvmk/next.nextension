using System;
using UnityEngine;

namespace Nextension
{
    public class NUnityDebug : INLogger
    {
        [StartupMethod]
        private static void installLogger()
        {
            NDebug.setLogger(new NUnityDebug());
        }

        public NLogLevel LogLevel => NLogLevel.All;

        public void Log(object message)
        {
            Debug.Log(message);
        }

        public void Log(object message, object context)
        {
            Debug.Log(message, context as UnityEngine.Object);
        }

        public void LogError(object message)
        {
            Debug.LogError(message);
        }

        public void LogError(object message, object context)
        {
            Debug.LogError(message, context as UnityEngine.Object);
        }

        public void LogException(Exception exception)
        {
            Debug.LogException(exception);
        }

        public void LogException(Exception exception, object context)
        {
            Debug.LogException(exception, context as UnityEngine.Object);
        }

        public void LogWarning(object message)
        {
            Debug.LogWarning(message);
        }

        public void LogWarning(object message, object context)
        {
            Debug.LogWarning(message, context as UnityEngine.Object);
        }
    }
}
