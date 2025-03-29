using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Kogase.Core
{
    public class KogaseHttpClient : IHttpClient
    {
        public event Action<IHttpRequest> ServerErrorOccurred;
        public event Action<IHttpRequest> NetworkErrorOccurred;

        private readonly Dictionary<string, string> additionalHeaders = new Dictionary<string, string>();
        private readonly IHttpRequestSender requestSender;
        private Uri baseUri;
        private string bearerToken;
        
        private const int DefaultTimeoutMs = 30000; // 30 seconds
        private const uint DefaultDelayMs = 0;
        private const uint MaxRetries = 3;

        public KogaseHttpClient(IHttpRequestSender requestSender = null)
        {
            this.requestSender = requestSender ?? new KogaseHttpRequestSender();
        }

        public IEnumerator SendRequest(IHttpRequest request, Action<IHttpResponse, Error> callback)
        {
            IHttpRequest processedRequest = ProcessRequest(request);
            
            HttpSendResult result = default;
            bool isDone = false;
            
            requestSender.AddTask(
                processedRequest, 
                sendResult => 
                {
                    result = sendResult;
                    isDone = true;
                    
                    // Handle network errors
                    if (result.CallbackError != null && result.CallbackError.Code == Code.NETWORK_ERROR)
                    {
                        NetworkErrorOccurred?.Invoke(processedRequest);
                    }
                    
                    // Handle server errors
                    if (result.CallbackResponse != null && result.CallbackResponse.Code >= 500)
                    {
                        ServerErrorOccurred?.Invoke(processedRequest);
                    }
                }, 
                DefaultTimeoutMs, 
                DefaultDelayMs
            );
            
            while (!isDone)
            {
                yield return null;
            }
            
            callback?.Invoke(result.CallbackResponse, result.CallbackError);
        }

        public async Task<HttpSendResult> SendRequestAsync(IHttpRequest request)
        {
            IHttpRequest processedRequest = ProcessRequest(request);
            
            TaskCompletionSource<HttpSendResult> tcs = new TaskCompletionSource<HttpSendResult>();
            
            requestSender.AddTask(
                processedRequest, 
                result => 
                {
                    // Handle network errors
                    if (result.CallbackError != null && result.CallbackError.Code == Code.NETWORK_ERROR)
                    {
                        NetworkErrorOccurred?.Invoke(processedRequest);
                    }
                    
                    // Handle server errors
                    if (result.CallbackResponse != null && result.CallbackResponse.Code >= 500)
                    {
                        ServerErrorOccurred?.Invoke(processedRequest);
                    }
                    
                    tcs.SetResult(result);
                }, 
                DefaultTimeoutMs, 
                DefaultDelayMs
            );
            
            return await tcs.Task;
        }

        public void SetBearerAuth(string inBearerToken)
        {
            this.bearerToken = inBearerToken;
        }

        public void AddAdditionalHeader(string headerKey, string headerValue)
        {
            additionalHeaders[headerKey] = headerValue;
        }

        public void SetBaseUri(Uri inBaseUri)
        {
            this.baseUri = inBaseUri;
        }

        public void ClearCookies()
        {
            requestSender.ClearCookies(baseUri);
        }

        private IHttpRequest ProcessRequest(IHttpRequest request)
        {
            // Apply base URI if the request URL is relative
            if (baseUri != null && !Uri.IsWellFormedUriString(request.Url, UriKind.Absolute))
            {
                var relativeUri = request.Url.StartsWith("/") ? request.Url.Substring(1) : request.Url;
                Uri combinedUri = new Uri(baseUri, relativeUri);
                request.Url = combinedUri.ToString();
            }

            // Add bearer token if requested
            if (request.AuthType == HttpAuth.BEARER && !string.IsNullOrEmpty(bearerToken) && 
                !request.Headers.ContainsKey("Authorization"))
            {
                ((Dictionary<string, string>)request.Headers)["Authorization"] = "Bearer " + bearerToken;
            }

            // Add additional headers
            foreach (var header in additionalHeaders)
            {
                if (!request.Headers.ContainsKey(header.Key))
                {
                    ((Dictionary<string, string>)request.Headers)[header.Key] = header.Value;
                }
            }

            return request;
        }
    }

    internal class KogaseHttpRequestSender : IHttpRequestSender
    {
        private readonly WebRequestScheduler scheduler;

        public KogaseHttpRequestSender(WebRequestScheduler scheduler = null)
        {
            this.scheduler = scheduler ?? new WebRequestScheduler();
        }

        public void AddTask(IHttpRequest request, Action<HttpSendResult> callback, int timeoutMs, uint delayTimeMs)
        {
            WebRequestTask task = new WebRequestTask(request, timeoutMs, delayTimeMs)
            {
                OnComplete = webResponse =>
                {
                    HttpSendResult result = ParseWebRequestResult(webResponse);
                    callback?.Invoke(result);
                }
            };

            scheduler.AddTask(task);
        }

        public void ClearTasks()
        {
            scheduler.ClearTasks();
        }

        public void ClearCookies(Uri baseUri)
        {
            if (baseUri != null)
            {
                UnityWebRequest.ClearCookieCache(baseUri);
            }
        }

        private HttpSendResult ParseWebRequestResult(WebResponse webResponse)
        {
            IHttpResponse response = null;
            Error error = null;

            if (webResponse?.WebRequest == null)
            {
                error = new Error(Code.NETWORK_ERROR, "No response received");
                return new HttpSendResult(null, error);
            }

            var unityWebRequest = webResponse.WebRequest;

            #if UNITY_2020_1_OR_NEWER
            switch (unityWebRequest.result)
            {
                case UnityWebRequest.Result.Success:
                    response = webResponse.GetHttpResponse();
                    break;
                case UnityWebRequest.Result.ConnectionError:
                    error = new Error(Code.NETWORK_ERROR, unityWebRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    response = webResponse.GetHttpResponse();
                    break;
                case UnityWebRequest.Result.DataProcessingError:
                    error = new Error(Code.INVALID_RESPONSE, unityWebRequest.error);
                    break;
            }
            #else
            if (unityWebRequest.isNetworkError)
            {
                error = new Error(Code.NETWORK_ERROR, unityWebRequest.error);
            }
            else
            {
                response = webResponse.GetHttpResponse();
            }
            #endif

            return new HttpSendResult(response, error);
        }
    }
} 