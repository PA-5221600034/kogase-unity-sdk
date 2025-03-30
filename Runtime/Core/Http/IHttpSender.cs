using System;

namespace Kogase.Core
{
    internal interface IHttpSender
    {
        void AddTask(IHttpRequest request, Action<HttpResult> callback, int timeoutMs, uint delayTimeMs);
        void ClearTasks();
        void ClearCookies(Uri baseUri);
    }
}