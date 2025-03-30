using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kogase.Core
{
    internal class KogaseHttpClient : IHttpClient
    {
        public event Action<IHttpRequest> ServerErrorOccurred;
        public event Action<IHttpRequest> NetworkErrorOccurred;

        readonly Dictionary<string, string> additionalHeaders = new();
        readonly IHttpSender sender;
        Uri baseUri;
        string bearerToken;

        const int DefaultTimeoutMs = 30000; // 30 seconds
        const uint DefaultDelayMs = 0;
        const uint MaxRetries = 3;

        public KogaseHttpClient(IHttpSender sender = null)
        {
            this.sender = sender ?? new KogaseHttpSender();
        }

        public IEnumerator SendRequest(IHttpRequest request, Action<IHttpResponse, Error> callback)
        {
            var processedRequest = ProcessRequest(request);

            HttpResult result = default;
            var isDone = false;

            sender.AddTask(
                processedRequest,
                sendResult =>
                {
                    result = sendResult;
                    isDone = true;

                    // Handle network errors
                    if (result.CallbackError != null && result.CallbackError.Code == Code.NETWORK_ERROR)
                        NetworkErrorOccurred?.Invoke(processedRequest);

                    // Handle server errors
                    if (result.CallbackResponse != null && result.CallbackResponse.Code >= 500)
                        ServerErrorOccurred?.Invoke(processedRequest);
                },
                DefaultTimeoutMs,
                DefaultDelayMs
            );

            while (!isDone) yield return null;

            callback?.Invoke(result.CallbackResponse, result.CallbackError);
        }

        public async Task<HttpResult> SendRequestAsync(IHttpRequest request)
        {
            var processedRequest = ProcessRequest(request);

            var tcs = new TaskCompletionSource<HttpResult>();

            sender.AddTask(
                processedRequest,
                result =>
                {
                    // Handle network errors
                    if (result.CallbackError != null && result.CallbackError.Code == Code.NETWORK_ERROR)
                        NetworkErrorOccurred?.Invoke(processedRequest);

                    // Handle server errors
                    if (result.CallbackResponse != null && result.CallbackResponse.Code >= 500)
                        ServerErrorOccurred?.Invoke(processedRequest);

                    tcs.SetResult(result);
                },
                DefaultTimeoutMs,
                DefaultDelayMs
            );

            return await tcs.Task;
        }

        public void SetBearerAuth(string inBearerToken)
        {
            bearerToken = inBearerToken;
        }

        public void AddAdditionalHeader(string headerKey, string headerValue)
        {
            additionalHeaders[headerKey] = headerValue;
        }

        public void SetBaseUri(Uri inBaseUri)
        {
            baseUri = inBaseUri;
        }

        public void ClearCookies()
        {
            sender.ClearCookies(baseUri);
        }

        IHttpRequest ProcessRequest(IHttpRequest request)
        {
            // Apply base URI if the request URL is relative
            if (baseUri != null && !Uri.IsWellFormedUriString(request.Url, UriKind.Absolute))
            {
                var relativeUri = request.Url.StartsWith("/") ? request.Url.Substring(1) : request.Url;
                var combinedUri = new Uri(baseUri, relativeUri);
                request.Url = combinedUri.ToString();
            }

            // Add bearer token if requested
            if (request.AuthType == HttpAuthType.BEARER && !string.IsNullOrEmpty(bearerToken) &&
                !request.Headers.ContainsKey("Authorization"))
                ((Dictionary<string, string>)request.Headers)["Authorization"] = "Bearer " + bearerToken;

            // Add additional headers
            foreach (var header in additionalHeaders)
                if (!request.Headers.ContainsKey(header.Key))
                    ((Dictionary<string, string>)request.Headers)[header.Key] = header.Value;

            return request;
        }
    }
}