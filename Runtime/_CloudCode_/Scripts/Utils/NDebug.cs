using System;

namespace Nextension
{
    public enum NLogLevel
    {
        None = 0,
        Log = 1,
        Warning = 2,
        Error = 4,
        All = Log | Warning | Error
    }
    public interface INLogger
    {
        NLogLevel LogLevel { get; }
        void Log(object message);
        void LogWarning(object message);
        void LogError(object message);
        void LogException(Exception exception);

        void Log(object message, object context);
        void LogWarning(object message, object context);
        void LogError(object message, object context);
        void LogException(Exception exception, object context);
    }
    public static class NDebug
    {
        [ThreadStatic]
        private static INLogger _logger;

        public static void setLogger(INLogger logger)
        {
            _logger = logger;
        }

        public static bool IsEnable(NLogLevel logLevel)
        {
            if (_logger == null) return false;
            return ((int)_logger.LogLevel & (int)logLevel) != 0;
        }

        public static void Log(object message)
        {
            _logger?.Log(message);
        }

        public static void Log(object message, object context)
        {
            _logger?.Log(message, context);
        }

        public static void LogWarning(object message)
        {
            _logger?.LogWarning(message);
        }

        public static void LogWarning(object message, object context)
        {
            _logger?.LogWarning(message, context);
        }

        public static void LogError(object message)
        {
            _logger?.LogError(message);
        }

        public static void LogError(object message, object context)
        {
            _logger?.LogError(message, context);
        }

        public static void LogException(Exception exception)
        {
            _logger?.LogException(exception);
        }

        public static void LogException(Exception exception, object context)
        {
            _logger?.LogException(exception, context);
        }
    }
}
