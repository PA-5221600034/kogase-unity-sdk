using System.Threading.Tasks;

namespace Kogase.Core
{
    internal static class HttpHelper
    {
        internal const int HttpRequestDefaultPriority = 2;
        internal const int HttpDelayOneFrameTimeMs = 25;

        internal static Task HttpDelayOneFrame => Task.Delay(HttpDelayOneFrameTimeMs);
    }
}