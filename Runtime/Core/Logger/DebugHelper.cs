using System;
using System.Collections.Generic;

namespace Kogase.Core
{
    public static class DebugHelper
    {
        internal const string DefaultLogFormat = "[{Severity}][<b>KogaseSDK</b>][{Time}]{Message}[/{Severity}]";
        internal const int SeverityFormatIndex = 0;
        internal const int TimeFormatIndex = 1;
        internal const int MessageFormatIndex = 2;
        internal const int StackTraceFormatIndex = 3;
        static string _logFormat = DefaultLogFormat;

        internal static void SetLogFormatting(string newFormat)
        {
            _logFormat = newFormat;
        }

        internal static LogFormat GetLogFormatting(string severity, string time, object message, string stackTrace)
        {
            var stringParams = new List<object>();
            var format = _logFormat;
            if (_logFormat.Contains("{Severity}"))
            {
                format = format.Replace("{Severity}", "{" + SeverityFormatIndex + "}");
                stringParams.Add(severity);
            }

            if (_logFormat.Contains("{Time}"))
            {
                format = format.Replace("{Time}", "{" + TimeFormatIndex + "}");
                stringParams.Add(time);
            }

            if (_logFormat.Contains("{Message}"))
            {
                format = format.Replace("{Message}", "{" + MessageFormatIndex + "}");
                stringParams.Add(message);
            }

            if (_logFormat.Contains("{StackTrace}"))
            {
                format = format.Replace("{StackTrace}", "{" + StackTraceFormatIndex + "}");
                stringParams.Add(stackTrace);
            }

            var retval = new LogFormat()
            {
                Format = format,
                Params = stringParams.ToArray()
            };
            return retval;
        }

        internal static UnityEngine.LogType ConvertKogaseLogTypeToUnityLogType(LogType logType)
        {
            if (!Enum.TryParse(logType.ToString(), true, out UnityEngine.LogType unityLogType))
                unityLogType = UnityEngine.LogType.Log;
            return unityLogType;
        }

        internal static string GetCurrentTimeString()
        {
            var time = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            return time;
        }
    }
}