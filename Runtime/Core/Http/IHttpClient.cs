using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kogase.Core
{
    public interface IHttpClient
    {
        event Action<IHttpRequest> ServerErrorOccured;
        event Action<IHttpRequest> NetworkErrorOccured;
        IEnumerator SendRequest(IHttpRequest request, Action<IHttpResponse, Error> callback);
        Task<HttpSendResult> SendRequestAsync(IHttpRequest request);
        void AddAdditionalHeaderInfo(string headerKey, string headerValue);
        void SetImplicitPathParams(IDictionary<string, string> pathParams);
        void ClearImplicitPathParams();
        void SetBaseUri(Uri baseUrl);
        void ClearCookies();
    }
}