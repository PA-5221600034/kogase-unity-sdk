using System;
using UnityEngine;
namespace Kogase.Core
{
    internal class LogHandler : ILogger
    {
        public void InvokeException(Exception exception, UnityEngine.Object context=null)
        {
            Debug.LogException(exception, context);
        }

        public void InvokeLog(LogType logType, object message, UnityEngine.Object context=null)
        {
            PrintLog(logType, message, context);
        }

        private void PrintLog(LogType logType, object message, UnityEngine.Object context)
        {
            string stackTrace = string.Empty;
            string time = DebugHelper.GetCurrentTimeString();
            LogFormat logFormat = DebugHelper.GetLogFormatting(logType.ToString(), time, message, stackTrace);
            string format = logFormat.Format;
            object[] messageParams = logFormat.Params;

            UnityEngine.LogType unityLogType = DebugHelper.ConvertKogaseLogTypeToUnityLogType(logType);
            switch (unityLogType)
            {
                case UnityEngine.LogType.Error:
                    if (context != null)
                    {
                        Debug.LogErrorFormat(format, messageParams, context);
                    }
                    else
                    {
                        Debug.LogErrorFormat(format, messageParams);
                    }
                    break;
                case UnityEngine.LogType.Assert:
                    if (context != null)
                    {
                        Debug.LogAssertionFormat(format, messageParams, context);
                    }
                    else
                    {
                        Debug.LogAssertionFormat(format, messageParams);
                    }
                    break;
                case UnityEngine.LogType.Warning:
                    if (context != null)
                    {
                        Debug.LogWarningFormat(format, messageParams, context);
                    }
                    else
                    {
                        Debug.LogWarningFormat(format, messageParams);
                    }
                    break;
                case UnityEngine.LogType.Log:
                    if (context != null)
                    {
                        Debug.LogFormat(format, messageParams, context);
                    }
                    else
                    {
                        Debug.LogFormat(format, messageParams);
                    }
                    break;
            }
        }
    }
}
