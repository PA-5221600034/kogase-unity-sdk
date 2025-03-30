using System;
using System.Collections;
using System.Threading.Tasks;

namespace Kogase.Core
{
    public interface IHttpClient
    {
        event Action<IHttpRequest> ServerErrorOccurred;
        event Action<IHttpRequest> NetworkErrorOccurred;

        IEnumerator SendRequest(IHttpRequest request, Action<IHttpResponse, Error> callback);
        Task<HttpResult> SendRequestAsync(IHttpRequest request);

        void SetBearerAuth(string inBearerToken);
        void AddAdditionalHeader(string headerKey, string headerValue);
        void SetBaseUri(Uri inBaseUri);
        void ClearCookies();
    }
}