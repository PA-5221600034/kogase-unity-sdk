using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
namespace Kogase.Core
{
    public class Debugger : IDebugger
    {
        private List<ILogger> loggers = new List<ILogger>();

        internal LogType currentSeverity;

        private Action<LogType, object, Object> onLog;

        private Action<Exception, Object> onException;

        internal ILogger DefaultLogger
        {
            get;
            private set;
        }

        internal bool EnhancedLogEnabled
        {
            get;
            private set;
        }

        internal bool LogEnabled
        {
            get;
            private set;
        }

        internal Debugger()
        {
            DefaultLogger = new LogHandler();
            AddLogWriter(DefaultLogger);

            LogEnabled = false;
        }

        internal Debugger(bool enableLog, LogType logFilter)
        {
            DefaultLogger = new LogHandler();
            AddLogWriter(DefaultLogger);

            SetEnableLogging(enableLog);
            SetFilterLogType(logFilter);
        }

        public bool IsEnhancedLoggingEnabled()
        {
            return EnhancedLogEnabled;
        }

    public void SetEnableEnhancedLogging(bool enable)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR 
            EnhancedLogEnabled = enable;
#endif
        }

        public void SetEnableLogging(bool enable)
        {
            LogEnabled = enable;
        }

        public void SetFilterLogType(LogType type)
        {
            currentSeverity = type;
        }
        
        public void LogEnhancedService(object message)
        {
            if (!EnhancedLogEnabled)
            {
                return;
            }
            onLog?.Invoke(LogType.VERBOSE, message, null);
        }

        public void LogVerbose(object message, bool forceLog = false)
        {
            InvokeLog(LogType.VERBOSE, message, forceLog);
        }

        public void LogVerbose(object message, Object context, bool forceLog = false)
        {
            InvokeLog(LogType.VERBOSE, message, forceLog, context);
        }

        public void Log(object message, bool forceLog = false)
        {
            InvokeLog(LogType.LOG, message, forceLog);
        }

        public void Log(object message, Object context, bool forceLog = false)
        {
            InvokeLog(LogType.LOG, message, forceLog, context);
        }

        public void LogWarning(object message, bool forceLog = false)
        {
            InvokeLog(LogType.WARNING, message, forceLog);
        }

        public void LogWarning(object message, Object context, bool forceLog = false)
        {
            InvokeLog(LogType.WARNING, message, forceLog, context);
        }

        public void LogError(object message, bool forceLog = false)
        {
            InvokeLog(LogType.ERROR, message, forceLog);
        }

        public void LogError(object message, Object context, bool forceLog = false)
        {
            InvokeLog(LogType.ERROR, message, forceLog, context);
        }

        public void LogException(Exception exception, Object context = null)
        {
            if (onException == null || !FilterLogSeverity(currentSeverity, LogType.EXCEPTION) || !LogEnabled)
            {
                return;
            }
            onException?.Invoke(exception, context);
        }

        internal void SetLogWriter(ILogger newLogger = null)
        {
            if (newLogger != null)
            {
                SetLogWriters(new ILogger[] { newLogger });
            }
            else
            {
                ClearLoggers();
            }
        }

        internal void SetLogWriters(ILogger[] newLoggers)
        {
            ClearLoggers();
            AddLogWriters(newLoggers);
        }

        internal void AddLogWriter(ILogger newLogger)
        {
            AddLogWriters(new ILogger[] { newLogger });
        }

        internal void AddLogWriters(ILogger[] newLoggers)
        {
            foreach (var logger in newLoggers)
            {
                if (!loggers.Contains(logger))
                {
                    onLog += logger.InvokeLog;
                    onException += logger.InvokeException;
                    loggers.Add(logger);
                }
            }
        }

        internal void RemoveLogger(ILogger loggerToRemove)
        {
            RemoveLoggers(new ILogger[] { loggerToRemove });
        }

        internal void RemoveLoggers(ILogger[] loggersToRemove)
        {
            foreach (var logger in loggersToRemove)
            {
                if (loggers.Contains(logger))
                {
                    onLog -= logger.InvokeLog;
                    onException -= logger.InvokeException;
                    loggers.Remove(logger);
                }
            }
        }

        internal ILogger[] GetLoggers()
        {
            return loggers.ToArray();
        }

        internal void SetFilterLogType(UnityEngine.LogType type)
        {
            string unityLogTypeStr = type.ToString();
            if (!Enum.TryParse(unityLogTypeStr, true, out currentSeverity))
            {
                currentSeverity = LogType.LOG;
                throw new System.InvalidOperationException($"Failed assign Unity log severity type : {type}.");
            }
        }

        private void ClearLoggers()
        {
            onLog = null;
            onException = null;
            loggers.Clear();
        }

        // LogType level
        //  - Log (default setting) will display all log messages.
        //  - Warning will display warning, assert, error and exception log messages.
        //  - Assert will display assert, error and exception log messages.
        //  - Error will display error and exception log messages.
        //  - Exception will display exception log messages.
        private bool FilterLogSeverity(LogType activeSeverity, LogType logSeverity)
        {
            return logSeverity <= activeSeverity;
        }

        private void InvokeLog(LogType logType, object message, bool forceLog, Object context = null)
        {
            bool isLogAccepted = FilterLogSeverity(currentSeverity, logType) || forceLog;
            if (onLog == null || !isLogAccepted || !LogEnabled)
            {
                return;
            }
            onLog?.Invoke(logType, message, context);
        }
    }
}
