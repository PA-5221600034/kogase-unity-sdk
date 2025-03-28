using System;

namespace Kogase.Core
{
    public interface IHttpRequestSender
    {
        void AddTask(
            IHttpRequest request,
            Action<HttpSendResult> callback,
            int timeoutMs,
            uint delayTimeMs);

        void ClearTasks();

        void ClearCookies(Uri baseUri);
    }
}