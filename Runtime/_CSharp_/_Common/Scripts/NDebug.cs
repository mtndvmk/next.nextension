namespace Nextension
{
    public static class NDebug
    {
        public static void Log(object message)
        {
#if UNITY_5_3_OR_NEWER
            UnityEngine.Debug.Log(message);
#else
            System.Diagnostics.Debug.WriteLine(message);
#endif
        }

        public static void Log(object message, object context)
        {
#if UNITY_5_3_OR_NEWER
            UnityEngine.Debug.Log(message, (UnityEngine.Object)context);
#else
            System.Diagnostics.Debug.WriteLine(message);
#endif
        }

        public static void LogWarning(object message)
        {
#if UNITY_5_3_OR_NEWER
            UnityEngine.Debug.LogWarning(message);
#else
            System.Diagnostics.Debug.WriteLine($"Warning: {message}");
#endif
        }

        public static void LogWarning(object message, object context)
        {
#if UNITY_5_3_OR_NEWER
            UnityEngine.Debug.LogWarning(message, (UnityEngine.Object)context);
#else
            System.Diagnostics.Debug.WriteLine($"Warning: {message}");
#endif
        }

        public static void LogError(object message)
        {
#if UNITY_5_3_OR_NEWER
            UnityEngine.Debug.LogError(message);
#else
            System.Diagnostics.Debug.WriteLine($"Error: {message}");
#endif
        }

        public static void LogError(object message, object context)
        {
#if UNITY_5_3_OR_NEWER
            UnityEngine.Debug.LogError(message, (UnityEngine.Object)context);
#else
            System.Diagnostics.Debug.WriteLine($"Error: {message}");
#endif
        }

        public static void LogException(System.Exception exception)
        {
#if UNITY_5_3_OR_NEWER
            UnityEngine.Debug.LogException(exception);
#else
            System.Diagnostics.Debug.WriteLine($"Exception: {exception}");
#endif
        }

        public static void LogException(System.Exception exception, object context)
        {
#if UNITY_5_3_OR_NEWER
            UnityEngine.Debug.LogException(exception, (UnityEngine.Object)context);
#else
            System.Diagnostics.Debug.WriteLine($"Exception: {exception}");
#endif
        }
    }
}
