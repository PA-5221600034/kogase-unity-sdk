using System;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace Kogase.Core
{
    internal class WebRequestTask
    {
        public readonly int TimeoutMs;
        public IHttpRequest HttpRequest { get; }
        public Action<WebResponse> OnComplete;
        public uint DelayMs { get; }
        public int Priority => HttpRequest.Priority;
        public DateTime CreatedTimeStamp { get; }
        public WebRequestState State { get; private set; }

        public WebRequestTask(IHttpRequest httpRequest, int timeoutMs, uint delayMs)
        {
            HttpRequest = httpRequest;
            TimeoutMs = timeoutMs;
            DelayMs = delayMs;
            CreatedTimeStamp = DateTime.UtcNow;
            SetState(WebRequestState.WAITING);
        }

        public void SetComplete(WebResponse webRequestResult)
        {
            SetState(WebRequestState.COMPLETE);
            OnComplete?.Invoke(webRequestResult);
        }

        public void SetState(WebRequestState newState)
        {
            State = newState;
        }

        public UnityWebRequest CreateWebRequest()
        {
            UnityWebRequest unityWebRequest = new UnityWebRequest(HttpRequest.Url, HttpRequest.Method);
            
            // Add headers
            foreach (var header in HttpRequest.Headers)
            {
                unityWebRequest.SetRequestHeader(header.Key, header.Value);
            }
            
            // Add body if present
            if (HttpRequest.BodyBytes != null && HttpRequest.BodyBytes.Length > 0)
            {
                unityWebRequest.uploadHandler = new UploadHandlerRaw(HttpRequest.BodyBytes);
            }
            
            // Set download handler
            unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
            
            return unityWebRequest;
        }
    }

    internal class WebRequestTaskOrderComparer : IComparer<WebRequestTask>
    {
        public int Compare(WebRequestTask task1, WebRequestTask task2)
        {
            if (task1 == null || task2 == null)
            {
                return 0;
            }

            if (task1.Priority < task2.Priority)
            {
                return -1;
            }

            if (task1.Priority > task2.Priority)
            {
                return 1;
            }

            if (task1.CreatedTimeStamp < task2.CreatedTimeStamp)
            {
                return -1;
            }

            if (task1.CreatedTimeStamp > task2.CreatedTimeStamp)
            {
                return 1;
            }

            return 0;
        }
    }

    internal enum WebRequestState
    {
        WAITING,
        ON_PROCESS,
        COMPLETE
    }

    internal class WebResponse
    {
        public readonly UnityWebRequest WebRequest;
        public readonly DateTime SentTimestamp;
        public readonly DateTime ResponseTimestamp;

        public WebResponse(UnityWebRequest webRequest, DateTime sentTimestamp, DateTime responseTimestamp)
        {
            WebRequest = webRequest;
            SentTimestamp = sentTimestamp;
            ResponseTimestamp = responseTimestamp;
        }

        public IHttpResponse GetHttpResponse()
        {
            return new HttpResponseImpl
            {
                Url = WebRequest.url,
                Code = WebRequest.responseCode,
                Headers = GetResponseHeaders(WebRequest),
                BodyBytes = WebRequest.downloadHandler.data
            };
        }

        private Dictionary<string, string> GetResponseHeaders(UnityWebRequest request)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            
            if (request.GetResponseHeaders() != null)
            {
                foreach (var header in request.GetResponseHeaders())
                {
                    result[header.Key] = header.Value;
                }
            }
            
            return result;
        }

        private class HttpResponseImpl : IHttpResponse
        {
            public string Url { get; set; }
            public long Code { get; set; }
            public IDictionary<string, string> Headers { get; set; }
            public byte[] BodyBytes { get; set; }
        }
    }
} 