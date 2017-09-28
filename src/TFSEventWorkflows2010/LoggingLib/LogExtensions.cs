using System;
using log4net;

namespace artiso.TFSEventWorkflows.LoggingLib
{
    public static class LogExtensions
    {
        private static ILog log;

        public static void LogError(this object source, string message, Exception exception)
        {
            var log = source.GetLoggerForObject();
            log.LogError(message, exception);
        }

        public static void LogError(this ILog log, string message, Exception exception)
        {
            if (log.IsErrorEnabled)
            {
                log.Error(message, exception);
            }
        }

        public static void LogInfo(this object source, string message)
        {
            source.LogInfo(message, null);
        }

        public static void LogInfo(this object source, string message, Exception exception)
        {
            var log = source.GetLoggerForObject();
            log.LogInfo(message, exception);
        }

        public static void LogInfo(this ILog log, string message, Exception exception)
        {
            if (log.IsInfoEnabled)
            {
                log.Info(message, exception);
            }
        }

        private static ILog GetLoggerForObject(this object source)
        {
            return LogManager.GetLogger(source.GetType());
        }
    }
}