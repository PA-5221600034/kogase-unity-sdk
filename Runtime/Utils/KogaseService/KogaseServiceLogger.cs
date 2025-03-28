using Kogase.Core;

namespace Kogase.Utils
{
    internal class KogaseServiceLogger
    {
        public void LogServiceActivity(ServiceLog log, IDebugger logger)
        {
            logger?.LogEnhancedService($"LogKogaseServiceLoggingEvent: <--{log.ToJsonString()}-->");
        }
    }
}