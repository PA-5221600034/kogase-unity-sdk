using System;
using System.Collections;
using System.Threading.Tasks;

namespace Kogase.Core
{
    public static class HttpClientExtension
    {
        public static IEnumerator SendRequest(
            this IHttpClient client,
            IHttpRequest request,
            Action<IHttpResponse> callback)
        {
            return client.SendRequest(request, (response, err) => callback?.Invoke(response));
        }

        public static async Task<IHttpResponse> SendRequestAsync(
            this IHttpClient client,
            IHttpRequest request)
        {
            var sendTask = client.SendRequestAsync(request);
            var result = await sendTask;
            return result.CallbackResponse;
        }
    }
}