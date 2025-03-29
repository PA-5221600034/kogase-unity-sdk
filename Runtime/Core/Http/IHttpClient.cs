using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Kogase.Core
{
    public enum HttpAuth
    {
        NONE,
        BASIC,
        BEARER
    }

    public interface IHttpRequest
    {
        string Id { get; set; }
        string Method { get; }
        string Url { get; set; }
        HttpAuth AuthType { get; }
        IDictionary<string, string> Headers { get; }
        byte[] BodyBytes { get; }
        int Priority { get; set; }
    }

    public interface IHttpResponse 
    {
        string Url { get; }
        long Code { get; }
        IDictionary<string, string> Headers { get; }
        byte[] BodyBytes { get; }
    }

    public struct HttpSendResult
    {
        public readonly IHttpResponse CallbackResponse;
        public readonly Error CallbackError;

        public HttpSendResult(IHttpResponse callbackResponse, Error callbackError)
        {
            CallbackResponse = callbackResponse;
            CallbackError = callbackError;
        }
    }
    
    public interface IHttpClient
    {
        event Action<IHttpRequest> ServerErrorOccurred;
        event Action<IHttpRequest> NetworkErrorOccurred;
        
        IEnumerator SendRequest(IHttpRequest request, Action<IHttpResponse, Error> callback);
        Task<HttpSendResult> SendRequestAsync(IHttpRequest request);
        
        void SetBearerAuth(string inBearerToken);
        void AddAdditionalHeader(string headerKey, string headerValue);
        void SetBaseUri(Uri inBaseUri);
        void ClearCookies();
    }

    public interface IHttpRequestSender
    {
        void AddTask(IHttpRequest request, Action<HttpSendResult> callback, int timeoutMs, uint delayTimeMs);
        void ClearTasks();
        void ClearCookies(Uri baseUri);
    }

    public static class HttpClientExtension
    {
        public static IEnumerator SendRequest(this IHttpClient client, IHttpRequest request, Action<IHttpResponse> callback)
        {
            return client.SendRequest(request, (response, err) => callback?.Invoke(response));
        }
        
        public static async Task<IHttpResponse> SendRequestAsync(this IHttpClient client, IHttpRequest request)
        {
            var sendTask = client.SendRequestAsync(request);
            HttpSendResult result = await sendTask;
            return result.CallbackResponse;
        }
    }
} 