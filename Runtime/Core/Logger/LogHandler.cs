using System;
using UnityEngine;

namespace Kogase.Core
{
    internal class LogHandler : ILogger
    {
        public void InvokeException(Exception exception, UnityEngine.Object context = null)
        {
            Debug.LogException(exception, context);
        }

        public void InvokeLog(LogType logType, object message, UnityEngine.Object context = null)
        {
            PrintLog(logType, message, context);
        }

        void PrintLog(LogType logType, object message, UnityEngine.Object context)
        {
            var stackTrace = string.Empty;
            var time = DebugHelper.GetCurrentTimeString();
            var logFormat = DebugHelper.GetLogFormatting(logType.ToString(), time, message, stackTrace);
            var format = logFormat.Format;
            var messageParams = logFormat.Params;

            var unityLogType = DebugHelper.ConvertKogaseLogTypeToUnityLogType(logType);
            switch (unityLogType)
            {
                case UnityEngine.LogType.Error:
                    if (context != null)
                        Debug.LogErrorFormat(format, messageParams, context);
                    else
                        Debug.LogErrorFormat(format, messageParams);
                    break;
                case UnityEngine.LogType.Assert:
                    if (context != null)
                        Debug.LogAssertionFormat(format, messageParams, context);
                    else
                        Debug.LogAssertionFormat(format, messageParams);
                    break;
                case UnityEngine.LogType.Warning:
                    if (context != null)
                        Debug.LogWarningFormat(format, messageParams, context);
                    else
                        Debug.LogWarningFormat(format, messageParams);
                    break;
                case UnityEngine.LogType.Log:
                    if (context != null)
                        Debug.LogFormat(format, messageParams, context);
                    else
                        Debug.LogFormat(format, messageParams);
                    break;
            }
        }
    }
}