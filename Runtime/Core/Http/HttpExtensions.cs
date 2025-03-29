using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Kogase.Core
{
    public static class HttpExtensions
    {
        /// <summary>
        /// Send a custom request
        /// </summary>
        public static IEnumerator Send<TResponse>(this IHttpClient client, IHttpRequest request, 
            OkDelegate<TResponse> okCallback, ErrorDelegate<Error> errorCallback)
        {
            return client.SendRequest(request, (response, error) =>
            {
                if (error != null)
                {
                    errorCallback?.Invoke(error);
                    return;
                }
                
                HttpParser.ParseResponse(response, okCallback, errorCallback);
            });
        }
    }
} 