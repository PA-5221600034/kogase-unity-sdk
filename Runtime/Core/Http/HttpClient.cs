using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
namespace Kogase.Core.Http
{
    /// <summary>
    /// Client for executing HTTP requests
    /// </summary>
    public class HttpClient
    {
        MonoBehaviour coroutineRunner;

        /// <summary>
        /// Creates a new HTTP client
        /// </summary>
        /// <param name="coroutineRunner">MonoBehaviour to run coroutines on</param>
        public HttpClient(MonoBehaviour coroutineRunner = null)
        {
            this.coroutineRunner = coroutineRunner;
        }

        /// <summary>
        /// Sets the MonoBehaviour to run coroutines on
        /// </summary>
        public void SetCoroutineRunner(MonoBehaviour coroutineRunner)
        {
            this.coroutineRunner = coroutineRunner;
        }

        /// <summary>
        /// Sends an HTTP request asynchronously
        /// </summary>
        /// <param name="request">The request to send</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task<HttpResponse> SendAsync(IHttpRequest request)
        {
            UnityWebRequest webRequest = request.CreateWebRequest();

            var operation = webRequest.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            var response = new HttpResponse
            {
                IsSuccess = webRequest.result != UnityWebRequest.Result.ConnectionError && webRequest.result != UnityWebRequest.Result.ProtocolError,
                StatusCode = (int)webRequest.responseCode,
                Content = webRequest.downloadHandler?.text,
                Error = webRequest.error,
                Data = webRequest.downloadHandler?.data
            };

            webRequest.Dispose();
            return response;
        }

        /// <summary>
        /// Sends an HTTP request using a coroutine
        /// </summary>
        /// <param name="request">The request to send</param>
        /// <param name="callback">Callback to receive the response</param>
        public void Send(IHttpRequest request, Action<HttpResponse> callback)
        {
            if (coroutineRunner == null)
            {
                Debug.LogError("Cannot send request using coroutines without a coroutine runner");
                callback?.Invoke(new HttpResponse
                {
                    IsSuccess = false,
                    Error = "No coroutine runner set for HttpClient"
                });
                return;
            }

            coroutineRunner.StartCoroutine(SendCoroutine(request, callback));
        }

        IEnumerator SendCoroutine(IHttpRequest request, Action<HttpResponse> callback)
        {
            UnityWebRequest webRequest = request.CreateWebRequest();

            yield return webRequest.SendWebRequest();

            var response = new HttpResponse
            {
                IsSuccess = webRequest.result != UnityWebRequest.Result.ConnectionError && webRequest.result != UnityWebRequest.Result.ProtocolError,
                StatusCode = (int)webRequest.responseCode,
                Content = webRequest.downloadHandler?.text,
                Error = webRequest.error,
                Data = webRequest.downloadHandler?.data
            };

            webRequest.Dispose();
            callback?.Invoke(response);
        }
    }

    /// <summary>
    /// Represents an HTTP response
    /// </summary>
    public class HttpResponse
    {
        /// <summary>
        /// Whether the request was successful
        /// </summary>
        public bool IsSuccess { get; set; }
        
        /// <summary>
        /// The HTTP status code
        /// </summary>
        public int StatusCode { get; set; }
        
        /// <summary>
        /// The response content as a string
        /// </summary>
        public string Content { get; set; }
        
        /// <summary>
        /// Any error message
        /// </summary>
        public string Error { get; set; }
        
        /// <summary>
        /// The raw response data
        /// </summary>
        public byte[] Data { get; set; }
    }
} 